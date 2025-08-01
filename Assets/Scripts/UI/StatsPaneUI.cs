using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class StatsPaneUI : MonoBehaviour
    {
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private GameObject backgroundPanel;
        [SerializeField] private StatRowUI statRowPrefab;

        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            playerProgression.onStatUpdated += ShowUpdatedStats;
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable += ShowStatsWindow;
                
               
            }
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if ((waveSpawner!=null))
            {
                waveSpawner.OnWaveStarted += HideStatsWindow;
            }

            HideStatsWindow(0);
        }

        private void OnDisable()
        {
            playerProgression.onStatUpdated -= ShowUpdatedStats;
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable -= ShowStatsWindow;

            }
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if ((waveSpawner != null))
            {
                waveSpawner.OnWaveStarted -= HideStatsWindow;
            }
        }

        public void ShowStatsWindow(List<List<StatValuePair>> _) 
        {
            //Show panel
            contentPanel.SetActive(true);
            backgroundPanel.SetActive(true);
            // Clear all stat rows
            for (int i = contentPanel.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(contentPanel.transform.GetChild(i).gameObject);
            }

            var allStats = playerProgression.GetAllCurrentStats();

            foreach (var kvp in allStats)
            {
                string statName = StatDisplayNameHelper.GetDisplayName(kvp.Key);
                int statValue = kvp.Value;
                Color color = getTextColor(); // Default color for now

                StatRowUI row = Instantiate(statRowPrefab, contentPanel.transform);
                row.Initialize(statName, statValue, color);
            }
        }

        private void ShowUpdatedStats(StatType statType, int value)
        {
            ShowStatsWindow(null);
        }

        

        public void HideStatsWindow(int _)
        {
            contentPanel.SetActive(false);
            backgroundPanel.SetActive(false);
        }

        private Color getTextColor()
        {
            return Color.white;
        }
    }

}