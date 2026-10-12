using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Waves;
using Game.Common;
using System.Linq;
using Game.Combat;
using Game.Scene;
using Game.Items;

namespace Game.Progression
{
    public class UpgradeManager : Singleton<UpgradeManager>, ISceneCleanupHandler
    {
        private List<StatType> eligibleUpgradeStats = new();       

        private int lastRecordedLevel = 1;
        private int levelsGainedThisWave = 0;

        // The second argument previews a choice: (stat, amount) -> "Now: ... / After: ..." text, or null.
        public event Action<List<List<StatValuePair>>, Func<StatType, int, string>> OnUpgradeOptionsAvailable;
        public event Action OnNoUpgradeAvailable;       

        private PlayerProgression playerProgression;
        private PlayerExperience playerExperience;
       

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            SetupElegibleStats();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            playerExperience = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
          
            if (playerExperience != null)
            {
                lastRecordedLevel = playerExperience.GetCurrentLevel();
            }


            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested += ResetUpgradeManagerstate;
            }

            var playerExperienceData = GameSession.Instance.LoadPlayerExperienceState();
            lastRecordedLevel = playerExperienceData.CurrentLevel;
           
        }

        private void OnDestroy()
        {

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested -= ResetUpgradeManagerstate;
            }
        }

        private void SetupElegibleStats()
        {
            eligibleUpgradeStats.Clear();
            foreach (var stat in StatUpgradeDatabase.Instance.GetStatsConfig())
            {
                if (stat.AllowLevelUpgrades)
                {
                    eligibleUpgradeStats.Add(stat.StatType);
                }
            }
            if (eligibleUpgradeStats.Count == 0)
            {
                Debug.LogWarning("No eligible stats found for upgrades.");
            }
        }

        private void ResetUpgradeManagerstate()
        {
            lastRecordedLevel = 1;
            levelsGainedThisWave = 0;
        }


        private int RollUpgradeTier(StatType stat)
        {
            // Tier weights shifted by Luck; numbers live on PlayerStatSettings (shared with the tooltip).
            int luck = playerProgression.GetStatTotal(StatType.Luck);
            float[] baseChances = PlayerStatSettings.Instance.GetUpgradeTierChances(luck);

            // Build cumulative weights
            float total = baseChances.Sum();
            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            for (int i = 0; i < baseChances.Length; i++)
            {
                cumulative += baseChances[i];
                if (roll <= cumulative)
                    return i; // tier index
            }

            return 0; // fallback
        }


        private List<StatType> GetRandomStats(int count)
        {
            List<StatType> available = new List<StatType>(eligibleUpgradeStats);
            List<StatType> selected = new();

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }

            return selected;
        }

        public void RecordLastLevel()
        {
            int currentLevel = playerExperience.GetCurrentLevel();
            lastRecordedLevel = currentLevel;
        }

        public void BuildStatsUpgradeOptions() {
            List<List<StatValuePair>> upgradeChoiceGroups = new();

            for (int i = 0; i < levelsGainedThisWave; i++)
            {
                List<StatType> selectedStats = GetRandomStats(4); // Or adjust # of choices here
                List<StatValuePair> upgradeOptions = new();

                foreach (var stat in selectedStats)
                {
                    int tier = RollUpgradeTier(stat);
                    int value = StatUpgradeDatabase.Instance.GetUpgradeAmount(stat, tier);
                    upgradeOptions.Add(new StatValuePair(stat, value));
                }

                upgradeChoiceGroups.Add(upgradeOptions);
            }

            OnUpgradeOptionsAvailable?.Invoke(upgradeChoiceGroups, GetUpgradePreview);
        }

        /// <summary>
        /// "Now / After" benefit text for picking +amount of a stat, or null for stats with nothing
        /// to compute. Evaluated when the choice is shown, so earlier picks are already included.
        /// The preview adds the points directly; % item bonuses and stat bonus rules aren't applied.
        /// </summary>
        private string GetUpgradePreview(StatType stat, int amount)
        {
            if (playerProgression == null) return null;

            int current = playerProgression.GetStatTotal(stat);
            string now = playerProgression.GetStatBenefitText(stat, current);
            string after = playerProgression.GetStatBenefitText(stat, current + amount);
            if (string.IsNullOrEmpty(now) || string.IsNullOrEmpty(after)) return null;
            return $"Now: {now}\nAfter: {after}";
        }

        public void OnNoUpgradeOptionAvailable()
        {
            OnNoUpgradeAvailable?.Invoke();
        }

        public int CalculateLevelsGainedInWave() {
            int currentLevel = playerExperience.GetCurrentLevel();
            levelsGainedThisWave = currentLevel - lastRecordedLevel;           
            return levelsGainedThisWave;
        }

        public void Cleanup()
        {

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested -= ResetUpgradeManagerstate;
            }
            Destroy(gameObject);
        }        
    }
}
