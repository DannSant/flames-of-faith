
using Game.Combat;
using Game.Common;
using Game.Effects;
using Game.Scene;
using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression {
    public class PlayerProgression : MonoBehaviour, IInitializeAfterStateReady, IPrimaryStateLoader
    {       

        private Dictionary<StatType, int> currentStats = new Dictionary<StatType, int>();

        public delegate void OnStatUpdated(StatType statType, int value);
        public event OnStatUpdated onStatUpdated;
        public event Action onDerivedStatsChanged;

        private Dictionary<StatType, float> cachedFinalStats = new();
        private bool statsDirty = true;

        private EffectStore effectStore;
        private PlayerCorruption playerCorruption;
        private PlayerGrace playerGrace;

        private void Awake()
        {
            effectStore = GetComponent<EffectStore>();
            effectStore.OnEffectsChanged += HandleEffectsChanged;

            playerCorruption = GetComponent<PlayerCorruption>();
            playerCorruption.OnCorruptionChanged += OnCorruptionChanged;

            playerGrace = GetComponent<PlayerGrace>();
            //playerGrace.onGraceChanged += OnGraceChanged;
        }

        private void Start()
        {
           
            if (MainSceneController.Instance != null)
            {               
                MainSceneController.Instance.OnGameplayStateResetRequested += ResetProgression;
            }

        }

        private void OnDisable()
        {
            effectStore.OnEffectsChanged -= HandleEffectsChanged;
            playerCorruption.OnCorruptionChanged -= OnCorruptionChanged;
            //playerGrace.onGraceChanged -= OnGraceChanged;
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested -= ResetProgression;
            }
        }

        private void RecalculateFinalStats()
        {
            cachedFinalStats.Clear();

            float flatBonus;
            float percentBonus;

            var allModifiers = effectStore.GetAllStatModifiers();
           
            foreach (var stat in currentStats.Keys)
            {
                float baseValue = currentStats[stat];
                flatBonus = 0f;
                percentBonus = 0f;

                foreach (var m in allModifiers)
                {
                    if (m.stat != stat)
                        continue;

                    if (m.type == ModifierType.Flat)
                        flatBonus += m.value;
                    else if (m.type == ModifierType.PercentAdd)
                        percentBonus += m.value;
                }

                float totalBeforeCorruption = (baseValue + flatBonus) * (1f + percentBonus);

                // Handle corruption effects if applicable
                StatData statData = StatUpgradeDatabase.Instance.GetStatData(stat); // Exception is thrown inside this method
                if(statData != null && statData.AffectedByCorruption)
                {
                    float corruptionLevel = playerCorruption.CalculateCorruptionEffectLevel();
                    float corruptionReduction = corruptionLevel * statData.CorruptionReduceFactor;
                    float finalValue = totalBeforeCorruption - corruptionReduction;
                    cachedFinalStats[stat] = finalValue;
                    //Debug.Log($"Corrupted Stat {stat} finalValue {finalValue}");
                }else
                {
                    cachedFinalStats[stat] = totalBeforeCorruption;
                    
                }

               
                
            }

            statsDirty = false;
        }

       

        private void ResetProgression()
        {
            currentStats.Clear();            
            foreach (var statData in StatUpgradeDatabase.Instance.GetStatsConfig())
            {               
                currentStats.Add(statData.StatType, statData.InitialValue);
                onStatUpdated?.Invoke(statData.StatType, statData.InitialValue);
            }
            statsDirty = true;
        }

        // Method to update the player's stat
        public void UpdateStat(StatType statType, int value)
        {
            if (currentStats.ContainsKey(statType))
            {
                // Update the stat value
                currentStats[statType] += value;

                // Mark stats as dirty
                statsDirty = true;

                var newStat = GetStatTotal(statType, true);
                // Trigger the event to notify subscribers
                onStatUpdated?.Invoke(statType, newStat);

                //Trigger event for derived stats change
                onDerivedStatsChanged?.Invoke(); 

                //Debug.Log($"Updated Stat {statType} by {value}. New total: {newStat}");

            }
        }

        public Dictionary<StatType, int> GetAllCurrentStats()
        {
           
            Dictionary<StatType, int> result = new Dictionary<StatType, int>();
            foreach (var stat in currentStats)
            {               
                result.Add(stat.Key, GetStatTotal(stat.Key));
            }
            return result;
        }

        /*private void OnGraceChanged(float _, float __)
        {
            statsDirty = true;
            onDerivedStatsChanged?.Invoke();
        }*/

        private void OnCorruptionChanged(float _, float __, float ___)
        {
            statsDirty = true;
            onDerivedStatsChanged?.Invoke();
        }

        private void HandleEffectsChanged()
        {
            statsDirty = true;
            onDerivedStatsChanged?.Invoke();
        }

        // Method to retrieve a stat total value
        public int GetStatTotal(StatType statType, bool forceRecalculate = false)
        {
            return Mathf.FloorToInt(GetFinalStat(statType, forceRecalculate));
        }

        public float GetFinalStat(StatType stat, bool forceRecalculate=false)
        {
           
            if (statsDirty || forceRecalculate)
            {
                RecalculateFinalStats();
            }

            return cachedFinalStats.TryGetValue(stat, out float value) ? value : 0f;
        }

       

        public void InitializeAfterStateReady()
        {
          // not used
        }

        public void ResetState()
        {
            //State is reset in ResetProgression called from MainSceneController
        }

        public void SaveState()
        {
            /*foreach (var stat in currentStats)
            {
                if(stat.Key == StatType.MaxGrace)
                {
                    Debug.Log($"Saving Stat {stat.Key} with value {stat.Value}");
                    break;
                }
            }*/
            GameSession.Instance.SaveStats(currentStats);
        }

        public void LoadState()
        {
            statsDirty = true;
            currentStats = new Dictionary<StatType, int>(GameSession.Instance.PlayerData.savedStats);
            /*foreach (var stat in currentStats)
            {
                if (stat.Key == StatType.MaxGrace)
                {
                    Debug.Log($"Loading Stat {stat.Key} with value {stat.Value}");
                    break;
                }
            }*/

        }
    }
}
