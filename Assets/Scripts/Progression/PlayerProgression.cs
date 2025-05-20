
using Game.Common;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression {
    public class PlayerProgression : MonoBehaviour
    {
        [SerializeField] private List<StatData> initialStats = new List<StatData>();

        private Dictionary<StatType, int> currentStats = new Dictionary<StatType, int>();

        public delegate void OnStatUpdated(StatType statType, int value);
        public event OnStatUpdated onStatUpdated;

        [System.Serializable]
        public struct StatData {
            public StatType statType;
            public int initialValue;             
        }

        private void Awake()
        {           
            ResetProgression();            
        }

        private void Start()
        {
            if (MainSceneController.Instance != null)
            {               
                MainSceneController.Instance.OnGameplayResetRequested += ResetProgression;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= ResetProgression;
            }
        }

        private void ResetProgression()
        {
            currentStats.Clear();
            foreach (var statData in initialStats)
            {
                currentStats.Add(statData.statType, statData.initialValue);
                onStatUpdated?.Invoke(statData.statType, statData.initialValue);
            }
        }

        // Method to update the player's stat
        public void UpdateStat(StatType statType, int value)
        {
            if (currentStats.ContainsKey(statType))
            {
                // Update the stat value
                currentStats[statType] += value;

                Debug.Log($"Stat updated: {statType} - {currentStats[statType]}");

                // Trigger the event to notify subscribers
                onStatUpdated?.Invoke(statType, GetStatTotal(statType));
            }
        }

        // Method to retrieve a stat total value
        public int GetStatTotal(StatType statType)
        {
            return GetStatBase(statType);
        }

        // Method to retrieve a stat base value
        public int GetStatBase(StatType statType)
        {
            return currentStats.ContainsKey(statType) ? currentStats[statType] : 0;
        }

    }
}
