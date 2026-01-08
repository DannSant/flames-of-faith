using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{

    public class StatsPaneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject primaryContentPanel;
        [SerializeField] private GameObject secondaryContentPanel;
        [SerializeField] private GameObject backgroundPanel;
        [SerializeField] private TextMeshProUGUI primaryTitle;
        [SerializeField] private TextMeshProUGUI secondaryTitle;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [Header("Prefabs")]
        [SerializeField] private StatRowUI statRowPrefab;
        [Header("Display settings")]
        [SerializeField] private Color selectedStatColor;       

        private PlayerProgression playerProgression;

        private readonly Dictionary<StatType, StatRowUI> rows = new();

        private StatCategory displayCategory = StatCategory.Primary;

        private void Start()
        {
            if (PlayerManager.Instance!= null)
            {
                playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
                if(playerProgression != null)
                {
                    playerProgression.onDerivedStatsChanged += RefreshVisibleStats;
                }
                
            }
            
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

            primaryButton.onClick.AddListener(() => SetDisplayCategory(StatCategory.Primary));
            secondaryButton.onClick.AddListener(() => SetDisplayCategory(StatCategory.Secondary));

            HideStatsWindow(0);

        }

        private void OnDisable()
        {
           if(playerProgression != null)
            {
                playerProgression.onDerivedStatsChanged -= RefreshVisibleStats;
            }
                
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

            primaryButton.onClick.RemoveAllListeners();
            secondaryButton.onClick.RemoveAllListeners();
        }

        public void BuildStatRowsFromOverworld(Dictionary<StatType, int> savedStats)
        {
            var allStats = savedStats;
            var primaryStats = GetStatsByCategory(allStats, StatCategory.Primary);
            foreach (var kvp in primaryStats)
            {

                var row = Instantiate(statRowPrefab, primaryContentPanel.transform);
                row.Initialize(
                    StatDisplayNameHelper.GetDisplayName(kvp.Key),
                    kvp.Value,
                    getTextColor(kvp.Value),
                    kvp.Key
                );

                rows[kvp.Key] = row;
            }
            var secondaryStats = GetStatsByCategory(allStats, StatCategory.Secondary);
            foreach (var kvp in secondaryStats)
            {

                var row = Instantiate(statRowPrefab, secondaryContentPanel.transform);
                row.Initialize(
                    StatDisplayNameHelper.GetDisplayName(kvp.Key),
                    kvp.Value,
                    getTextColor(kvp.Value),
                    kvp.Key
                );

                rows[kvp.Key] = row;
            }
        }

        private void BuildStatRows()
        {
            if(playerProgression == null)
            {
                return;
            }
            var allStats = playerProgression.GetAllCurrentStats();
            var primaryStats = GetStatsByCategory(allStats, StatCategory.Primary);
            foreach (var kvp in primaryStats)            {               
               
                var row = Instantiate(statRowPrefab, primaryContentPanel.transform);
                row.Initialize(
                    StatDisplayNameHelper.GetDisplayName(kvp.Key),
                    kvp.Value,
                    getTextColor(kvp.Value),
                    kvp.Key
                );

                rows[kvp.Key] = row;
            }
            var secondaryStats = GetStatsByCategory(allStats, StatCategory.Secondary);
            foreach (var kvp in secondaryStats)            {
               
                var row = Instantiate(statRowPrefab, secondaryContentPanel.transform);
                row.Initialize(
                    StatDisplayNameHelper.GetDisplayName(kvp.Key),
                    kvp.Value,
                    getTextColor(kvp.Value),
                    kvp.Key
                );

                rows[kvp.Key] = row;
            }
        }

        private void SetDisplayCategory(StatCategory category)
        {
            displayCategory = category;
            ShowStatsWindow(null);
        }

        public void ShowStatsWindow(List<List<StatValuePair>> _)
        {

            // Build stat rows only once
            if(rows.Count == 0)
            {
                BuildStatRows();
            }

            if (displayCategory == StatCategory.Primary)
            {               
                primaryTitle.color = selectedStatColor;
                secondaryTitle.color = Color.white;
                primaryContentPanel.SetActive(true);
                secondaryContentPanel.SetActive(false);
            }
            else
            {               
                primaryTitle.color = Color.white;
                secondaryTitle.color = selectedStatColor;
                primaryContentPanel.SetActive(false);
                secondaryContentPanel.SetActive(true);
            }

           
            backgroundPanel.SetActive(true);

            RefreshVisibleStats();
        }

        private Dictionary<StatType,int> GetStatsByCategory(Dictionary<StatType, int>  allStats,StatCategory category)
        {
           
            Dictionary<StatType, int> filteredStats = new();
            foreach (var kvp in allStats)
            {
                var currentCategory = StatUpgradeDatabase.Instance.GetStatData(kvp.Key).Category;
                if (currentCategory == category)
                {
                    filteredStats.Add(kvp.Key, kvp.Value);
                }
            }
            return filteredStats;
        }

        private void RefreshVisibleStats()
        {
            if (!(primaryContentPanel.activeInHierarchy || secondaryContentPanel.activeInHierarchy))
                return;

            if(playerProgression == null)
                return;

            foreach (var kvp in rows)
            {
                int value = playerProgression.GetStatTotal(kvp.Key);
                kvp.Value.UpdateValue(value, getTextColor(value));
            }

            if(displayCategory == StatCategory.Primary)
            {
                primaryTitle.color = selectedStatColor;
                secondaryTitle.color = Color.white;
            }
            else
            {
                primaryTitle.color = Color.white;
                secondaryTitle.color = selectedStatColor;
            }

        }



        public void HideStatsWindow(int _)
        {
            primaryContentPanel.SetActive(false);
            secondaryContentPanel.SetActive(false);
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