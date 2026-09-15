using System.Collections;
using System.Collections.Generic;
using Game.Combat;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/Spawn Object On Random Enemy Overtime")]
    public class SpawnObjectOnRandomEnemyOvertimeEffectBehavior : EffectBehavior
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private int spawnCount = 1;
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [Header("Timing")]
        [Tooltip("How often, in seconds, a random tracked enemy is targeted for a spawn.")]
        [SerializeField] private float spawnInterval = 3f;

        [Header("Detection")]
        [Tooltip("Radius around the player within which enemies are tracked as spawn candidates.")]
        [SerializeField] private float detectionRadius = 8f;
        [Tooltip("How often, in seconds, the nearby-enemy list is refreshed.")]
        [SerializeField] private float trackingInterval = 0.25f;

        // Per-owner runtime state - see EffectBehaviorContext for why this can't live directly
        // on this ScriptableObject. Both loops are hosted as coroutines on the EffectStore (the
        // player's own persistent MonoBehaviour), the same way SpawnObjectWhileDashingEffectBehavior
        // hosts its spawn loop, since this ScriptableObject has no Update of its own.
        private class RuntimeState
        {
            public ContactFilter2D contactFilter;
            public readonly List<Collider2D> overlapResults = new();
            public readonly List<EnemyHealth> nearbyEnemies = new();
            public readonly HashSet<EnemyHealth> seenThisPass = new();
            public Coroutine trackingRoutine;
            public Coroutine spawnRoutine;
        }

        public override void Initialize(EffectBehaviorContext context, Effect parentEffect)
        {
            base.Initialize(context, parentEffect);

            var state = GetState<RuntimeState>();
            state.contactFilter = new ContactFilter2D { useTriggers = true };
            state.contactFilter.SetLayerMask(LayerMask.GetMask("Enemy", "Boss"));

            // Defensive: Initialize shouldn't normally run twice for a live state, but don't stack loops if it does.
            StopRoutines(state);

            state.trackingRoutine = storeOwner.StartCoroutine(TrackNearbyEnemiesRoutine(state));
            state.spawnRoutine = storeOwner.StartCoroutine(SpawnOnIntervalRoutine(state));
        }

        public override void Cleanup()
        {
            var state = GetState<RuntimeState>();
            StopRoutines(state);
            state.nearbyEnemies.Clear();
        }

        private void StopRoutines(RuntimeState state)
        {
            if (state.trackingRoutine != null)
            {
                storeOwner.StopCoroutine(state.trackingRoutine);
                state.trackingRoutine = null;
            }

            if (state.spawnRoutine != null)
            {
                storeOwner.StopCoroutine(state.spawnRoutine);
                state.spawnRoutine = null;
            }
        }

        private IEnumerator TrackNearbyEnemiesRoutine(RuntimeState state)
        {
            while (true)
            {
                RefreshNearbyEnemies(state);
                yield return new WaitForSeconds(trackingInterval);
            }
        }

        /// <summary>
        /// Re-scans the circle around the player: newly-overlapping enemies join the tracked list,
        /// enemies that left the circle (or died) since the last pass are dropped from it.
        /// </summary>
        private void RefreshNearbyEnemies(RuntimeState state)
        {
            if (ownerObject == null) return;

            state.overlapResults.Clear();
            Physics2D.OverlapCircle(ownerObject.transform.position, detectionRadius, state.contactFilter, state.overlapResults);

            state.seenThisPass.Clear();
            foreach (var hit in state.overlapResults)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsDead()) continue;

                state.seenThisPass.Add(enemy);
                if (!state.nearbyEnemies.Contains(enemy))
                {
                    state.nearbyEnemies.Add(enemy);
                }
            }

            state.nearbyEnemies.RemoveAll(enemy => enemy == null || !state.seenThisPass.Contains(enemy));
        }

        private IEnumerator SpawnOnIntervalRoutine(RuntimeState state)
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                SpawnOnRandomEnemies(state);
            }
        }

        private void SpawnOnRandomEnemies(RuntimeState state)
        {
            if (state.nearbyEnemies.Count == 0) return;

            int finalSpawnCount = ResolveSpawnCount(spawnCount);

            for (int i = 0; i < finalSpawnCount; i++)
            {
                EnemyHealth target = state.nearbyEnemies[Random.Range(0, state.nearbyEnemies.Count)];
                if (target == null) continue;

                Vector2 spawnPos = (Vector2)target.transform.position + spawnOffset;
                SpawnEffectObject(prefabToSpawn, spawnPos, Quaternion.identity);
            }
        }
    }
}
