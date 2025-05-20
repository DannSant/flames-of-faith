using Game.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Game.Scene
{

    public class MainSceneController : Singleton<MainSceneController>
    {
        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameplaySceneName = "Gameplay";
        [SerializeField] private string uiSceneName = "UI";       
        [SerializeField] private List<string> mainMenuScenes = new List<string>();
        [SerializeField] private List<string> gameplayScenes = new List<string>();

        [Header("Loading Screen")]
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Other")]
        [SerializeField] private GameObject startingCamera;

        public event System.Action OnGameplayResetRequested;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            LoadMainMenu();
            StartCoroutine(DisableStartingCamera());
        }

        public void LoadMainMenu()
        {
            StartCoroutine(LoadMainMenuRoutine());
        }

        public void LoadGameplay()
        {
            StartCoroutine(LoadGameplayRoutine());
        }

        private IEnumerator LoadMainMenuRoutine()
        {
            yield return StartCoroutine(FadeIn());
           
            yield return StartCoroutine(UnloadScenesByName(gameplayScenes));

            yield return SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);

            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator LoadGameplayRoutine()
        {
            yield return StartCoroutine(FadeIn());

            //Spawn player
            PlayerManager.Instance.SpawnSelectedPlayer(GameSession.Instance.SelectedPlayerIndex);

            //unloads gameplay scenes if already loaded (for restart)
            yield return StartCoroutine(UnloadScenesByName(gameplayScenes));

            //unloads main menu scenes if coming from main menu
            yield return StartCoroutine(UnloadScenesByName(mainMenuScenes));

            yield return SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
            yield return SceneManager.LoadSceneAsync(uiSceneName, LoadSceneMode.Additive);

            // Move the player to the gameplay scene
            var gameplayScene = SceneManager.GetSceneByName(gameplaySceneName);
            PlayerManager.Instance.MovePlayerToScene( gameplayScene);
            PlayerManager.Instance.LateInitializePlayer();

            OnGameplayResetRequested?.Invoke();

            yield return StartCoroutine(FadeOut());
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


    }
}