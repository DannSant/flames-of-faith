using Game.Common;
using Game.Effects;
using Game.Map;
using Game.Metaprogression;
using Game.Overworld;
using Game.Progression;
using Game.Saving;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scene
{
    public class GameSession : Singleton<GameSession>
    {
        [Header("Character Selection")]
        public int SelectedPlayerIndex = 0;

        [Header("Other Settings")]
        public DifficultyLevel selectedDifficulty = DifficultyLevel.Normal;

        

        public LevelData currentLevel;

        //state
        private bool isNewRun = true;
        private bool isInitialized = false;
        private PlayerData playerData;
        private int levelsBeaten = 0; // Track how many levels have been beaten in this session
        private bool suppressNextProgressionIncrement = false;

        //Properties
        public bool IsInitialized => isInitialized;
        public bool IsNewRun  => isNewRun;
        public PlayerData PlayerData => playerData;
        public int LevelsBeaten => levelsBeaten;


        protected override void Awake()
        {
            base.Awake();
            playerData = new PlayerData();
            Application.targetFrameRate = 60;            
        }

        private void Start()
        {
            Initialize();           
        }

        public void Initialize()
        {
            levelsBeaten = 0;           
            //LevelSelectionController.Instance.InitialSetup();            
        }

        public void MarkLevelBeaten(LevelData level)
        {
            // Only fights count toward run progression. This fires for every level the player
            // finishes, including shops, campfires, treasures and event encounters - counting
            // those would scale enemy damage, health and XP based on the route taken through the
            // map rather than on how much combat the player has actually cleared.
            if (level == null) return;
            if (level.type != LevelType.Combat && level.type != LevelType.Boss) return;

            if (suppressNextProgressionIncrement)
            {
                suppressNextProgressionIncrement = false;
                return;
            }

            levelsBeaten++;
        }

        // Debug-window hook: lets a level be skipped without counting toward run
        // progression (enemy health/damage/XP scaling). Self-resets after one use.
        public void SetSuppressNextProgressionIncrement(bool suppress)
        {
            suppressNextProgressionIncrement = suppress;
        }

        public void SetIsNewRun(bool value)
        {
            isNewRun = value;
        }

        public void SaveCurrentHealth(float health)
        {
            playerData.currentHealth = health;
        }

        public void SaveCurrentGrace(float grace)
        {
            playerData.currentGrace = grace;
        }

        public void SaveStats(Dictionary<StatType, int> stats)
        {
            playerData.savedStats.Clear();
            foreach (var stat in stats)
            {
                playerData.savedStats[stat.Key] = stat.Value;
               
            }
            
        }

        public void SaveEffectStore(List<EffectInstance> effects)
        {
            // Copy so the snapshot can't change when the live store does
            GameSession.Instance.playerData.savedEffects = new List<EffectInstance>(effects);
            //Debug.Log($"Saved {effects.Count} effects to player data store.");
        }

        public List<EffectInstance> LoadEffectStore()
        {
            //Debug.Log($"Loaded {GameSession.Instance.playerData.savedEffects.Count} effects to player data store.");
            return GameSession.Instance.playerData.savedEffects;

        }

        public void SaveCurrencyAmount(int amount)
        {
            playerData.currencyAmount = amount;
        }

        public int LoadCurrencyAmount()
        {
            return playerData.currencyAmount;
        }

        public float LoadCurrentHealth()
        {
            return playerData.currentHealth;
        }

        public float LoadMaxHealth()
        {
            int maxHealth = 0;
            playerData.savedStats.TryGetValue(StatType.MaxHealth, out maxHealth);
            return maxHealth;
        }
        public float LoadCurrentGrace()
        {
            //Debug.Log($"Loaded Grace: {playerData.currentGrace}");
            return playerData.currentGrace;
        }

        public void SavePlayerExperienceState(PlayerExperienceData data)
        {
            playerData.playerExperienceData = data;
        }

        public PlayerExperienceData LoadPlayerExperienceState()
        {
            return playerData.playerExperienceData;
        }

        /// <summary>
        /// Starts a run with empty player data, so stale data from a previous run in this session can't be saved.
        /// </summary>
        public void ResetPlayerData()
        {
            playerData = new PlayerData();
        }

        public RunSaveData ToSaveData()
        {
            var data = new RunSaveData
            {
                selectedPlayerIndex = SelectedPlayerIndex,
                difficulty = selectedDifficulty.ToString(),
                levelsBeaten = levelsBeaten,
                runStarted = !isNewRun,
                currentHealth = playerData.currentHealth,
                currentGrace = playerData.currentGrace,
                currencyAmount = playerData.currencyAmount,
                experience = playerData.playerExperienceData
            };

            foreach (var stat in playerData.savedStats)
            {
                data.stats.Add(new StatSaveEntry { stat = stat.Key.ToString(), value = stat.Value });
            }

            foreach (var effect in playerData.savedEffects)
            {
                if (effect.effect == null) continue;
                data.effects.Add(new EffectSaveEntry { effectId = effect.effect.EffectID, count = effect.count });
            }

            return data;
        }

        /// <summary>
        /// Checks that every saved effect and stat still exists in this build.
        /// </summary>
        public static bool CanApplySaveData(RunSaveData data)
        {
            foreach (var stat in data.stats)
            {
                if (!Enum.TryParse(stat.stat, out StatType _))
                {
                    Debug.LogWarning($"[RunSave] Unknown stat '{stat.stat}' in the save.");
                    return false;
                }
            }

            foreach (var effect in data.effects)
            {
                if (EffectsDatabaseProvider.GetEffectById(effect.effectId) == null)
                {
                    Debug.LogWarning($"[RunSave] Unknown effect '{effect.effectId}' in the save.");
                    return false;
                }
            }

            return true;
        }

        public void ApplySaveData(RunSaveData data)
        {
            SelectedPlayerIndex = data.selectedPlayerIndex;
            if (Enum.TryParse(data.difficulty, out DifficultyLevel difficulty))
            {
                selectedDifficulty = difficulty;
            }
            levelsBeaten = data.levelsBeaten;
            isNewRun = !data.runStarted;

            playerData = new PlayerData
            {
                currentHealth = data.currentHealth,
                currentGrace = data.currentGrace,
                currencyAmount = data.currencyAmount,
                playerExperienceData = data.experience
            };

            foreach (var stat in data.stats)
            {
                if (Enum.TryParse(stat.stat, out StatType statType))
                {
                    playerData.savedStats[statType] = stat.value;
                }
            }

            foreach (var entry in data.effects)
            {
                var effect = EffectsDatabaseProvider.GetEffectById(entry.effectId);
                if (effect != null)
                {
                    playerData.savedEffects.Add(new EffectInstance(effect, entry.count));
                }
            }
        }




    }
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }
}
