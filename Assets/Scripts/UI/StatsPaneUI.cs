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

        private readonly Dictionary<StatType, StatRowUI> rows = new();

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            //playerProgression.onStatUpdated += ShowUpdatedStats;
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            playerProgression.onDerivedStatsChanged += RefreshVisibleStats;
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
            //playerProgression.onStatUpdated -= ShowUpdatedStats;
            playerProgression.onDerivedStatsChanged -= RefreshVisibleStats;
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
            contentPanel.SetActive(true);
            backgroundPanel.SetActive(true);

            var allStats = playerProgression.GetAllCurrentStats();

            foreach (var kvp in allStats)
            {
                if (rows.ContainsKey(kvp.Key))
                    continue;

                var row = Instantiate(statRowPrefab, contentPanel.transform);
                row.Initialize(
                    StatDisplayNameHelper.GetDisplayName(kvp.Key),
                    kvp.Value,
                    getTextColor(kvp.Value),
                    kvp.Key
                );

                rows[kvp.Key] = row;
            }
            RefreshVisibleStats();
        }

        private void RefreshVisibleStats()
        {
            if (!contentPanel.activeInHierarchy)
                return;

            foreach (var kvp in rows)
            {
                int value = playerProgression.GetStatTotal(kvp.Key);
                kvp.Value.UpdateValue(value, getTextColor(value));
            }
        }

        /*private void ShowUpdatedStats(StatType statType, int value)
        {
            //ShowStatsWindow(null);
            if (!rows.TryGetValue(statType, out var row))
                return;

            row.UpdateValue(value, getTextColor(value));
        }*/



        public void HideStatsWindow(int _)
        {
            contentPanel.SetActive(false);
            backgroundPanel.SetActive(false);
        }

        private Color getTextColor(int value)
        {
            if(value < 0)
            {
                return Color.red;
            }
            else if (value > 0)
            {
                return Color.green;
            }
            return Color.white;
        }
    }

}