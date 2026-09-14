using Game.Control;
using Game.Scene;
using System.Collections;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/Spawn Object While Dashing")]
    public class SpawnObjectWhileDashingEffectBehavior : EffectBehavior
    {
        [Header("Spawn Settings")]
        [Tooltip("Prefab spawned at the player's position, repeatedly, for as long as the dash lasts.")]
        [SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private float spawnInterval = 0.1f;

        // Per-owner runtime state - see EffectBehaviorContext for why this can't live directly
        // on this ScriptableObject. The spawn loop is hosted as a coroutine on the EffectStore
        // (the player's own persistent MonoBehaviour), the same way AI behaviors host theirs on
        // BehaviorController, since this ScriptableObject has no Update of its own.
        private class RuntimeState
        {
            public DashBase dashBase;
            public Coroutine spawnRoutine;
        }

        public override void Initialize(EffectBehaviorContext context, Effect parentEffect)
        {
            base.Initialize(context, parentEffect);

            var state = GetState<RuntimeState>();
            state.dashBase = PlayerManager.Instance.GetPlayerComponent<DashBase>();
            if (state.dashBase == null)
                return;

            state.dashBase.onDashStarted += HandleDashStarted;
            state.dashBase.onDashEnded += HandleDashEnded;
        }

        public override void Cleanup()
        {
            var state = GetState<RuntimeState>();

            if (state.dashBase != null)
            {
                state.dashBase.onDashStarted -= HandleDashStarted;
                state.dashBase.onDashEnded -= HandleDashEnded;
            }

            StopSpawning(state);
        }

        private void HandleDashStarted()
        {
            var state = GetState<RuntimeState>();

            // Defensive: a dash can't normally restart while active, but don't stack loops if it does.
            StopSpawning(state);

            SpawnObject();
            state.spawnRoutine = storeOwner.StartCoroutine(SpawnWhileDashingRoutine());
        }

        private void HandleDashEnded()
        {
            StopSpawning(GetState<RuntimeState>());
        }

        private void StopSpawning(RuntimeState state)
        {
            if (state.spawnRoutine == null)
                return;

            storeOwner.StopCoroutine(state.spawnRoutine);
            state.spawnRoutine = null;
        }

        private IEnumerator SpawnWhileDashingRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                SpawnObject();
            }
        }

        private void SpawnObject()
        {
            // Checked here rather than leaning on SpawnEffectObject's own guard: this runs on an
            // interval for the whole dash, so an unset prefab would warn several times per dash.
            if (prefabToSpawn == null || ownerObject == null)
                return;

            int count = ResolveSpawnCount(1);
            for (int i = 0; i < count; i++)
            {
                SpawnEffectObject(prefabToSpawn, ownerObject.transform.position, Quaternion.identity);
            }
        }
    }
}
