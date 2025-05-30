using UnityEngine;

namespace Game.Progression
{

    public enum StatCategory
    {
        Primary, 
        Secondary,
    }

    [System.Serializable]
    public class StatData
    {
        [SerializeField] private StatType statType;
        [SerializeField] private int initialValue;
        [SerializeField] private bool allowLevelUpgrades;
        [SerializeField] private int[] tierValues;
        [SerializeField] private StatCategory category = StatCategory.Primary;

        // Public properties (read-only)
        public StatType StatType => statType;
        public int InitialValue => initialValue;
        public bool AllowLevelUpgrades => allowLevelUpgrades;
        public int[] TierValues => tierValues;

        // Optional helper: get upgrade value for a specific tier
        public int GetValueForTier(int tier)
        {
            if (tier < 0 || tier >= tierValues.Length)
                return 0; // fallback
            return tierValues[tier];
        }

    }

}