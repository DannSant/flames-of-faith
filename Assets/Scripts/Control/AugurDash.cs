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
        [SerializeField] private float trailSpawnInterval = 0.05f;

        private readonly List<Collider2D> ignoredEnemyColliders = new();
        private float trailSpawnTimer;

        protected override void Update()
        {
            base.Update();

            if (dashUpdateTimer.GetIsEventActive())
            {
                trailSpawnTimer += Time.deltaTime;
                if (trailSpawnTimer >= trailSpawnInterval)
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
                Instantiate(flameTrailSegmentPrefab, transform.position, Quaternion.identity);
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
