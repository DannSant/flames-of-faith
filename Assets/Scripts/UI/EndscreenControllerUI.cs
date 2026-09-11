using Game.GameSettings;
using Game.Waves;
using UnityEngine;

namespace Game.UI
{
    public class EndscreenControllerUI : MonoBehaviour
    {
        [SerializeField] private GameObject endScreenPanel;

        private void Start()
        {
            endScreenPanel.SetActive(false);

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnAllLevelsFinished += ShowEndScreen;
            }
        }

        private void OnDisable()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnAllLevelsFinished -= ShowEndScreen;
            }
        }

        private void ShowEndScreen()
        {
            endScreenPanel.SetActive(true);
            // Route through PauseManager (rather than setting Time.timeScale directly) so
            // PauseManager.IsPaused reflects it too - otherwise the player can still rotate
            // and attack behind the end screen, since Update() keeps running at timeScale 0.
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.SetPause(true);
            }
        }
    }

}