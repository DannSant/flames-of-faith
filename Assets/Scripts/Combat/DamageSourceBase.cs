using Game.Control;
using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class DamageSourceBase : MonoBehaviour, IEffectMultiplier
    {
        [Header("Damage")]
        [SerializeField] private int baseDamage = 5;
        [SerializeField] private WeaponClass weaponClass = WeaponClass.None;
        [SerializeField] private float attackScale = 1f;
        [SerializeField] private string effectID;
        [SerializeField] private string layerMaskName = "Enemy";

        [Header("Hit Mode")]
        [Tooltip("If true, we deal damage when something ENTERS the trigger.")]
        [SerializeField] private bool useTriggerEnter = true;

        [Tooltip("If true, we deal damage periodically while something STAYS in the trigger.")]
        [SerializeField] private bool useTriggerStay = false;

        [Tooltip("Interval in seconds for OnTriggerStay ticks.")]
        [SerializeField] private float tickInterval = 1f;

        [Tooltip("If true, multiple overlapping instances can each tick damage. " +
             "If false, you may later centralize stacking per target.")]
        [SerializeField] private bool shouldStackDamage = true;

        [Header("Pierce")]
        [SerializeField] private bool enablePierce = false;
        [SerializeField] private int pierceCount = 1;

        [Header("Knockback")]
        [SerializeField] private bool applyKnockback = false;
        [SerializeField] private float knockbackForce = 5f;

        private EffectStore effectStore;
        private PlayerProgression playerProgression;
        private float stayTimer = 0f;

        int enemyLayerMask;

        // In case something external wants to know when we dealt damage
        public event Action<float, GameObject> OnDamageDealtEvent;

        private void Awake()
        {
            enemyLayerMask = LayerMask.NameToLayer(layerMaskName);
        }

        private void Start()
        {
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        }

        public void Initialize(int baseDamage, int pierceAmount, string effectId, WeaponClass weaponClass)
        {
            this.baseDamage = baseDamage;
            this.pierceCount = pierceAmount;
            this.effectID = effectId;
            this.weaponClass = weaponClass;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!useTriggerEnter) return;

            if ( other.gameObject.layer == enemyLayerMask)
            {
                TryApplyDamage(other);
            }
                
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!useTriggerStay) return;

            // Basic per-instance tick timer
            stayTimer += Time.deltaTime;
            if (stayTimer < tickInterval) return;

            stayTimer = 0f;

            // For now, shouldStackDamage is just a config hint; we always allow
            // each instance to tick independently like your current ContinuousDamageSource.
            // Later we can centralize stacking per-target if you want.
            if (other.gameObject.layer == enemyLayerMask)
            {
                TryApplyDamage(other);
            }
        }

        private void TryApplyDamage(Collider2D collision)
        {
            if (collision == null) return;

            // IDamageable check (enemy, etc.)
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float totalDamage = CalculateTotalDamage();
                damageable.TakeDamage(totalDamage);

                DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(
                    collision.transform.position,
                    totalDamage
                );
              
                OnDamageDealtEvent?.Invoke(totalDamage, collision.gameObject);
            }

            // Knockback (optional)
            if (applyKnockback)
            {
                Knockback knockback = collision.GetComponent<Knockback>();
                if (knockback != null)
                {
                    var playerTransform = PlayerManager.Instance
                        .GetPlayerComponent<PlayerController>()
                        .transform;

                    knockback.ApplyKnockback(playerTransform, knockbackForce);
                }
            }

            // Pierce (optional)
            if (enablePierce)
            {
                pierceCount--;
                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        private float CalculateTotalDamage()
        {          
            if (effectStore == null || playerProgression == null)
            {
                Debug.LogWarning("DamageSourceBase: Missing EffectStore or PlayerProgression.");
                return baseDamage;
            }
           
            return DamageCalculator.CalculateTotalDamage(
                new DamageRequest(
                    baseDamage,
                    effectStore,
                    effectID,
                    weaponClass,
                    playerProgression,
                    attackScale
                )
            );
        }

        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }

        public void SetBaseDamage(int value) => baseDamage = value;
        public void SetWeaponClass(WeaponClass wc) => weaponClass = wc;
        public void SetAttackScale(float scale) => attackScale = scale;
        public void EnableTriggerEnter(bool enabled) => useTriggerEnter = enabled;
        public void EnableTriggerStay(bool enabled, float interval = 1f)
        {
            useTriggerStay = enabled;
            tickInterval = interval;
        }

        public void ConfigurePierce(bool enabled, int count)
        {
            enablePierce = enabled;
            pierceCount = count;
        }

        
    }

}