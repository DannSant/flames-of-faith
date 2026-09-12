using Game.Misc;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Propagates a hit to everything in a radius, at a fraction of the damage that was actually
    /// dealt. Sits beside <see cref="DamageSourceBase"/> rather than inside it, like
    /// TriggerAnimationOnDamage / DestroyObjectOnDamage / ExplosionDamage already do.
    ///
    /// Use this when the AoE should inherit and scale the damage of the hit that caused it. Use
    /// <see cref="ExplosionDamage"/> instead when the AoE should be a real authored object with its
    /// own collider, VFX and element - that one spawns a prefab carrying its own DamageSourceBase,
    /// so its damage is independent of the hit rather than derived from it.
    /// </summary>
    [RequireComponent(typeof(DamageSourceBase))]
    public class AreaDamageOnHit : MonoBehaviour, IDamageDealtNotifier
    {
        private enum AreaOrigin { Self, Target }

        [Header("Area")]
        [SerializeField] private float radius = 2.5f;
        [Tooltip("Centre the area on this object, or on the enemy that was hit.")]
        [SerializeField] private AreaOrigin origin = AreaOrigin.Self;
        [SerializeField] private LayerMask targetLayerMask;

        [Header("Damage")]
        [Tooltip("Fraction of the damage actually dealt to the primary target. 0.5 = 50%.")]
        [Range(0f, 1f)]
        [SerializeField] private float damageMultiplier = 0.5f;
        [SerializeField] private bool applyDebuffsToSecondaryTargets = false;
        [SerializeField] private bool allowLifestealOnSecondaryTargets = false;

        [Header("Behaviour")]
        [Tooltip("Only ever fire once. Leave on for one-shot sources like a trap, otherwise every " +
            "enemy entering the trigger sweeps the whole area again.")]
        [SerializeField] private bool oneShot = true;

        [Header("VFX")]
        [SerializeField] private GameObject areaVfxPrefab;
        [SerializeField] private float areaVfxLifetime = 1f;

        public event Action<float, GameObject> OnDamageDealtEvent;

        private DamageSourceBase damageSource;
        private ContactFilter2D contactFilter;
        private readonly List<Collider2D> overlapResults = new();
        private readonly HashSet<IDamageable> damagedTargets = new();
        private bool hasTriggered;

        private void Awake()
        {
            damageSource = GetComponent<DamageSourceBase>();

            contactFilter = new ContactFilter2D { useTriggers = true };
            contactFilter.SetLayerMask(targetLayerMask);
        }

        private void OnEnable()
        {
            damageSource.OnDamageDealtEvent += HandleDamageDealt;
        }

        private void OnDisable()
        {
            damageSource.OnDamageDealtEvent -= HandleDamageDealt;
        }

        private void HandleDamageDealt(float damage, GameObject primaryTarget)
        {
            if (oneShot && hasTriggered) return;
            hasTriggered = true;

            Vector3 center = origin == AreaOrigin.Target && primaryTarget != null
                ? primaryTarget.transform.position
                : transform.position;

            SpawnAreaVfx(center);
            DamageArea(center, damage, ResolveDamageable(primaryTarget));
        }

        private void DamageArea(Vector3 center, float sourceDamage, IDamageable primaryTarget)
        {
            float areaDamage = sourceDamage * damageMultiplier;
            if (areaDamage <= 0f) return;

            damagedTargets.Clear();
            overlapResults.Clear();
            Physics2D.OverlapCircle(center, radius, contactFilter, overlapResults);

            foreach (var hit in overlapResults)
            {
                IDamageable target = ResolveDamageable(hit);
                if (target == null || ReferenceEquals(target, primaryTarget)) continue;
                if (target.IsDead() || target.IsImmune()) continue;

                // One enemy can contribute several colliders to the sweep (its body plus an
                // enabled melee hitbox child sitting on the same layer), so dedupe on the resolved
                // owner rather than on the collider.
                if (!damagedTargets.Add(target)) continue;

                target.TakeDamage(new DamageRequest(
                    areaDamage,
                    damageSource.WeaponClass,
                    allowLifestealOnSecondaryTargets && damageSource.CanTriggerLifesteal));

                if (target is not Component targetComponent) continue;

                if (target.ShouldSpawnDamageNumber())
                {
                    DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(
                        targetComponent.transform.position, areaDamage, damageSource.WeaponClass);
                }

                if (applyDebuffsToSecondaryTargets)
                {
                    damageSource.ApplyOnHitEffectsTo(targetComponent.gameObject);
                }

                OnDamageDealtEvent?.Invoke(areaDamage, targetComponent.gameObject);
            }
        }

        private void SpawnAreaVfx(Vector3 center)
        {
            if (areaVfxPrefab == null) return;

            var instance = Instantiate(areaVfxPrefab, center, Quaternion.identity);
            Destroy(instance, areaVfxLifetime);
        }

        private void OnDrawGizmosSelected()
        {
            // Accurate for origin = Self; with origin = Target the sweep is centred on whoever was
            // hit, so this shows the reach rather than the exact circle.
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        /// <summary>
        /// Enemy variants such as EnemyLightDevourer wrap EnemyBase as a child, so the collider that
        /// the sweep returns is not necessarily the object holding the health component.
        /// </summary>
        private static IDamageable ResolveDamageable(Collider2D collider)
        {
            if (collider == null) return null;

            if (collider.attachedRigidbody != null)
            {
                var byBody = collider.attachedRigidbody.GetComponent<IDamageable>();
                if (byBody != null) return byBody;
            }

            return collider.GetComponentInParent<IDamageable>();
        }

        private static IDamageable ResolveDamageable(GameObject target)
        {
            return target != null ? target.GetComponentInParent<IDamageable>() : null;
        }
    }
}
