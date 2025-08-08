using Game.Common;
using Game.Progression;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Game.Progression
{
    /*[System.Serializable]
    public struct StatUpgradeConfig
    {
        public StatType statType;
        public int[] tierValues;
    }
    */
    [System.Serializable]
    public struct StatValuePair
    {
        public StatType statType;
        public int value;

        public StatValuePair(StatType statType, int value)
        {
            this.statType = statType;
            this.value = value;
        }
    }

    public class StatUpgradeDatabase : Singleton<StatUpgradeDatabase>
    {
        [SerializeField] private List<StatData> statsConfig;
        private Dictionary<StatType, int[]> tierLookup;
       

        protected override void Awake()
        {
            base.Awake();
            tierLookup = statsConfig.ToDictionary(cfg => cfg.StatType, cfg => cfg.TierValues);
        }

        public int GetUpgradeAmount(StatType statType, int tier = 0)
        {
            if (tierLookup.TryGetValue(statType, out int[] values))
            {
                if (tier >= 0 && tier < values.Length)
                {
                    return values[tier];
                }
                else if (values.Length > 0)
                {
                    return values[^1]; // fallback to last value
                }
            }

            Debug.LogWarning($"No upgrade value configured for {statType}, tier {tier}");
            return 1; // sensible fallback
        }
        public List<StatData> GetStatsConfig()
        {
            return statsConfig;
        }

        public string GetStatDescription(StatType statType)
        {
            var stat = statsConfig.FirstOrDefault(s => s.StatType == statType);
            if (stat != null)
            {
                return stat.Description;
            }
            Debug.LogWarning($"No description found for stat {statType}");
            return string.Empty; // fallback
        }
    }

}