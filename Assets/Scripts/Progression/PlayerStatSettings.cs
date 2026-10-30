using Game.Combat;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Tunable numbers behind player stats that used to be hardcoded in gameplay scripts. Gameplay
    /// and the stat tooltips (StatBenefitDescriber) read the same values, so they can't drift.
    /// Loaded from Resources/Progression/PlayerStatSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStatSettings", menuName = "Progression/Player Stat Settings")]
    public class PlayerStatSettings : ScriptableObject
    {
        private const string ResourcePath = "Progression/PlayerStatSettings";

        [Header("Health Regen")]
        [Tooltip("Seconds between each 1 HP healed at 1 point of Health Regen.")]
        [Min(0.01f)]
        [SerializeField] private float regenBaseInterval = 5f;
        [Tooltip("Interval = base / (1 + (points - 1) / this). Lower = each point speeds regen up more.")]
        [Min(0.01f)]
        [SerializeField] private float regenIntervalDivisor = 2.25f;

        [Header("Life Steal")]
        [Tooltip("Chance to heal on a hit with 1+ Life Steal, before the per-point bonus (0.2 = 20%).")]
        [Range(0f, 1f)]
        [SerializeField] private float lifestealBaseChance = 0.2f;
        [Tooltip("Extra chance per Life Steal point (0.01 = 1%).")]
        [SerializeField] private float lifestealChancePerPoint = 0.01f;
        [Tooltip("Health healed per Life Steal point, clamped between the min and max below.")]
        [SerializeField] private float lifestealHealPerPoint = 0.1f;
        [Tooltip("Smallest heal from Life Steal.")]
        [SerializeField] private float lifestealMinHeal = 1f;
        [Tooltip("Largest heal from Life Steal.")]
        [SerializeField] private float lifestealMaxHeal = 20f;

        [Header("Dash Recovery")]
        [Tooltip("Dash cooldown = base / (1 + points × this).")]
        [Min(0f)]
        [SerializeField] private float dashCooldownScalePerPoint = 0.08f;

        [Header("Shop Discount")]
        [Tooltip("1% off per point up to this many points, then 0.5% per point.")]
        [Min(0)]
        [SerializeField] private int shopDiscountSoftCap = 50;
        [Tooltip("Points beyond this give no more discount.")]
        [Min(0)]
        [SerializeField] private int shopDiscountHardCap = 80;

        [Header("Attack Speed")]
        [Tooltip("Every Attack Speed point counts in full up to this many points.")]
        [Min(0)]
        [SerializeField] private int attackSpeedSoftCap = 30;
        [Tooltip("Attack Speed points beyond this make attacks no faster.")]
        [Min(0)]
        [SerializeField] private int attackSpeedHardCap = 60;
        [Tooltip("What each Attack Speed point between the soft cap and the hard cap counts for (0.5 = half a point).")]
        [Range(0f, 1f)]
        [SerializeField] private float attackSpeedPastSoftCapRate = 0.5f;

        [Header("Pickup Range")]
        [Tooltip("Distance at which pickups start flying to the player, with 0 Pickup Range.")]
        [Min(0f)]
        [SerializeField] private float pickupBaseRange = 2f;
        [Tooltip("Extra pickup distance per Pickup Range point.")]
        [Min(0f)]
        [SerializeField] private float pickupRangePerPoint = 0.1f;

        [Header("Movement")]
        [Tooltip("Walking speed, in units per second, with 0 Move Speed points.")]
        [Min(0f)]
        [SerializeField] private float baseMoveSpeed = 3.5f;
        [Tooltip("Walking speed, in units per second, added per Move Speed point.")]
        [SerializeField] private float moveSpeedPerPoint = 0.01f;

        [Header("Damage Taken")]
        [Tooltip("Seconds the player can't be hurt again after taking a hit.")]
        [Min(0f)]
        [SerializeField] private float invulnerabilityAfterHit = 0.5f;

        [Header("Retribution")]
        [SerializeField] private RetributionCurve retribution = new RetributionCurve
        {
            damageTier1Cap = 35f, damageTier2Cap = 50f, damageHardCap = 70f,
            damageTier1Rate = 1f, damageTier2Rate = 0.2f, damageTier3Rate = 0.01f,
            meleeDamageScaling = 0.2f,
            graceTier1Cap = 25f, graceHardCap = 50f, graceTier1Rate = 1f, graceTier2Rate = 0.5f,
            graceGeneratedAmount = 1, graceGainCooldown = 20f,
        };

        [Header("Luck: Level-up Upgrade Tiers")]
        [Tooltip("Base chance weight of each upgrade tier (1-4). Each Luck point moves 1 from tier 1, split evenly over tiers 2-4.")]
        [SerializeField] private float[] upgradeTierBaseChances = { 80f, 10f, 8f, 2f };

        private static PlayerStatSettings instance;

        public float RegenBaseInterval => regenBaseInterval;
        public float RegenIntervalDivisor => regenIntervalDivisor;
        public float LifestealBaseChance => lifestealBaseChance;
        public float LifestealChancePerPoint => lifestealChancePerPoint;
        public float LifestealHealPerPoint => lifestealHealPerPoint;
        public float LifestealMinHeal => lifestealMinHeal;
        public float LifestealMaxHeal => lifestealMaxHeal;
        public float DashCooldownScalePerPoint => dashCooldownScalePerPoint;
        public int ShopDiscountSoftCap => shopDiscountSoftCap;
        public int ShopDiscountHardCap => shopDiscountHardCap;
        public int AttackSpeedSoftCap => attackSpeedSoftCap;
        public int AttackSpeedHardCap => attackSpeedHardCap;
        public float PickupBaseRange => pickupBaseRange;
        public float PickupRangePerPoint => pickupRangePerPoint;
        public float[] UpgradeTierBaseChances => upgradeTierBaseChances;
        public float BaseMoveSpeed => baseMoveSpeed;
        public float MoveSpeedPerPoint => moveSpeedPerPoint;
        public float InvulnerabilityAfterHit => invulnerabilityAfterHit;
        public RetributionCurve Retribution => retribution;

        public static PlayerStatSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<PlayerStatSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"PlayerStatSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<PlayerStatSettings>();
                    }
                }
                return instance;
            }
        }

        public float GetRegenInterval(int points) =>
            StatsCalculations.CalculateHealthRegenInterval(points, regenBaseInterval, regenIntervalDivisor);

        public float GetLifestealChance(int points) =>
            StatsCalculations.CalculateLifestealChance(points, lifestealBaseChance, lifestealChancePerPoint);

        public float GetLifestealHeal(int points) =>
            StatsCalculations.CalculateLifestealHeal(points, lifestealHealPerPoint, lifestealMinHeal, lifestealMaxHeal);

        public float GetDashCooldown(int points, float baseCooldown) =>
            StatsCalculations.CalculateDashCooldown(points, baseCooldown, dashCooldownScalePerPoint);

        public float GetShopDiscountMultiplier(int points) =>
            StatsCalculations.CalculateShopDiscountMultiplier(points, shopDiscountSoftCap, shopDiscountHardCap);

        public float GetEffectiveAttackSpeed(int points) =>
            StatsCalculations.CalculateEffectiveAttackSpeed(points, attackSpeedSoftCap, attackSpeedHardCap, attackSpeedPastSoftCapRate);

        /// <summary>The one cooldown formula: the weapons and the Attack Speed tooltip both use it.</summary>
        public float GetAttackDelay(int points, float baseCooldown, float attackSpeedScale) =>
            StatsCalculations.CalculateAttackDelay(GetEffectiveAttackSpeed(points), baseCooldown, attackSpeedScale);

        public float GetPickupRange(int points) =>
            StatsCalculations.CalculatePickupRange(points, pickupBaseRange, pickupRangePerPoint);

        public float[] GetUpgradeTierChances(int luck) =>
            StatsCalculations.CalculateUpgradeTierChances(luck, upgradeTierBaseChances);
    }
}
