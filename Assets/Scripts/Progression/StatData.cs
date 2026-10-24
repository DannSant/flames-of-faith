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
        [Tooltip("Points the player starts a run with.")]
        [SerializeField] private int initialValue;
        [Tooltip("Level-up can offer this stat.")]
        [SerializeField] private bool allowLevelUpgrades;
        [Tooltip("Points a level-up choice gives, per tier (tier 1 is the most common, tier 4 the rarest; Luck makes higher tiers likelier).")]
        [SerializeField] private int[] tierValues;
        [Tooltip("The Stats pane tab it shows on: Primary or Secondary.")]
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