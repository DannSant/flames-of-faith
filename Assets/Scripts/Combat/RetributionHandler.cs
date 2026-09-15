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
    public class RetributionHandler : MonoBehaviour
    {
        [Header("Damage Reflection")]
        [SerializeField] private float damageTier1Cap = 35f;
        [SerializeField] private float damageTier2Cap = 50f;
        [SerializeField] private float damageHardCap = 70f;
        [SerializeField] private float damageTier1Rate = 1f;
        [SerializeField] private float damageTier2Rate = 0.2f;
        [SerializeField] private float damageTier3Rate = 0.01f;
        [SerializeField] private float meleeDamageScaling = 0.2f;
        [SerializeField] private GameObject damageReflectionEffectPrefab;

        [Header("Grace Generation")]
        [SerializeField] private float graceTier1Cap = 25f;
        [SerializeField] private float graceHardCap = 50f;
        [SerializeField] private float graceTier1Rate = 1f;
        [SerializeField] private float graceTier2Rate = 0.5f;
        [SerializeField] private int graceGeneratedAmount = 1;
        [Tooltip("Minimum time, in seconds, between Grace gains from Retribution.")]
        [SerializeField] private float graceGainCooldown = 20f;

        private PlayerHealth playerHealth;
        private PlayerProgression playerProgression;
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

            float damagePercent = StatsCalculations.CalculateRetributionDamagePercent(
                retributionStat, damageTier1Cap, damageTier2Cap, damageHardCap,
                damageTier1Rate, damageTier2Rate, damageTier3Rate);

            float meleeDamageStat = playerProgression.GetStatTotal(StatType.MeleeDamage);
            float reflectedDamage = damageAmount * damagePercent + meleeDamageStat * meleeDamageScaling;
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

            float graceChance = StatsCalculations.CalculateRetributionGraceChance(
                retributionStat, graceTier1Cap, graceHardCap, graceTier1Rate, graceTier2Rate);

            if (Random.value <= graceChance)
            {
                playerGrace.AddGrace(graceGeneratedAmount);
                nextGraceGainTime = Time.time + graceGainCooldown;
            }
        }
    }
}
