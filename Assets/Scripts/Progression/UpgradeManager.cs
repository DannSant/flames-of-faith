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
    /// <summary>Everything the upgrade choice UI needs, pushed by <see cref="UpgradeManager"/> (gameplay pushes, UI renders).</summary>
    public class UpgradeChoices
    {
        /// <summary>One group of choices per level gained, in level order.</summary>
        public List<List<StatValuePair>> Groups;
        /// <summary>(stat, amount) -> "Now: ... / After: ..." text, or null.</summary>
        public Func<StatType, int, string> Preview;
        /// <summary>The level the first group belongs to; each next group is one level higher.</summary>
        public int FirstLevel;
        /// <summary>The class's recommended stats (gold/silver stars); empty when unknown.</summary>
        public IReadOnlyList<ClassStatPriority> ClassStats = Array.Empty<ClassStatPriority>();
        /// <summary>For the tooltip line ("Key stat for the Warrior").</summary>
        public string ClassName;
    }

    public class UpgradeManager : Singleton<UpgradeManager>, ISceneCleanupHandler
    {
        private List<StatType> eligibleUpgradeStats = new();

        private int lastRecordedLevel = 1;
        private int levelsGainedThisWave = 0;

        public event Action<UpgradeChoices> OnUpgradeOptionsAvailable;
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
                if (stat.AllowLevelUpgrades && !stat.Retired)
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

            var classData = PlayerManager.Instance != null
                ? PlayerManager.Instance.GetPlayerComponent<PlayerClassManager>()?.ClassData
                : null;
            OnUpgradeOptionsAvailable?.Invoke(new UpgradeChoices
            {
                Groups = upgradeChoiceGroups,
                Preview = GetUpgradePreview,
                FirstLevel = UpgradeLevels.FirstLevel(playerExperience.GetCurrentLevel(), levelsGainedThisWave),
                ClassStats = classData != null && classData.statPriorities != null
                    ? classData.statPriorities
                    : (IReadOnlyList<ClassStatPriority>)Array.Empty<ClassStatPriority>(),
                ClassName = classData != null ? classData.characterName : null,
            });
        }

        /// <summary>
        /// "Now / After" benefit text for picking +amount of a stat, plus any other stat the pick
        /// changes through stat bonus rules ("Also: +2 Pierce"). Evaluated when the choice is shown,
        /// so earlier picks are already included. "After" goes through the same math as the real
        /// stats (PlayerProgression.PreviewFinalStats): % item bonuses, corruption and stat bonus
        /// rules included. Null when there's nothing to show.
        /// </summary>
        private string GetUpgradePreview(StatType stat, int amount)
        {
            if (playerProgression == null) return null;

            var preview = playerProgression.PreviewFinalStats(stat, amount);
            int current = playerProgression.GetStatTotal(stat);
            int after = preview.TryGetValue(stat, out float afterValue) ? Mathf.FloorToInt(afterValue) : current + amount;

            var lines = new List<string>();
            string nowText = playerProgression.GetStatBenefitText(stat, current);
            string afterText = playerProgression.GetStatBenefitText(stat, after);
            if (!string.IsNullOrEmpty(nowText) && !string.IsNullOrEmpty(afterText))
            {
                lines.Add($"Now: {nowText}");
                lines.Add($"After: {afterText}");
            }

            string also = DescribeOtherChanges(stat, preview);
            if (!string.IsNullOrEmpty(also)) lines.Add($"Also: {also}");

            return lines.Count > 0 ? string.Join("\n", lines) : null;
        }

        // Other stats a pick changes through stat bonus rules, e.g. "+2 Pierce, -1 Health Regen".
        private string DescribeOtherChanges(StatType pickedStat, Dictionary<StatType, float> preview)
        {
            var changes = new List<string>();
            foreach (var pair in preview)
            {
                if (pair.Key == pickedStat) continue;

                int difference = Mathf.FloorToInt(pair.Value) - playerProgression.GetStatTotal(pair.Key);
                if (difference == 0) continue;

                string sign = difference > 0 ? "+" : "";
                changes.Add($"{sign}{difference} {StatDisplayNameHelper.GetDisplayName(pair.Key)}");
            }
            return changes.Count > 0 ? string.Join(", ", changes) : null;
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
