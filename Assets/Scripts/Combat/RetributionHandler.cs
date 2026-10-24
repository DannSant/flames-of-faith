using Game.Misc;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Drives the Retribution stat's two effects: reflecting a portion of incoming damage back at
    /// the attacker, and rolling a chance to generate Grace whenever the equipped weapon lands a
    /// hit. See <see cref="PlayerHealth.onDamageTaken"/> and <see cref="WeaponBase.OnWeaponDamageDealt"/>.
    /// </summary>
    /// <summary>The numbers behind Retribution, on PlayerStatSettings (gameplay and the stat tooltip read the same values).</summary>
    [System.Serializable]
    public struct RetributionCurve
    {
        [Tooltip("Retribution points up to which each point reflects Damage Tier 1 Rate % of the damage taken.")]
        public float damageTier1Cap;
        [Tooltip("Retribution points up to which each point past Tier 1 Cap reflects Damage Tier 2 Rate %.")]
        public float damageTier2Cap;
        [Tooltip("Retribution points past which more points reflect nothing more.")]
        public float damageHardCap;
        [Tooltip("% of the damage taken reflected per point, up to Damage Tier 1 Cap.")]
        public float damageTier1Rate;
        [Tooltip("% of the damage taken reflected per point between Damage Tier 1 Cap and Tier 2 Cap.")]
        public float damageTier2Rate;
        [Tooltip("% of the damage taken reflected per point between Damage Tier 2 Cap and the Hard Cap.")]
        public float damageTier3Rate;
        [Tooltip("Reflected damage also adds this share of the player's Melee Damage stat (0.2 = 20%).")]
        public float meleeDamageScaling;
        [Tooltip("Retribution points up to which each point adds Grace Tier 1 Rate % chance to gain Grace on a weapon hit.")]
        public float graceTier1Cap;
        [Tooltip("Retribution points past which more points add no more Grace chance.")]
        public float graceHardCap;
        [Tooltip("% chance to gain Grace on a weapon hit per point, up to Grace Tier 1 Cap.")]
        public float graceTier1Rate;
        [Tooltip("% chance to gain Grace on a weapon hit per point between Grace Tier 1 Cap and the Hard Cap.")]
        public float graceTier2Rate;
        [Tooltip("Grace gained each time the chance succeeds.")]
        public int graceGeneratedAmount;
        [Tooltip("Minimum time, in seconds, between Grace gains from Retribution.")]
        public float graceGainCooldown;

        public float GetReflectPercent(int points) => StatsCalculations.CalculateRetributionDamagePercent(
            points, damageTier1Cap, damageTier2Cap, damageHardCap, damageTier1Rate, damageTier2Rate, damageTier3Rate);

        public float GetGraceChance(int points) => StatsCalculations.CalculateRetributionGraceChance(
            points, graceTier1Cap, graceHardCap, graceTier1Rate, graceTier2Rate);
    }

    public class RetributionHandler : MonoBehaviour
    {
        // The curve's numbers are on PlayerStatSettings
        [SerializeField] private GameObject damageReflectionEffectPrefab;

        private PlayerHealth playerHealth;
        private PlayerProgression playerProgression;

        public RetributionCurve Curve => PlayerStatSettings.Instance.Retribution;
        private PlayerGrace playerGrace;
        private WeaponBase subscribedWeapon;
        private float nextGraceGainTime;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();

            var weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            subscribedWeapon = weaponManager != null ? weaponManager.GetCurrentWeapon() : null;
            if (subscribedWeapon != null)
            {
                subscribedWeapon.OnWeaponDamageDealt += HandleWeaponDamageDealt;
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.onDamageTaken += HandleDamageTaken;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.onDamageTaken -= HandleDamageTaken;
            }

            if (subscribedWeapon != null)
            {
                subscribedWeapon.OnWeaponDamageDealt -= HandleWeaponDamageDealt;
            }
        }

        private void HandleDamageTaken(float damageAmount, GameObject attacker)
        {
            //Debug.Log($"RetributionHandler: HandleDamageTaken called with damageAmount={damageAmount}, attacker={attacker?.name}");
            if (attacker == null) return;

            IDamageable damageable = attacker.GetComponent<IDamageable>();
            if (damageable == null || damageable.IsDead()) return;

            int retributionStat = playerProgression.GetStatTotal(StatType.Retribution);
            if (retributionStat <= 0) return;

            var curve = Curve;
            float damagePercent = curve.GetReflectPercent(retributionStat);

            float meleeDamageStat = playerProgression.GetStatTotal(StatType.MeleeDamage);
            float reflectedDamage = damageAmount * damagePercent + meleeDamageStat * curve.meleeDamageScaling;
            if (reflectedDamage <= 0f) return;

            damageable.TakeDamage(new DamageRequest(reflectedDamage, WeaponClass.Melee, false));

            if (damageReflectionEffectPrefab != null)
            {
                GameObject effectInstance = Instantiate(damageReflectionEffectPrefab, attacker.transform.position, Quaternion.identity);
                Destroy(effectInstance, 0.6f);
            }

            if (damageable.ShouldSpawnDamageNumber() && !damageable.IsImmune())
            {
                DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(attacker.transform.position, reflectedDamage, WeaponClass.Melee);
            }
        }

        private void HandleWeaponDamageDealt(float damage, GameObject target)
        {
            //Debug.Log($"RetributionHandler: HandleWeaponDamageDealt called with damage={damage}, target={target?.name}");
            int retributionStat = playerProgression.GetStatTotal(StatType.Retribution);
            if (retributionStat <= 0) return;

            if (Time.time < nextGraceGainTime) return;

            var curve = Curve;
            if (Random.value <= curve.GetGraceChance(retributionStat))
            {
                playerGrace.AddGrace(curve.graceGeneratedAmount);
                nextGraceGainTime = Time.time + curve.graceGainCooldown;
            }
        }
    }
}
