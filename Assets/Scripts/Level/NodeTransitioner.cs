using Game.Control;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Level
{
    public class NodeTransitioner : MonoBehaviour
    {
        [SerializeField] private float timeUntilExit = 5f;
        [SerializeField] private bool enableExit = true;

        private float timer = 0f;
        private float waitToExit = 1f;

        private void Awake()
        {
            timer = timeUntilExit;
           
        }

        private void Start()
        {
            InitializeNode();
            FadeIn();
        }

        private void InitializeNode()
        {
            //PlayerController.Instance.transform.position = transform.position;
            CameraController.Instance.SetPlayerCameraFollow();
        }

        private void FadeIn()
        {
            UIFade.Instance.FadeToClear();
        }

        private void Update()
        {
            if(!enableExit) return;
            timer -= Time.deltaTime;
           
            if (timer <= 0)
            {
                StartCoroutine(ExitRoutine());
            }
        }

        private IEnumerator ExitRoutine()
        {
            UIFade.Instance.FadeToBlack();
            yield return new WaitForSeconds(waitToExit);
            // Load the next scene or perform any other exit logic here
            Debug.Log("Exiting the level...");
            SceneManager.LoadScene("Sandbox2");
        }

    }
}
