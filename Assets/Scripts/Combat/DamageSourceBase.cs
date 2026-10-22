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
        [Tooltip("This source's design numbers: damage, hit mode, pierce, knockback, debuff, and its projectile's " +
            "speed and lifetime. Edit them on the asset (Resources/Combat/DamageSources), not here.")]
        [SerializeField] private DamageSourceData data;

        [Header("Wiring")]
        [SerializeField] private string effectID;
        [SerializeField] private string layerMaskName = "Enemy";
        [SerializeField] private LayerMask enemyLayerMask = 0;
        [SerializeField] private DamageOriginType originType = DamageOriginType.Weapon;

        // The design numbers, loaded from data (EnsureLoaded) and then changed at runtime by spawners
        // (Initialize, the setters below) and item stacking. Not serialized: the asset is their only source.
        // The defaults are for a source without data (tests, a component added in code).
        private int baseDamage = 5;
        private WeaponClass weaponClass = WeaponClass.None;
        private float attackScale = 1f;
        private bool useTriggerEnter = true;
        // Trigger stay: each target has its own timer and is hit as soon as it enters, so a target
        // that only passes through still takes a hit.
        private bool useTriggerStay = false;
        private float tickInterval = 1f;
        // Still only a config hint: every instance ticks on its own.
        private bool shouldStackDamage = true;
        private bool enablePierce = false;
        private int pierceCount = 1;
        private bool addPierceStat = false;
        private bool applyKnockback = false;
        private float knockbackForce = 5f;
        private bool canTriggerLifesteal = true;
        private ElementalDebuffData directElementalDebuffData;

        // Set once the values above are loaded from data. Every entry point loads first, because a
        // spawner can configure a source before its object is active, i.e. before Awake.
        private bool loaded;

        // Set when a spawner sets the pierce itself (Initialize, ConfigurePierce), so the Pierce
        // stat isn't added on top of a value that already includes it (or a split arrow's single hit).
        private bool pierceSetExplicitly;

        private EffectStore effectStore;
        private PlayerProgression playerProgression;
        private float stackDamageMultiplier = 1f;
        // Trigger-stay mode: when each target can be hit next. Per target, so several targets
        // inside don't share (and split) one timer, and a quick pass still lands the first hit.
        private readonly Dictionary<Collider2D, float> nextStayHitTime = new();
        private WeaponData weaponData;
        
        public DamageSourceData Data => data;
        public DamageOriginType OriginType => originType;

        // Exposed so sibling components that propagate this hit (e.g. AreaDamageOnHit) stay in
        // sync with it instead of duplicating the serialized fields on their own inspector.
        public WeaponClass WeaponClass { get { EnsureLoaded(); return weaponClass; } }
        public bool CanTriggerLifesteal { get { EnsureLoaded(); return canTriggerLifesteal; } }
        public int PierceCount { get { EnsureLoaded(); return pierceCount; } }
        public float KnockbackForce { get { EnsureLoaded(); return knockbackForce; } }

        public void SetKnockbackForce(float value) { EnsureLoaded(); knockbackForce = value; }

        // In case something external wants to know when we dealt damage
        public event Action<float, GameObject> OnDamageDealtEvent;

        private void Awake() => EnsureLoaded();

        private void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            if (data == null) return;

            baseDamage = data.BaseDamage;
            weaponClass = data.WeaponClass;
            attackScale = data.AttackScale;
            useTriggerEnter = data.UseTriggerEnter;
            useTriggerStay = data.UseTriggerStay;
            tickInterval = data.TickInterval;
            shouldStackDamage = data.ShouldStackDamage;
            enablePierce = data.EnablePierce;
            pierceCount = data.PierceCount;
            addPierceStat = data.AddPierceStat;
            applyKnockback = data.ApplyKnockback;
            knockbackForce = data.KnockbackForce;
            canTriggerLifesteal = data.CanTriggerLifesteal;
            directElementalDebuffData = data.DirectElementalDebuffData;
        }

        /// <summary>
        /// Copies another source's runtime values (the ones its spawner set, not its data's defaults).
        /// For a source cloned from a live one: Instantiate copies serialized fields only, and these aren't.
        /// </summary>
        public void CopyRuntimeFrom(DamageSourceBase other)
        {
            other.EnsureLoaded();
            loaded = true;
            baseDamage = other.baseDamage;
            weaponClass = other.weaponClass;
            attackScale = other.attackScale;
            useTriggerEnter = other.useTriggerEnter;
            useTriggerStay = other.useTriggerStay;
            tickInterval = other.tickInterval;
            shouldStackDamage = other.shouldStackDamage;
            enablePierce = other.enablePierce;
            pierceCount = other.pierceCount;
            addPierceStat = other.addPierceStat;
            pierceSetExplicitly = other.pierceSetExplicitly;
            applyKnockback = other.applyKnockback;
            knockbackForce = other.knockbackForce;
            canTriggerLifesteal = other.canTriggerLifesteal;
            directElementalDebuffData = other.directElementalDebuffData;
            effectID = other.effectID;
            weaponData = other.weaponData;
            stackDamageMultiplier = other.stackDamageMultiplier;
        }

        private void Start()
        {
            EnsureLoaded();
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();

            if (addPierceStat && enablePierce && !pierceSetExplicitly && playerProgression != null && PierceSettings.Instance != null)
            {
                pierceCount += PierceSettings.Instance.GetExtraPierce(playerProgression.GetStatTotal(StatType.Pierce));
            }
        }

        public void Initialize(int baseDamage, int pierceAmount, string effectId, WeaponClass weaponClass, WeaponData weaponData)
        {
            EnsureLoaded();
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
        public void ApplyOnHitEffectsTo(GameObject target)
        {
            EnsureLoaded();
            TryApplyElementalDebuff(target);
        }

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
            EnsureLoaded();
            stackDamageMultiplier = scaling.Get(StackScalingTarget.Damage);

            float pierceScale = scaling.Get(StackScalingTarget.Pierce);
            if (pierceScale != 1f)
            {
                pierceCount = Mathf.Max(1, Mathf.RoundToInt(pierceCount * pierceScale));
            }
        }

        public void SetElementalDebuffData(ElementalDebuffData debuffData) { EnsureLoaded(); directElementalDebuffData = debuffData; }

        public void SetBaseDamage(int value) { EnsureLoaded(); baseDamage = value; }
        public int BaseDamage { get { EnsureLoaded(); return baseDamage; } }

        // Targets this source never damages, e.g. the enemy a split arrow broke off from (it spawns
        // inside that enemy's collider and would otherwise hit it again immediately).
        private HashSet<GameObject> ignoredTargets;

        public void IgnoreTarget(GameObject target)
        {
            if (target == null) return;
            ignoredTargets ??= new HashSet<GameObject>();
            ignoredTargets.Add(target);
        }
        public void SetWeaponClass(WeaponClass wc) { EnsureLoaded(); weaponClass = wc; }
        public void SetAttackScale(float scale) { EnsureLoaded(); attackScale = scale; }
        public float AttackScale { get { EnsureLoaded(); return attackScale; } }
        public void EnableTriggerEnter(bool enabled) { EnsureLoaded(); useTriggerEnter = enabled; }
        public void EnableTriggerStay(bool enabled, float interval = 1f)
        {
            EnsureLoaded();
            useTriggerStay = enabled;
            tickInterval = interval;
        }

        public void ConfigurePierce(bool enabled, int count)
        {
            EnsureLoaded();
            enablePierce = enabled;
            pierceCount = count;
            pierceSetExplicitly = true;
        }

    }

}