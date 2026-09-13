using Game.Audio;
using Game.Common;
using Game.Overworld;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Game.Scene
{

    public class MainSceneController : Common.Singleton<MainSceneController>
    {
        [Header("Map generation")]      
        [SerializeField] private List<MapDefinition> actDefinitions;
        [SerializeField] private int seed = 12345;        

        [Header("Loading Screen")]
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Loading Screen")]
        [SerializeField] private AudioClip mainMenuMusic;

        [Header("Other")]
        [SerializeField] private GameObject startingCamera;
        [Tooltip("If enabled it captures number of objects grouped by name when loading a level. If the level has this debug feature enabled it will print a report with the names and numbers to see which objects are overgrowing in count")]
        [SerializeField] private bool debugObjectCountTracking = false;

        private readonly List<string> nonGameplaySceneNames = new List<string>
        {
            "MainMenu",
            "LevelSelector"
        };

        //public event System.Action OnGameplayResetRequested;
        public event System.Action OnGameplayStateResetRequested; // For ResetState() only
        public event System.Action OnGameplayInitialSetup;        // For input/event/UI setup
        public event System.Action OnGameplayUISetupRequested;    // For UI setup
        public event System.Action OnLevelSelectorUISetupRequested;    // For UI setup for level selector

        private List<string> activeGameplayScenes = new List<string>();

        // "UIMain" is the gameplay HUD scene. It is reloaded fresh for each gameplay level so its
        // UI controllers' Start() re-runs and rebinds to that level's WaveSpawner/BossWaveHandler,
        // and unloaded again whenever we leave gameplay (main menu or level selector).
        //
        // It is tracked by its actual Scene handle rather than looked up by name, because
        // SceneManager.GetSceneByName cannot disambiguate once two loaded scenes share a name -
        // and two is exactly what used to happen: Bootstraper loads one copy at startup, and
        // LoadGameplayRoutine loaded a second on top of it, so every gameplay level ran with two
        // full HUDs, two EventSystems and two of every UI controller. Unity keeps only one
        // EventSystem active among duplicates, so clicks could land on UI that isn't the one
        // being drawn - which is what made the pause/end-screen buttons unreliable.
        private UnityEngine.SceneManagement.Scene uiScene;

        private bool isTransitioning;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            LoadMainMenu();
           
        }

        public void LoadMainMenu()
        {
            if (!BeginTransition()) return;
            StartCoroutine(RunTransition(LoadMainMenuRoutine()));
        }

        public void LoadGameplay(LevelData levelData)
        {
            if (!BeginTransition()) return;
            bool shouldReset = GameSession.Instance.IsNewRun;
            StartCoroutine(RunTransition(LoadGameplayRoutine(levelData, shouldReset)));
        }

        public void LoadLevelSelectorScene(bool newGame)
        {
            if (!BeginTransition()) return;
            StartCoroutine(RunTransition(LoadLevelSelectorSceneRoutine(newGame)));
        }

        /// <summary>
        /// Scene transitions must never overlap. They unload and reload the same scenes and share
        /// activeGameplayScenes, so two running at once corrupt each other - and because the UI
        /// stays interactive during the fade, a second click on a button like Exit is enough to
        /// start one. Returns false if a transition is already in flight.
        /// </summary>
        private bool BeginTransition()
        {
            if (isTransitioning)
            {
                Debug.LogWarning("MainSceneController: a scene transition is already in progress, ignoring this request.");
                return false;
            }

            isTransitioning = true;
            return true;
        }

        private IEnumerator RunTransition(IEnumerator routine)
        {
            try
            {
                yield return StartCoroutine(routine);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private IEnumerator LoadLevelSelectorSceneRoutine(bool newGame)
        {
            //Debug.Log("LoadLevelSelectorSceneRoutine");
            yield return StartCoroutine(FadeIn());

            //unloads main menu scenes if coming from main menu
            yield return StartCoroutine(UnloadScenesByName(nonGameplaySceneNames));

            //unloads gameplay scenes if already loaded (for restart between levels)
            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            //the gameplay HUD belongs to gameplay levels only, not the map
            yield return StartCoroutine(UnloadUIScene());

            //Spawn player
            PlayerManager.Instance.SpawnSelectedPlayer(GameSession.Instance.SelectedPlayerIndex, newGame);

            //loads level selector scene
            yield return SceneManager.LoadSceneAsync(SceneNames.LevelSelector, LoadSceneMode.Additive);
            PlayerManager.Instance.IsPlayerOnMap = true;

            // If starting a new game, generate a new map per act
            if (newGame)
            {
                var mapRunState = OverworldMapGenerator.GenerateRun(actDefinitions,seed);
                MapRunController.Instance.Initialize(mapRunState);             
                
            }

            yield return new WaitForSecondsRealtime(0.1f); // Wait for scene to be fully initialized

            // Move the player to the level selection scene
            var levelSelectionScene = SceneManager.GetSceneByName("LevelSelector");
            PlayerManager.Instance.MovePlayerToScene(levelSelectionScene);

            //Setup UI events
            OnLevelSelectorUISetupRequested?.Invoke();

            //Once level and player are loaded, bind events
            PlayerManager.Instance.BindWaveEventsIfReady();
            if (newGame)
            {
                // Resets the state, which initializes player components the first time it loads
                OnGameplayStateResetRequested?.Invoke();
                // Resets player components to default values, which is needed on the first run and also on retries to reset any progress in the level.
                PlayerManager.Instance.ResetAllPlayerComponentStates();
            }
            else
            {
                PlayerManager.Instance.LoadAllPlayerComponentStates();
            }
            PlayerManager.Instance.LateInitializePlayer();

            OnGameplayInitialSetup?.Invoke();

            PlayerManager.Instance.DisableComponentsForMap();

            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator LoadMainMenuRoutine()
        {           
            //Debug.Log("LoadMainMenuRoutine 1");
            yield return StartCoroutine(FadeIn());

            CleanupSceneObjects();

            //unloads main menu scenes and level selector scene if loaded already
            yield return StartCoroutine(UnloadScenesByName(nonGameplaySceneNames));

            MusicManager.Instance.PlayTrack(mainMenuMusic);

            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            //the gameplay HUD must go too, or it stays overlaid on top of the main menu
            yield return StartCoroutine(UnloadUIScene());


            yield return SceneManager.LoadSceneAsync(SceneNames.MainMenu, LoadSceneMode.Additive);
            yield return new WaitForSecondsRealtime(0.1f);
            GameSession.Instance.SetIsNewRun(true);           
            GameSession.Instance.Initialize();

            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator LoadGameplayRoutine(LevelData levelData, bool shouldReset)
        {
            //Debug.Log($"LoadGameplayRoutine");
            GameSession.Instance.currentLevel = levelData;
            yield return StartCoroutine(FadeIn());

            CleanupSceneObjects();
            PlayerManager.Instance.UnbindWaveEventsIfReady();           

            //unloads gameplay scenes if already loaded (for restart)
            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            //unloads main menu scenes and level selector scene if loaded already
            yield return StartCoroutine(UnloadScenesByName(nonGameplaySceneNames));

            //Spawn player
            PlayerManager.Instance.SpawnSelectedPlayer(GameSession.Instance.SelectedPlayerIndex, shouldReset);
            GameSession.Instance.SetIsNewRun(false);

            // Load the gameplay scene and UI scene
            yield return SceneManager.LoadSceneAsync(levelData.SceneName, LoadSceneMode.Additive);
            yield return StartCoroutine(LoadUIScene());
            PlayerManager.Instance.IsPlayerOnMap = false;

            //clear previously loaded scenes
            activeGameplayScenes.Clear();
            activeGameplayScenes.Add(levelData.SceneName);

            // Move the player to the gameplay scene
            var gameplayScene = SceneManager.GetSceneByName(levelData.SceneName);
            PlayerManager.Instance.MovePlayerToScene( gameplayScene);         

            yield return new WaitForSecondsRealtime(0.1f); // Wait for player to be fully initialized

            //Setup UI events
            OnGameplayUISetupRequested?.Invoke();

            //Once level and player are loaded, bind events
            PlayerManager.Instance.BindWaveEventsIfReady();
            if (shouldReset)
            {
                // Resets the state, which initializes player components the first time it loads
                OnGameplayStateResetRequested?.Invoke();
                // Resets player components to default values, which is needed on the first run and also on retries to reset any progress in the level.
                PlayerManager.Instance.ResetAllPlayerComponentStates(); 
            }
            else
            {
                PlayerManager.Instance.LoadAllPlayerComponentStates();
            }
            PlayerManager.Instance.LateInitializePlayer();

            OnGameplayInitialSetup?.Invoke();

            //debug
            if (debugObjectCountTracking)
            {
                ObjectCountTracker.Snapshot(levelData.SceneName);

                if (levelData.debugShouldPrintObjectCountReport)
                {
                    ObjectCountTracker.PrintReport();
                }
            }

            yield return StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Loads a fresh gameplay HUD, guaranteeing exactly one "UIMain" is ever loaded.
        /// See the comment on <see cref="uiScene"/> for why this is handle-based.
        /// </summary>
        private IEnumerator LoadUIScene()
        {
            yield return StartCoroutine(UnloadUIScene());

            yield return SceneManager.LoadSceneAsync(SceneNames.UI, LoadSceneMode.Additive);
            uiScene = SceneManager.GetSceneByName(SceneNames.UI);
        }

        /// <summary>
        /// Unloads the gameplay HUD. Called whenever we leave gameplay, so it can't sit on top of
        /// the main menu or the level selector.
        /// </summary>
        private IEnumerator UnloadUIScene()
        {
            // Adopt an instance we never loaded ourselves - Bootstraper loads one at startup.
            // Safe to resolve by name here precisely because we never allow two at once.
            if (!uiScene.IsValid() || !uiScene.isLoaded)
            {
                uiScene = SceneManager.GetSceneByName(SceneNames.UI);
            }

            if (uiScene.IsValid() && uiScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(uiScene);
            }

            uiScene = default;
        }

        public void RetryCurrentRun()
        {
            if (!BeginTransition()) return;
            StartCoroutine(RunTransition(RetryCurrentRunRoutine()));
        }

        private IEnumerator RetryCurrentRunRoutine()
        { 
            // Debug.Log($"RetryCurrentRunRoutine");
            // Mark the session as a new run so future state resets correctly
            GameSession.Instance.SetIsNewRun(true);
            GameSession.Instance.Initialize();

            // Fade to black before unloading
            yield return StartCoroutine(FadeIn());

            // Clean up any persistent gameplay objects
            CleanupSceneObjects();

            // Unload all active gameplay scenes
            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            //this retry ends up on the map, so the gameplay HUD goes too
            yield return StartCoroutine(UnloadUIScene());

            //Spawn player
            PlayerManager.Instance.SpawnSelectedPlayer(GameSession.Instance.SelectedPlayerIndex, true);

            // Clear the list after unloading
            activeGameplayScenes.Clear();

            // Load Level Selector again (fresh run)
            yield return SceneManager.LoadSceneAsync(SceneNames.LevelSelector, LoadSceneMode.Additive);

            var mapRunState = OverworldMapGenerator.GenerateRun(actDefinitions, seed);
            MapRunController.Instance.Initialize(mapRunState);

            yield return new WaitForSecondsRealtime(0.1f); // Wait for scene to be fully initialized

            // Move the player to the level selection scene
            var levelSelectionScene = SceneManager.GetSceneByName("LevelSelector");
            PlayerManager.Instance.MovePlayerToScene(levelSelectionScene);

            //Setup UI events
            OnLevelSelectorUISetupRequested?.Invoke();            

            //Once level and player are loaded, bind events
            PlayerManager.Instance.BindWaveEventsIfReady();

            // Resets the state, which initializes player components the first time it loads
            OnGameplayStateResetRequested?.Invoke();

            // Resets player components to default values, which is needed on the first run and also on retries to reset any progress in the level.
            PlayerManager.Instance.ResetAllPlayerComponentStates();

            PlayerManager.Instance.LateInitializePlayer();

            OnGameplayInitialSetup?.Invoke();

            PlayerManager.Instance.DisableComponentsForMap();

            yield return StartCoroutine(FadeOut());
        }


        private IEnumerator UnloadScenesByName(List<string> scenesToUnload)
        {
            // Iterate a snapshot: this loop yields between unloads, so the enumeration spans
            // several frames, and the list it is handed is the same one that gets cleared and
            // rebuilt during a transition. Enumerating it live would throw mid-transition and
            // kill the coroutine, leaving the game stuck behind a black fade.
            var sceneNames = new List<string>(scenesToUnload);

            foreach(string sceneName in sceneNames)
            {
              
                UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid() && scene.isLoaded)
                {
                   
                    yield return SceneManager.UnloadSceneAsync(scene);
                   
                }                
            }

            if (ReferenceEquals(scenesToUnload, activeGameplayScenes))
            {
                activeGameplayScenes.Clear();
            }

        }

        private IEnumerator FadeIn()
        {
            LoadingScreen.Instance.Show(0f);
            Image img = LoadingScreen.Instance.GetFadeImage();
            Color color = img.color;
            float time = 0f;
            while (time < fadeDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
                img.color = new Color(color.r, color.g, color.b, alpha);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            img.color = new Color(color.r, color.g, color.b, 1f);
            if (startingCamera != null)
            {
                startingCamera.SetActive(false);
            }
        }

        private IEnumerator FadeOut()
        {
            Image img = LoadingScreen.Instance.GetFadeImage();
            Color color = img.color;
            float time = 0f;
            while (time < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
                img.color = new Color(color.r, color.g, color.b, alpha);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            img.color = new Color(color.r, color.g, color.b, 0f);
            LoadingScreen.Instance.Hide();
        }

        private IEnumerator DisableStartingCamera()
        {
            yield return new WaitForSeconds(0.5f);
            if (startingCamera != null)
            {
                startingCamera.SetActive(false);
            }
        }

        private void CleanupSceneObjects()
        {
            var cleanupHandlers = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                            .OfType<ISceneCleanupHandler>();

            foreach (var handler in cleanupHandlers)
            {
                handler.Cleanup();
            }
        }

    }
}