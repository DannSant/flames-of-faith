using UnityEngine;

namespace Game.Combat.Elemental
{
    [CreateAssetMenu(menuName = "Debuff/Elemental data")]
    public class ElementalDebuffData : ScriptableObject
    {
        [SerializeField] private ElementalType elementalType = ElementalType.None;
        [SerializeField] private float baseDuration = 0f;
        [SerializeField] private float durationStatScale = 0.25f;
        [SerializeField] private float baseStrength = 0f;
        [SerializeField] private float strengthStatScale = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float chanceToApply = 1f;
        [Tooltip("Seconds between damage-over-time ticks, for debuffs that tick.")]
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private GameObject vfx;
        [SerializeField] private DebuffContactSpreadData contactSpread = new DebuffContactSpreadData();
        [SerializeField] private DebuffStackData stacking = new DebuffStackData();

        public ElementalType ElementalType => elementalType;
        public float BaseDuration => baseDuration;
        public float DurationStatScale => durationStatScale;
        public float BaseStrength => baseStrength;
        public float StrengthStatScale => strengthStatScale;
        public float ChanceToApply => chanceToApply;
        public float TickInterval => tickInterval;
        public GameObject VFX => vfx;
        public DebuffContactSpreadData ContactSpread => contactSpread;
        public DebuffStackData Stacking => stacking;
    }

}