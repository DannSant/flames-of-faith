
using Game.Common;
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

        private Dictionary<StatType, int> extraStats = new Dictionary<StatType, int>();


        private void Start()
        {           
            if (MainSceneController.Instance != null)
            {               
                MainSceneController.Instance.OnGameplayStateResetRequested += ResetProgression;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested -= ResetProgression;
            }
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

        // Method to retrieve a stat total value
        public int GetStatTotal(StatType statType)
        {
            return GetStatBase(statType) + GetExtraStat(statType);
        }

        // Method to retrieve a stat base value
        public int GetStatBase(StatType statType)
        {
            return currentStats.ContainsKey(statType) ? currentStats[statType] : 0;
        }

        public int GetExtraStat(StatType statType)
        {
            return extraStats.ContainsKey(statType) ? extraStats[statType] : 0;
        }

        // Method to update the player's stat from effects
        public void UpdateExtraStat(StatType statType, int value)
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
        }

        public void LateInitialize()
        {
          
        }

        public void ResetState()
        {
           
        }

        public void SaveState()
        {
            GameSession.Instance.SaveStats(currentStats);
        }

        public void LoadState()
        {
            currentStats = new Dictionary<StatType, int>(GameSession.Instance.PlayerData.savedStats);
            
        }
    }
}
