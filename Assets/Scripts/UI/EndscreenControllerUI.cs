using Game.Boss;
using Game.GameSettings;
using Game.Waves;
using System.Collections;
using UnityEngine;

namespace Game.UI
{
    public class EndscreenControllerUI : MonoBehaviour
    {
        [SerializeField] private GameObject endScreenPanel;

        [Tooltip("Delay before the panel appears, so the boss's death animation gets to play " +
            "instead of being covered immediately. Demo-only screen, so this is a plain wait " +
            "rather than something tied to the animation's actual length.")]
        [SerializeField] private float showDelay = 2f;

        // One-time lookup purely to attach/detach an event subscription (wiring, not a data
        // query) - the boss level has no WaveSpawner.OnAllLevelsFinished to listen to instead,
        // since WaveSpawner is deliberately disabled there.
        private BossWaveHandler bossWaveHandler;

        private void Start()
        {
            endScreenPanel.SetActive(false);

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnAllLevelsFinished += ShowEndScreen;
            }

            bossWaveHandler = FindAnyObjectByType<BossWaveHandler>();
            if (bossWaveHandler != null)
            {
                bossWaveHandler.OnBossFightEnded += ShowEndScreen;
            }
        }

        private void OnDisable()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnAllLevelsFinished -= ShowEndScreen;
            }

            if (bossWaveHandler != null)
            {
                bossWaveHandler.OnBossFightEnded -= ShowEndScreen;
            }
        }

        private void ShowEndScreen()
        {
            StartCoroutine(ShowEndScreenAfterDelay());
        }

        private IEnumerator ShowEndScreenAfterDelay()
        {
            // Time.timeScale is still 1 here (nothing has paused yet), so this is a real-time
            // wait that lets the boss's death/fade-out animation actually play instead of being
            // instantly covered by the panel.
            yield return new WaitForSeconds(showDelay);

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