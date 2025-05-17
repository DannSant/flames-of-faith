using Game.Waves;
using TMPro;
using UnityEngine;

namespace Game.UI {
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI waveTimeText;

        private void Start()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveStarted += WaveUI_OnWaveStartedEvent;
                WaveSpawner.Instance.OnWaveTimerUpdated += WaveUI_OnWaveTimeUpdatedEvent;
            }
            WaveUI_OnWaveStartedEvent(1);
        }

        private void WaveUI_OnWaveTimeUpdatedEvent(float time)
        {
            waveTimeText.text = (time).ToString("F0");
        }

        private void WaveUI_OnWaveStartedEvent(int waveNumber)
        {          
            waveNumberText.text = "Wave: " + (waveNumber);
        }
    }
}
