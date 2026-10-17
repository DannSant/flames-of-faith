using Game.Combat.Elemental;
using Game.Control;
using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class DamageSourceBase : MonoBehaviour, IEffectMultiplier, IDamageDealtNotifier, IStackScalable
    {
        [Header("Damage")]
        [SerializeField] private int baseDamage = 5;
        [SerializeField] private WeaponClass weaponClass = WeaponClass.None;
        [SerializeField] private float attackScale = 1f;
        [SerializeField] private string effectID;
        [SerializeField] private string layerMaskName = "Enemy";
        [SerializeField] private LayerMask enemyLayerMask = 0;
        [SerializeField] private DamageOriginType originType = DamageOriginType.Weapon;

        [Header("Hit Mode")]
        [Tooltip("If true, we deal damage when something ENTERS the trigger.")]
        [SerializeField] private bool useTriggerEnter = true;

        [Tooltip("If true, we deal damage periodically while something STAYS in the trigger.")]
        [SerializeField] private bool useTriggerStay = false;

        [Tooltip("Seconds between hits on the same target while it stays inside. Each target has its " +
            "own timer and is hit as soon as it enters, so a target that only passes through still takes a hit.")]
        [SerializeField] private float tickInterval = 1f;

        [Tooltip("If true, multiple overlapping instances can each tick damage. " +
             "If false, you may later centralize stacking per target.")]
        [SerializeField] private bool shouldStackDamage = true;

        [Header("Pierce")]
        [SerializeField] private bool enablePierce = false;
        [SerializeField] private int pierceCount = 1;
        [Tooltip("Add the Pierce stat's extra targets to Pierce Count when spawned. For projectiles " +
            "spawned straight from a prefab (e.g. Strafe arrows); weapons pass their own pierce through Initialize.")]
        [SerializeField] private bool addPierceStat = false;

        // Set when a spawner sets the pierce itself (Initialize, ConfigurePierce), so the Pierce
        // stat isn't added on top of a value that already includes it (or a split arrow's single hit).
        private bool pierceSetExplicitly;

        [Header("Knockback")]
        [SerializeField] private bool applyKnockback = false;
        [SerializeField] private float knockbackForce = 5f;

        [Header("Lifesteal")]
        [SerializeField] private bool canTriggerLifesteal = true;

        [Header("Direct Elemental Debuff")]
        [Tooltip("Elemental debuff applied directly by this damage source, independent of any weapon/effect data. " +
            "Useful for damage sources not tied to a WeaponData asset (e.g. an AoE explosion prefab).")]
        [SerializeField] private ElementalDebuffData directElementalDebuffData;

        private EffectStore effectStore;
        private PlayerProgression playerProgression;
        private float stackDamageMultiplier = 1f;
        // Trigger-stay mode: when each target can be hit next. Per target, so several targets
        // inside don't share (and split) one timer, and a quick pass still lands the first hit.
        private readonly Dictionary<Collider2D, float> nextStayHitTime = new();
        private WeaponData weaponData;
        
        public DamageOriginType OriginType => originType;

        // Exposed so sibling components that propagate this hit (e.g. AreaDamageOnHit) stay in
        // sync with it instead of duplicating the serialized fields on their own inspector.
        public WeaponClass WeaponClass => weaponClass;
        public bool CanTriggerLifesteal => canTriggerLifesteal;
        public int PierceCount => pierceCount;
        public float KnockbackForce => knockbackForce;

        public void SetKnockbackForce(float value) => knockbackForce = value;

        // In case something external wants to know when we dealt damage
        public event Action<float, GameObject> OnDamageDealtEvent;

        private void Start()
        {
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();

            if (addPierceStat && enablePierce && !pierceSetExplicitly && playerProgression != null && PierceSettings.Instance != null)
            {
                pierceCount += PierceSettings.Instance.GetExtraPierce(playerProgression.GetStatTotal(StatType.Pierce));
            }
        }

        public void Initialize(int baseDamage, int pierceAmount, string effectId, WeaponClass weaponClass, WeaponData weaponData)
        {
            this.baseDamage = baseDamage;
            this.pierceCount = pierceAmount;
            pierceSetExplicitly = true;
            this.effectID = effectId;
            this.weaponClass = weaponClass;
            this.weaponData = weaponData;

            // Adopt the firing weapon's attack scale so special attacks actually apply their
            // multiplier. The prefab's own value stays as the fallback for sources spawned
            // without weapon data (effect-spawned projectiles, AoE prefabs).
            if (weaponData != null)
            {
                this.attackScale = weaponData.attackScale;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (useTriggerStay)
            {
                // Stay mode hits on contact too (see TryApplyStayDamage)
                TryApplyStayDamage(other);
            }

            if (!useTriggerEnter) return;

            if (((1 << other.gameObject.layer) & enemyLayerMask) != 0)
            {
                TryApplyDamage(other);
            }
                
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!useTriggerStay) return;
            TryApplyStayDamage(other);
        }

        // shouldStackDamage is still only a config hint: every instance ticks on its own, so
        // overlapping sources (e.g. several flame segments) each hit.
        private void TryApplyStayDamage(Collider2D other)
        {
            if (other == null || ((1 << other.gameObject.layer) & enemyLayerMask) == 0) return;

            float now = Time.time;
            if (nextStayHitTime.TryGetValue(other, out float nextHit) && now < nextHit) return;

            nextStayHitTime[other] = now + tickInterval;
            TryApplyDamage(other);
        }

        private void TryApplyDamage(Collider2D collision)
        {
            if (collision == null) return;
            if (ignoredTargets != null && ignoredTargets.Contains(collision.gameObject)) return;

            // IDamageable check (enemy, etc.)
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {              
                float totalDamage = CalculateTotalDamage();
                damageable.TakeDamage(new DamageRequest(totalDamage,weaponClass, canTriggerLifesteal));

                // TakeDamage above already no-ops on an immune target (e.g. the boss's first
                // phase), but it still needs telling apart here - otherwise this spawns a
                // real-looking damage number for damage that never actually landed.
                if (damageable.ShouldSpawnDamageNumber() && !damageable.IsImmune())
                {
                    DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(collision.transform.position, totalDamage, weaponClass);
                }

                
              
                OnDamageDealtEvent?.Invoke(totalDamage, collision.gameObject);

                // Elemental Debuff (optional) - skipped on an immune target so it can't pick up a
                // real DoT (which would then tick its own real-looking damage numbers) from a hit
                // that never actually landed.
                if (!damageable.IsImmune())
                {
                    TryApplyElementalDebuff(collision.gameObject);
                }
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

        /// <summary>
        /// Applies this source's on-hit elemental debuffs to a target it did not hit directly.
        /// Used by sibling components that propagate the hit (e.g. <see cref="AreaDamageOnHit"/>)
        /// so they don't have to re-implement the weapon/effect/direct debuff lookups.
        /// </summary>
        public void ApplyOnHitEffectsTo(GameObject target) => TryApplyElementalDebuff(target);

        private void TryApplyElementalDebuff(GameObject target)
        {
            if (target == null) return;

            //Weapon debuff check
            var debuffHandler = target.GetComponent<DebuffHandler>();
            if (debuffHandler == null) return;

            if (weaponData!=null && weaponData.elementalDebuffData!= null && weaponData.elementalDebuffData.ElementalType != ElementalType.None)
            {
                var weaponDebuffData = weaponData.elementalDebuffData;
                int debuffStrengthStat = playerProgression.GetStatTotal(StatType.MastowAffinity);                
                debuffHandler.TryToApplyDebuff(weaponDebuffData, debuffStrengthStat);
            }

            //Effect debuff check
            var debuffsToApply = effectStore.GetElementalTypesToApply(originType, weaponClass);
            foreach (var debuffData in debuffsToApply)
            {
                int debuffStrengthStat = playerProgression.GetStatTotal(StatType.MastowAffinity) + debuffData.count;
                debuffHandler.TryToApplyDebuff(debuffData.elementalDebuffData, debuffStrengthStat);
            }

            //Direct debuff check (assigned straight on this component, e.g. an AoE explosion prefab)
            if (directElementalDebuffData != null && directElementalDebuffData.ElementalType != ElementalType.None)
            {
                int debuffStrengthStat = playerProgression.GetStatTotal(StatType.MastowAffinity);
                debuffHandler.TryToApplyDebuff(directElementalDebuffData, debuffStrengthStat);
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
                new DamageCalculationRequest(
                    baseDamage,
                    effectStore,
                    effectID,
                    weaponClass,
                    playerProgression,
                    attackScale,
                    stackDamageMultiplier
                )
            );
        }

        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }

        public void ApplyStackScaling(StackScaling scaling)
        {
            stackDamageMultiplier = scaling.Get(StackScalingTarget.Damage);

            float pierceScale = scaling.Get(StackScalingTarget.Pierce);
            if (pierceScale != 1f)
            {
                pierceCount = Mathf.Max(1, Mathf.RoundToInt(pierceCount * pierceScale));
            }
        }

        public void SetElementalDebuffData(ElementalDebuffData debuffData) => directElementalDebuffData = debuffData;

        public void SetBaseDamage(int value) => baseDamage = value;
        public int BaseDamage => baseDamage;

        // Targets this source never damages, e.g. the enemy a split arrow broke off from (it spawns
        // inside that enemy's collider and would otherwise hit it again immediately).
        private HashSet<GameObject> ignoredTargets;

        public void IgnoreTarget(GameObject target)
        {
            if (target == null) return;
            ignoredTargets ??= new HashSet<GameObject>();
            ignoredTargets.Add(target);
        }
        public void SetWeaponClass(WeaponClass wc) => weaponClass = wc;
        public void SetAttackScale(float scale) => attackScale = scale;
        public float AttackScale => attackScale;
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
            pierceSetExplicitly = true;
        }

    }

}