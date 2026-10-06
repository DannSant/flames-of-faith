using Game.GameSettings;
using Game.Waves;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Small tutorial panel that tells the player what the current wave expects from them. Stays visible for the
    /// whole step (no timeout). Can be turned off with SettingsManager.ShowTutorialHints.
    /// </summary>
    public class WaveHintUI : MonoBehaviour
    {
        [Tooltip("Shown while there is a hint. Must be a child object, not this one.")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Hints")]
        [TextArea]
        [SerializeField] private string waveStartedHint = "Kill enemies until you lure out the Corruptor";
        [SerializeField] private Color waveStartedColor = Color.white;
        [TextArea]
        [SerializeField] private string corruptorSpawnedHint = "You are getting Corruption, kill the Corruptor now!";
        [SerializeField] private Color corruptorSpawnedColor = new Color(0.75f, 0.45f, 1f);

        private string currentHint;
        private Color currentColor;

        private void Start()
        {
            if (panel != null && panel == gameObject)
            {
                Debug.LogError("WaveHintUI: panel must be a child object, otherwise hiding it also disables this script.");
            }

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveStarted += HandleWaveStarted;
                WaveSpawner.Instance.OnCorruptorSpawned += HandleCorruptorSpawned;
                WaveSpawner.Instance.OnCorruptorPhaseEnded += ClearHint;
                WaveSpawner.Instance.OnWaveCompleteStarted += ClearHint;

                // The UI scene loads after the level, so the first wave may already be running
                if (WaveSpawner.Instance.WaveInProgress)
                {
                    ShowHint(waveStartedHint, waveStartedColor);
                }
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnShowTutorialHintsChanged += HandleHintsSettingChanged;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveStarted -= HandleWaveStarted;
                WaveSpawner.Instance.OnCorruptorSpawned -= HandleCorruptorSpawned;
                WaveSpawner.Instance.OnCorruptorPhaseEnded -= ClearHint;
                WaveSpawner.Instance.OnWaveCompleteStarted -= ClearHint;
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnShowTutorialHintsChanged -= HandleHintsSettingChanged;
            }
        }

        private void HandleWaveStarted(int _) => ShowHint(waveStartedHint, waveStartedColor);

        private void HandleCorruptorSpawned(Transform _) => ShowHint(corruptorSpawnedHint, corruptorSpawnedColor);

        private void HandleHintsSettingChanged(bool _) => Refresh();

        private void ShowHint(string hint, Color color)
        {
            currentHint = hint;
            currentColor = color;
            Refresh();
        }

        private void ClearHint()
        {
            currentHint = null;
            Refresh();
        }

        private void Refresh()
        {
            bool hintsEnabled = SettingsManager.Instance == null || SettingsManager.Instance.ShowTutorialHints;
            bool visible = hintsEnabled && !string.IsNullOrEmpty(currentHint);

            if (hintText != null && visible)
            {
                hintText.text = currentHint;
                hintText.color = currentColor;
            }

            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }
    }
}
