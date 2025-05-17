using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scene {
    public class Bootstraper : MonoBehaviour
    {
        [SerializeField] private string gameplayScene = "Sandbox1";
        [SerializeField] private string uiScene = "UIMain";

        private void Start()
        {
            StartCoroutine(LoadScenes());
        }

        private IEnumerator LoadScenes()
        {
            yield return SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive);
            yield return SceneManager.LoadSceneAsync(uiScene, LoadSceneMode.Additive);
            // Disable the temporary camera after scenes are loaded
            Camera tempCam = Camera.main; // Or find it by name/tag if needed
            if (tempCam != null)
            {
                tempCam.gameObject.SetActive(false);
            }
        }
    }
}

