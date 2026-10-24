using UnityEngine;

namespace Game.Combat.Elemental
{
    [CreateAssetMenu(menuName = "Debuff/Elemental data")]
    public class ElementalDebuffData : ScriptableObject
    {
        [SerializeField] private ElementalType elementalType = ElementalType.None;
        [Tooltip("Seconds the debuff lasts with 0 Mastow Affinity.")]
        [SerializeField] private float baseDuration = 0f;
        [Tooltip("Seconds added to the duration per point of Mastow Affinity.")]
        [SerializeField] private float durationStatScale = 0.25f;
        [Tooltip("Strength with 0 Mastow Affinity. What it means depends on the element: Fire and Bleed = damage per tick (Bleed per stack), Frost = slow and extra damage taken (0.1 = 10%), Ley Charge = damage of each strike.")]
        [SerializeField] private float baseStrength = 0f;
        [Tooltip("Strength added per point of Mastow Affinity.")]
        [SerializeField] private float strengthStatScale = 1f;
        [Tooltip("Chance (0 to 1) that a hit applies the debuff.")]
        [Range(0f, 1f)]
        [SerializeField] private float chanceToApply = 1f;
        [Tooltip("If an enemy has already a debuff of the same type, add this number to the chance to apply it again")]
        [SerializeField] private float additionalChanceToReapply = 0f;
        [Tooltip("Seconds between damage-over-time ticks, for debuffs that tick.")]
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private GameObject vfx;
        [SerializeField] private DebuffContactSpreadData contactSpread = new DebuffContactSpreadData();
        [SerializeField] private DebuffStackData stacking = new DebuffStackData();
        [Header("Frost")]
        [Tooltip("Frost only: the strongest slow it can apply (0.8 = 80% slower), whatever its strength.")]
        [Range(0f, 1f)]
        [SerializeField] private float maxSlow = 0.8f;
        [Header("Ley Charge")]
        [Tooltip("Ley Charge only: how far, in units, a strike can jump from a charged enemy to the next one.")]
        [Min(0f)]
        [SerializeField] private float chainRadius = 6f;
        [Tooltip("Ley Charge only: how many jumps one chain can make before it stops.")]
        [Min(0)]
        [SerializeField] private int maxChainDepth = 3;
        [Header("Debug")]
        [Tooltip("Logs what this debuff does to the Console (Ley Charge: every damage taken, strike and chain hop).")]
        [SerializeField] private bool logDebug = false;

        public ElementalType ElementalType => elementalType;
        public float BaseDuration => baseDuration;
        public float DurationStatScale => durationStatScale;
        public float BaseStrength => baseStrength;
        public float StrengthStatScale => strengthStatScale;
        public float ChanceToApply => chanceToApply;
        public float AdditionalChanceToReapply => additionalChanceToReapply;
        public float TickInterval => tickInterval;
        public GameObject VFX => vfx;
        public DebuffContactSpreadData ContactSpread => contactSpread;
        public DebuffStackData Stacking => stacking;
        public float MaxSlow => maxSlow;
        public float ChainRadius => chainRadius;
        public int MaxChainDepth => maxChainDepth;
        public bool LogDebug => logDebug;
    }

}