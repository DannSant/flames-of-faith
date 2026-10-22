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
        [TextArea]
        [SerializeField] private string description;
        [Header("Corruption Settings")]
        [Tooltip("Indicates whether this stat is affected by corruption.")]
        [SerializeField] private bool affectedByCorruption=false;
        [Tooltip("Factor by which corruption reduces this stat's effectiveness.")]
        [SerializeField] private float corruptionReduceFactor = 0.0f;
        [Header("Retired")]
        [Tooltip("No longer in the game (e.g. Cleave): hidden from the Stats pane, never offered on level-up, and no " +
                 "item may grant it. The StatType stays because stats are stored by index.")]
        [SerializeField] private bool retired = false;

        // Public properties (read-only)
        public StatType StatType => statType;
        public int InitialValue => initialValue;
        public bool AllowLevelUpgrades => allowLevelUpgrades;
        public int[] TierValues => tierValues;
        public StatCategory Category => category;
        public string Description => description;
        public bool AffectedByCorruption => affectedByCorruption;
        public float CorruptionReduceFactor => corruptionReduceFactor;
        public bool Retired => retired;

        // Optional helper: get upgrade value for a specific tier
        public int GetValueForTier(int tier)
        {
            if (tier < 0 || tier >= tierValues.Length)
                return 0; // fallback
            return tierValues[tier];
        }

    }

}