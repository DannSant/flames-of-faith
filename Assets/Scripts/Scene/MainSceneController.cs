using Game.Audio;
using Game.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Game.Scene
{

    public class MainSceneController : Singleton<MainSceneController>
    {
        [Header("Scene Names")]      
        [SerializeField] private List<string> activeGameplayScenes = new List<string>();

        [Header("Loading Screen")]
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Loading Screen")]
        [SerializeField] private AudioClip mainMenuMusic;

        [Header("Other")]
        [SerializeField] private GameObject startingCamera;

        private readonly List<string> nonGameplaySceneNames = new List<string>
        {
            "MainMenu",
            "LevelSelector"
        };

        //public event System.Action OnGameplayResetRequested;
        public event System.Action OnGameplayStateResetRequested; // For ResetState() only
        public event System.Action OnGameplayInitialSetup;        // For input/event/UI setup
        public event System.Action OnGameplayUISetupRequested;    // For UI setup

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
            StartCoroutine(LoadMainMenuRoutine());
        }

        public void LoadGameplay(LevelData levelData)
        {
            bool shouldReset = GameSession.Instance.IsNewRun;
            StartCoroutine(LoadGameplayRoutine(levelData, shouldReset));
        }

        public void LoadLevelSelectorScene()
        {
            StartCoroutine(LoadLevelSelectorSceneRoutine());
        }

        private IEnumerator LoadLevelSelectorSceneRoutine()
        {
            yield return StartCoroutine(FadeIn());

            //unloads main menu scenes if coming from main menu
            yield return StartCoroutine(UnloadScenesByName(nonGameplaySceneNames));

            //unloads gameplay scenes if already loaded (for restart between levels)
            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            //loads level selector scene
            yield return SceneManager.LoadSceneAsync(SceneNames.LevelSelector, LoadSceneMode.Additive);

            LevelSelectionController.Instance.StartMusicOfCurrentAct();

            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator LoadMainMenuRoutine()
        {           
            yield return StartCoroutine(FadeIn());

            CleanupSceneObjects();

            AudioManager.Instance.PlayMusic(mainMenuMusic);

            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            yield return SceneManager.LoadSceneAsync(SceneNames.MainMenu, LoadSceneMode.Additive);
            yield return new WaitForSeconds(0.1f);
            GameSession.Instance.Initialize();

            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator LoadGameplayRoutine(LevelData levelData, bool shouldReset)
        {
            GameSession.Instance.currentLevel = levelData;
            yield return StartCoroutine(FadeIn());

            CleanupSceneObjects();

            //Spawn player
            PlayerManager.Instance.SpawnSelectedPlayer(GameSession.Instance.SelectedPlayerIndex, GameSession.Instance.IsNewRun);
            GameSession.Instance.SetIsNewRun(false);

            //unloads gameplay scenes if already loaded (for restart)
            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            //unloads main menu scenes and level selector scene if loaded already
            yield return StartCoroutine(UnloadScenesByName(nonGameplaySceneNames));

            // Load the gameplay scene and UI scene
            yield return SceneManager.LoadSceneAsync(levelData.SceneName, LoadSceneMode.Additive);
            yield return SceneManager.LoadSceneAsync(SceneNames.UI, LoadSceneMode.Additive);
            

            //clear previously loaded scenes
            activeGameplayScenes.Clear();
            activeGameplayScenes.Add(levelData.SceneName);
            activeGameplayScenes.Add(SceneNames.UI);

            // Move the player to the gameplay scene
            var gameplayScene = SceneManager.GetSceneByName(levelData.SceneName);
            PlayerManager.Instance.MovePlayerToScene( gameplayScene);
            PlayerManager.Instance.LateInitializePlayer();
           
            yield return new WaitForSeconds(0.1f); // Wait for player to be fully initialized

            //Setup UI events
            OnGameplayUISetupRequested?.Invoke();

            //Once level and player are loaded, bind events
            PlayerManager.Instance.BindWaveEventsIfReady();
            if (shouldReset)
            {
                OnGameplayStateResetRequested?.Invoke();
            }else
            {
                PlayerManager.Instance.LoadAllPlayerComponentStates();
            }

            OnGameplayInitialSetup?.Invoke();

            yield return StartCoroutine(FadeOut());
        }

        public void RetryCurrentRun()
        {
            StartCoroutine(RetryCurrentRunRoutine());
        }

        private IEnumerator RetryCurrentRunRoutine()
        {
            Debug.Log("[MainSceneController] Retrying current run...");

            // Mark the session as a new run so future state resets correctly
            GameSession.Instance.SetIsNewRun(true);
            GameSession.Instance.Initialize();

            // Fade to black before unloading
            yield return StartCoroutine(FadeIn());

            // Clean up any persistent gameplay objects
            CleanupSceneObjects();

            // Unload all active gameplay scenes
            yield return StartCoroutine(UnloadScenesByName(activeGameplayScenes));

            // Clear the list after unloading
            activeGameplayScenes.Clear();

            // Load Level Selector again (fresh run)
            yield return SceneManager.LoadSceneAsync(SceneNames.LevelSelector, LoadSceneMode.Additive);

            // Play the act music if needed
            LevelSelectionController.Instance.StartMusicOfCurrentAct();

            yield return StartCoroutine(FadeOut());

            Debug.Log("[MainSceneController] Level selector reloaded successfully.");
        }


        private IEnumerator UnloadScenesByName(List<string> scenesToUnload)
        {

            foreach(string sceneName in scenesToUnload)
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
                time += Time.deltaTime;
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
                time += Time.deltaTime;
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