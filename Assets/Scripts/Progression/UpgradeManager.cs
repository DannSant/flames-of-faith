using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Waves;
using Game.Common;
using System.Linq;
using Game.Combat;
using Game.Scene;

namespace Game.Progression
{
    
    public class UpgradeManager : Singleton<UpgradeManager>
    {
        [SerializeField] private List<StatType> eligibleUpgradeStats = new();       

        private int lastRecordedLevel = 1;
        private int levelsGainedThisWave = 0;

        public event Action<List<List<StatValuePair>>> OnUpgradeOptionsAvailable;
        public event Action OnNoUpgradeAvailable;

        private PlayerProgression playerProgression;
        private PlayerExperience playerExperience;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            playerExperience = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
            if (playerExperience != null)
            {
                lastRecordedLevel = playerExperience.GetCurrentLevel();
            }

            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveComplete += HandleWaveComplete;
            }

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += ResetUpgradeManagerstate;
            }
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnWaveComplete -= HandleWaveComplete;
            }
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= ResetUpgradeManagerstate;
            }
        }

        private void ResetUpgradeManagerstate()
        {
            lastRecordedLevel = 1;
            levelsGainedThisWave = 0;
        }

        private void HandleWaveComplete()
        {
            int currentLevel = playerExperience.GetCurrentLevel();
            levelsGainedThisWave = currentLevel - lastRecordedLevel;
            lastRecordedLevel = currentLevel;

            if (levelsGainedThisWave > 0)
            {
                List<List<StatValuePair>> upgradeChoiceGroups = new();

                for (int i = 0; i < levelsGainedThisWave; i++)
                {
                    List<StatType> selectedStats = GetRandomStats(3); // Or adjust # of choices here
                    List<StatValuePair> upgradeOptions = new();

                    foreach (var stat in selectedStats)
                    {
                        int tier = RollUpgradeTier(stat);
                        int value = StatUpgradeDatabase.Instance.GetUpgradeAmount(stat, tier);
                        upgradeOptions.Add(new StatValuePair(stat, value));
                    }

                    upgradeChoiceGroups.Add(upgradeOptions);
                }

                OnUpgradeOptionsAvailable?.Invoke(upgradeChoiceGroups);
            }
            else
            {
                OnNoUpgradeAvailable?.Invoke();
            }
        }

        private int RollUpgradeTier(StatType stat)
        {
            float[] baseChances = new float[] { 80f, 10f, 8f, 2f }; // Tiers 1–4

            int luck = playerProgression.GetStatTotal(StatType.Luck);

            // Adjust tier 1
            baseChances[0] = Mathf.Max(0f, baseChances[0] - luck);

            // Distribute the subtracted % across tiers 2–4
            float gainPerTier = luck / 3f;
            for (int i = 1; i < baseChances.Length; i++)
            {
                baseChances[i] += gainPerTier;
            }

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

        /*public void ApplyUpgrade(StatType selectedStat, int statIncreaseAmount)
        {
            PlayerProgression.Instance.UpdateStat(selectedStat, statIncreaseAmount);
            WaveSpawner.Instance.ConfirmNextWave();
        }*/

    }
}
