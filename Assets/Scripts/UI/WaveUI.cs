using Game.UI.Style;
using Game.Waves;
using TMPro;
using UnityEngine;

namespace Game.UI {
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI waveTimeText;

        // The timer shows the Corruptor countdown in the Corruption color (UISettings) while the Corruptor phase runs
        private void Start()
        {
            waveTimeText.color = UISettings.Instance.TextColor;

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveStarted += WaveUI_OnWaveStartedEvent;
                WaveSpawner.Instance.OnWaveTimerUpdated += WaveUI_OnWaveTimeUpdatedEvent;
                WaveSpawner.Instance.OnCorruptorPhaseStarted += WaveUI_OnCorruptorPhaseStarted;
                WaveSpawner.Instance.OnCorruptorTimerUpdated += WaveUI_OnWaveTimeUpdatedEvent;
                WaveSpawner.Instance.OnCorruptorPhaseEnded += WaveUI_OnCorruptorPhaseEnded;
            }
            WaveUI_OnWaveStartedEvent(1);
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveStarted -= WaveUI_OnWaveStartedEvent;
                WaveSpawner.Instance.OnWaveTimerUpdated -= WaveUI_OnWaveTimeUpdatedEvent;
                WaveSpawner.Instance.OnCorruptorPhaseStarted -= WaveUI_OnCorruptorPhaseStarted;
                WaveSpawner.Instance.OnCorruptorTimerUpdated -= WaveUI_OnWaveTimeUpdatedEvent;
                WaveSpawner.Instance.OnCorruptorPhaseEnded -= WaveUI_OnCorruptorPhaseEnded;
            }
        }

        private void WaveUI_OnWaveTimeUpdatedEvent(float time)
        {
            waveTimeText.text = (time).ToString("F0");
        }

        private void WaveUI_OnWaveStartedEvent(int waveNumber)
        {
            waveNumberText.text = "Wave " + waveNumber;
        }

        private void WaveUI_OnCorruptorPhaseStarted(float duration, Transform _)
        {
            waveTimeText.color = UISettings.Instance.CorruptionColor;
            WaveUI_OnWaveTimeUpdatedEvent(duration);
        }

        private void WaveUI_OnCorruptorPhaseEnded()
        {
            waveTimeText.color = UISettings.Instance.TextColor;
            WaveUI_OnWaveTimeUpdatedEvent(0f);
        }
    }
}
