using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Tunable values for the Pierce stat: extra enemies a projectile passes through, and the
    /// chance for a linear arrow to split on hit (ProjectileSplitOnHit).
    /// Loaded from Resources/Combat/PierceSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "PierceSettings", menuName = "Combat/Pierce Settings")]
    public class PierceSettings : ScriptableObject
    {
        private const string ResourcePath = "Combat/PierceSettings";

        [Header("Extra Targets")]
        [Tooltip("Extra enemies pierced per Pierce point, rounded down (0.25 = one more enemy every 4 points).")]
        [Min(0f)]
        [SerializeField] private float extraPiercePerPoint = 0.25f;

        [Header("Split Arrows")]
        [Tooltip("Chance per Pierce point for a linear arrow to split when it hits an enemy (0.03 = 3%).")]
        [Min(0f)]
        [SerializeField] private float splitChancePerPoint = 0.03f;
        [Tooltip("Highest split chance (0.4 = 40%).")]
        [Range(0f, 1f)]
        [SerializeField] private float splitMaxChance = 0.4f;
        [Tooltip("Damage of the split arrow as a fraction of the original's base damage.")]
        [Range(0f, 1f)]
        [SerializeField] private float splitDamageMultiplier = 0.5f;
        [Tooltip("How far a split arrow flies before it's removed.")]
        [Min(0f)]
        [SerializeField] private float splitMaxTravelDistance = 4f;

        private static PierceSettings instance;

        public float ExtraPiercePerPoint => extraPiercePerPoint;
        public float SplitChancePerPoint => splitChancePerPoint;
        public float SplitMaxChance => splitMaxChance;
        public float SplitDamageMultiplier => splitDamageMultiplier;
        public float SplitMaxTravelDistance => splitMaxTravelDistance;

        public static PierceSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<PierceSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"PierceSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<PierceSettings>();
                    }
                }
                return instance;
            }
        }

        public int GetExtraPierce(int piercePoints) =>
            StatsCalculations.CalculateExtraPierce(piercePoints, extraPiercePerPoint);

        public float GetSplitChance(int piercePoints) =>
            StatsCalculations.CalculateSplitChance(piercePoints, splitChancePerPoint, splitMaxChance);
    }
}
