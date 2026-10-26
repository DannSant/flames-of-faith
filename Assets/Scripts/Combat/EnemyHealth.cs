using Game.Audio;
using Game.Misc;
using Game.Progression;
using Game.UI;
using System;
using System.Collections;
using UnityEngine;
using static Game.Combat.PlayerHealth;

namespace Game.Combat
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("UI")]
        [SerializeField] private EnemyHealthbar healthbar;

        [Header("Settings")]
        [SerializeField] private float deathDelay = 0.5f; // Delay before destroying the object after death

        [Header("Effects")]
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private AudioClip damagedSFX;
        [SerializeField] private AudioClip deathSFX;
        [SerializeField] private bool shouldSpawnDamageNumbers = true;
        [Tooltip("While immune (the boss's first phase), a hit shows IMMUNE at most once per this many seconds, " +
            "so fast repeated hits (Whirlwind, flame trail ticks, volleys) don't stack a pile of them.")]
        [Min(0f)]
        [SerializeField] private float immuneNumberInterval = 0.4f;

        [Header("Targeting")]
        [Tooltip("Extra targeting reach standing in for the visible body, used only by " +
            "EnemyTargeting when resolving what is in attack range - it is not a collider and " +
            "affects nothing else. Set it on enemies whose sprite is much larger than their " +
            "physics collider: BossAct1 draws 6x6 world units but collides as a circle of radius " +
            "0.98, so without this the player has to close two extra units before the game agrees " +
            "the boss is in range. Leave at 0 for normal-sized enemies whose collider already " +
            "matches their sprite.")]
        [SerializeField] private float targetingBodyRadius = 0f;

        [Header("Misc")]
        [SerializeField] private bool shouldDieOnTouchingAltar = false;

        public event OnHealthChanged onHealthChanged;
        public event OnDeath onDeath;
        public event Action<DamageRequest> onDamageTaken;

        private float maxHealth;
        private float currentHealth;
        private Flash flash;
        private float extraDamageTakenPercentage = 0f;
        private bool isImmune = false;
        private bool deathDetected = false;
        private float lastImmuneNumberTime = float.NegativeInfinity;

        public bool IsImmuneFlag
        {
            get => isImmune;
            set => isImmune = value;
        }

        public bool ShouldDieOnTouchingAltar
        {
            get => shouldDieOnTouchingAltar;
            private set => shouldDieOnTouchingAltar = value;
        }

        /// <summary>
        /// Stand-in for the visible body when it is larger than the physics collider. See the
        /// tooltip on the serialized field, and <see cref="EnemyTargeting"/> for how it is applied.
        /// </summary>
        public float TargetingBodyRadius => targetingBodyRadius;

        //References
        private PlayerHealth playerHealth;


        private void Start()
        {
            currentHealth = maxHealth;
            flash = GetComponent<Flash>();
            playerHealth = Scene.PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void SetMaxHealth(int amount)
        {
            maxHealth = amount;
            currentHealth = maxHealth;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(DamageRequest damageRequest)
        {
            if (damageRequest == null)
            {
                Debug.LogWarning("DamageRequest is null!");
                return;
            }
            float damage = damageRequest.baseDamage;

            if (damagedSFX != null && !damageRequest.silent)
            {
                AudioManager.Instance.PlayEnemyHitSFX(damagedSFX);
            }

            if (IsImmuneFlag)
            {
                // The hitters skip their own number on an immune target; this is the only one shown
                if (shouldSpawnDamageNumbers && !damageRequest.silent && DamageNumberSpawner.Instance != null
                    && ShouldShowImmuneNumber(Time.time, lastImmuneNumberTime, immuneNumberInterval))
                {
                    lastImmuneNumberTime = Time.time;
                    DamageNumberSpawner.Instance.SpawnImmuneNumber(transform.position);
                }
                return;
            }
           
            // Apply damage
            currentHealth -= damage;

            onHealthChanged?.Invoke(currentHealth, maxHealth);
            onDamageTaken?.Invoke(damageRequest);
            healthbar.SetHealth(currentHealth, maxHealth);
            if (flash != null)
            {
                flash.StartFlash();
            }

            if (currentHealth <= 0)
            {
                healthbar.Hide();
                DetectDeath(damageRequest.silent);
                return;
            }

            if (extraDamageTakenPercentage > 0f)
            {
                InflictExtraDamage(damage);
            }

            bool canTriggerLifesteal = damageRequest.canTriggerLifeSteal;
            if (playerHealth != null && canTriggerLifesteal)
            {
                playerHealth.Lifesteal(damage);
            }

        }

        private void InflictExtraDamage(float damage)
        {
            float extraDamage = CalculateExtraDamage(damage, extraDamageTakenPercentage);
            currentHealth -= extraDamage;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            healthbar.SetHealth(currentHealth, maxHealth);
            DamageNumberSpawner.Instance.SpawnFrostDebuffDamageNumber(transform.position, extraDamage);
            if (currentHealth <= 0)
            {
                healthbar.Hide();
                DetectDeath();
            }
        }

        /// <summary>
        /// The extra damage Frost adds on top of a hit: <paramref name="percentage"/> of it, at least 1. It used to
        /// subtract the hit's damage a second time instead, so Frost doubled every hit whatever its strength.
        /// </summary>
        public static float CalculateExtraDamage(float damage, float percentage) => Mathf.Max(damage * percentage, 1f);

        public void SetExtraDamageTakenPercentage(float percentage)
        {
            extraDamageTakenPercentage = percentage;
        }

        public void ResetExtraDamageTakenPercentage()
        {
            extraDamageTakenPercentage = 0f;
        }

        public bool DiedSilently { get; private set; }

        private void DetectDeath(bool silent = false)
        {
            // TakeDamage has no already-dead guard and the collider stays live through the death
            // delay, so lingering projectiles and DoT ticks can still land on a corpse. Without
            // this each one would start another DeathRoutine and fire onDeath again - duplicate
            // XP drops and death VFX, and for the boss a duplicate NotifyBossDied/end screen.
            if (deathDetected) return;
            deathDetected = true;

            DiedSilently = silent;

            if (deathSFX != null && !silent)
            {
                AudioManager.Instance.PlayEnemyDeathSFX(deathSFX);
            }
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(deathDelay);
            if (deathVfxPrefab != null)
            {
                var instancedVfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
                GameObject.Destroy(instancedVfx, 2f);
            }
            onDeath?.Invoke();
        }

        public bool ShouldSpawnDamageNumber()
        {
            return shouldSpawnDamageNumbers;
        }

        public bool ShouldSpawnEffectObject()
        {
            return true;
        }

        public bool IsImmune()
        {
            return isImmune;
        }

        /// <summary>IMMUNE shows on the first hit, then at most once per <paramref name="interval"/> seconds.</summary>
        public static bool ShouldShowImmuneNumber(float now, float lastShown, float interval) => now - lastShown >= interval;

        public bool IsDead()
        {
            return currentHealth <= 0;
        }

#if UNITY_EDITOR
        // Lets targetingBodyRadius be tuned against the sprite by eye: select the enemy and grow
        // the circle until it matches the drawn silhouette, not the sprite's padded bounds.
        private void OnDrawGizmosSelected()
        {
            if (targetingBodyRadius <= 0f) return;

            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, targetingBodyRadius);
        }
#endif
    }
}
