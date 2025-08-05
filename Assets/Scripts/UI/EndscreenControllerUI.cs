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
            Time.timeScale = 0f; // Pause the game
        }
    }

}