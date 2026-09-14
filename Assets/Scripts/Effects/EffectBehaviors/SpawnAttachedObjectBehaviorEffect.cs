using Game.Misc;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/SpawnAttachedObjects")]
    public class SpawnAttachedObjectBehaviorEffect : EffectBehavior
    {
        [SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private int count = 2;

        // Per-owner runtime state - see EffectBehaviorContext for why this can't live directly
        // on this ScriptableObject.
        private class RuntimeState
        {
            public readonly List<GameObject> spawnedObjects = new();
        }

        public override void Initialize(EffectBehaviorContext context, Effect effect)
        {
            base.Initialize(context, effect);
            PlayerManager.Instance.OnPlayerDisabledOnMap += DisableVisuals;
        }

        public override void OnTrigger(EffectTrigger trigger)
        {
            switch (trigger)
            {
                case EffectTrigger.OnApply:
                    SpawnObjects();
                    break;

                case EffectTrigger.OnStack:
                    UpdateStackScaling();
                    break;
            }
        }

        private void SpawnObjects()
        {
            if (prefabToSpawn == null || ownerObject == null)
            {
                Debug.LogWarning("SpawnAttachedObjectsBehavior: Missing prefab or ownerObject");
                return;
            }

            var state = GetState<RuntimeState>();
            int finalCount = ResolveSpawnCount(count);

            // Spacing is derived from the count rather than authored, so stacking redistributes
            // the orbiters evenly instead of piling new ones on top of the existing ones.
            float angleStep = 360f / finalCount;

            for (int i = 0; i < finalCount; i++)
            {
                GameObject obj = SpawnEffectObject(
                    prefabToSpawn, ownerObject.transform.position, Quaternion.identity, ownerObject.transform);

                if (obj == null) continue;

                obj.GetComponent<IOrbitInitializer>()?.InitializeOrbit(ownerObject.transform, angleStep * i);
                state.spawnedObjects.Add(obj);
            }
        }

        /// <summary>
        /// These orbiters are persistent rather than fire-and-forget, so a new stack can't just
        /// widen a spawn loop - the existing ones have to be torn down and the whole ring rebuilt
        /// at the new count to stay evenly spaced.
        /// </summary>
        private void UpdateStackScaling()
        {
            DespawnObjects();
            SpawnObjects();
        }

        private void DespawnObjects()
        {
            var state = GetState<RuntimeState>();

            foreach (var obj in state.spawnedObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }

            state.spawnedObjects.Clear();
        }

        private void DisableVisuals()
        {
            foreach (var obj in GetState<RuntimeState>().spawnedObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        public override void Cleanup()
        {
            PlayerManager.Instance.OnPlayerDisabledOnMap -= DisableVisuals;
            DespawnObjects();
        }
    }

}
