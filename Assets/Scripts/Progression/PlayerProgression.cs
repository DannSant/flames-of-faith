
using Game.Common;
using Game.Effects;
using Game.Scene;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression {
    public class PlayerProgression : MonoBehaviour, ILateInitializable, IPrimaryStateLoader
    {       

        private Dictionary<StatType, int> currentStats = new Dictionary<StatType, int>();

        public delegate void OnStatUpdated(StatType statType, int value);
        public event OnStatUpdated onStatUpdated;

        private Dictionary<StatType, float> cachedFinalStats = new();
        private bool statsDirty = true;

        private EffectStore effectStore;

        private void Awake()
        {
            effectStore = GetComponent<EffectStore>();
            effectStore.OnEffectsChanged += HandleEffectsChanged;
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

                float finalValue = (baseValue + flatBonus) * (1f + percentBonus);
                cachedFinalStats[stat] = finalValue;
                
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
        }

        // Method to update the player's stat
        public void UpdateStat(StatType statType, int value)
        {
            if (currentStats.ContainsKey(statType))
            {
                // Update the stat value
                currentStats[statType] += value;           

                // Trigger the event to notify subscribers
                onStatUpdated?.Invoke(statType, GetStatTotal(statType));

                // Mark stats as dirty
                statsDirty = true;
            }
        }

        public Dictionary<StatType, int> GetAllCurrentStats()
        {
            Dictionary<StatType, int> result = new Dictionary<StatType, int>();
            foreach (var stat in currentStats)
            {
                //Debug.Log($"Stat: {stat.Key}, Value: {stat.Value}");
                result.Add(stat.Key, GetStatTotal(stat.Key));
            }
            return result;
        }

        private void HandleEffectsChanged()
        {          

            // Mark stats as dirty
            statsDirty = true;

            // Notify all listeners that stats have changed
            foreach (var stat in currentStats.Keys)
            {
                onStatUpdated?.Invoke(stat, GetStatTotal(stat));
            }
                
        }

        // Method to retrieve a stat total value
        public int GetStatTotal(StatType statType)
        {
            return Mathf.FloorToInt(GetFinalStat(statType));
        }

        // Method to retrieve a stat base value
        /*public int GetStatBase(StatType statType)
        {
            return currentStats.ContainsKey(statType) ? currentStats[statType] : 0;
        }*/

        public float GetFinalStat(StatType stat)
        {
           
            if (statsDirty)
            {
                RecalculateFinalStats();
            }

            return cachedFinalStats.TryGetValue(stat, out float value) ? value : 0f;
        }

        // Method to update the player's stat from effects
        /*public void UpdateExtraStat(StatType statType, int value)
        {
            if (extraStats.ContainsKey(statType))
            {
                // Update the stat value
                extraStats[statType] += value;


                // Trigger the event to notify subscribers
                onStatUpdated?.Invoke(statType, GetStatTotal(statType));
            }else
            {
                // If the stat doesn't exist, add it
                extraStats.Add(statType, value);
                onStatUpdated?.Invoke(statType, GetStatTotal(statType));
            }
        }*/

        public void LateInitialize()
        {
          // not used
        }

        public void ResetState()
        {
            //State is reset in ResetProgression called from MainSceneController
        }

        public void SaveState()
        {
            GameSession.Instance.SaveStats(currentStats);
        }

        public void LoadState()
        {
            statsDirty = true;
            currentStats = new Dictionary<StatType, int>(GameSession.Instance.PlayerData.savedStats);
            
        }
    }
}
