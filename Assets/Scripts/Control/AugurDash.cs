using Game.Combat;
using Game.Effects.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Control
{
    public class AugurDash : DashBase
    {
        [Header("Augur Dash - Pass Through")]
        [Tooltip("Radius scanned for enemies at dash start; collision with anything found is ignored for the dash's duration.")]
        [SerializeField] private float passThroughDetectionRadius = 3f;
        [SerializeField] private LayerMask enemyLayerMask;

        [Header("Augur Dash - Flame Trail")]
        [Tooltip("Prefab with a trigger Collider2D + DamageSourceBase (directElementalDebuffData = Fire) + DestroyAfterTime.")]
        [SerializeField] private GameObject flameTrailSegmentPrefab;
        [Tooltip("Seconds between flames while dashing. Glyph of Flame Dash overrides this.")]
        [SerializeField] private float trailSpawnInterval = 0.05f;

        private readonly List<Collider2D> ignoredEnemyColliders = new();
        private float trailSpawnTimer;

        // The dash's own values, used when the player has no Glyph of Flame Dash.
        private float defaultDashDuration;
        private int defaultMaxCharges;
        private float currentTrailSpawnInterval;
        // The flames' damage from the Glyph of Flame Dash; none without it (every Augur kit has it).
        private DamageProfile flameDamage;

        protected override void Awake()
        {
            base.Awake();
            defaultDashDuration = dashDuration;
            defaultMaxCharges = maxCharges;
            currentTrailSpawnInterval = trailSpawnInterval;
        }

        protected override void ApplyAbilityItems()
        {
            var flameDash = GetAbilityValues<FlameDashAbility, FlameDashAbility.Values>();
            SetDashDuration(flameDash != null ? flameDash.dashDuration : defaultDashDuration);
            SetMaxCharges(flameDash != null ? flameDash.maxCharges : defaultMaxCharges);
            currentTrailSpawnInterval = flameDash != null ? flameDash.flameSpawnInterval : trailSpawnInterval;
            flameDamage = flameDash != null ? new DamageProfile(flameDash.flameDamage, dashDamageStat, flameDash.flameStatScale) : default;
        }

        protected override void Update()
        {
            base.Update();

            if (dashUpdateTimer.GetIsEventActive())
            {
                trailSpawnTimer += Time.deltaTime;
                if (trailSpawnTimer >= currentTrailSpawnInterval)
                {
                    trailSpawnTimer = 0f;
                    SpawnFlameTrailSegment();
                }
            }
        }

        protected override void StartDashing()
        {
            base.StartDashing();
            trailSpawnTimer = 0f;
            SpawnFlameTrailSegment();
            EnableEnemyPassThrough();
        }

        protected override void EndDashing()
        {
            base.EndDashing();
            DisableEnemyPassThrough();
        }

        private void SpawnFlameTrailSegment()
        {
            if (flameTrailSegmentPrefab != null)
            {
                var flame = Instantiate(flameTrailSegmentPrefab, transform.position, Quaternion.identity);
                flameDamage.ApplyTo(flame.GetComponentInChildren<DamageSourceBase>());
            }
        }

        private void EnableEnemyPassThrough()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, passThroughDetectionRadius, enemyLayerMask);
            foreach (var hit in hits)
            {
                Physics2D.IgnoreCollision(characterCollider, hit, true);
                ignoredEnemyColliders.Add(hit);
            }
        }

        private void DisableEnemyPassThrough()
        {
            foreach (var col in ignoredEnemyColliders)
            {
                if (col != null)
                {
                    Physics2D.IgnoreCollision(characterCollider, col, false);
                }
            }
            ignoredEnemyColliders.Clear();
        }
    }
}
