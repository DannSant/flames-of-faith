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
        [SerializeField] private float angleStep = 180f;

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

            float currentAngle = 0f;
            var state = GetState<RuntimeState>();

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefabToSpawn, ownerObject.transform.position, Quaternion.identity, ownerObject.transform);

                // Give it initial rotation offset
                IOrbitInitializer orbitInit = obj.GetComponent<IOrbitInitializer>();
                orbitInit?.InitializeOrbit(ownerObject.transform, currentAngle);

                // Optional: effect multiplier
                obj.GetComponent<IEffectMultiplier>()?.SetEffectID(parentEffect.EffectID);

                state.spawnedObjects.Add(obj);
                currentAngle += angleStep;
            }
        }

        private void UpdateStackScaling()
        {

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

            var state = GetState<RuntimeState>();

            // Destroy spawned objects
            foreach (var obj in state.spawnedObjects)
            {
                if (obj != null)
                    GameObject.Destroy(obj);
            }

            state.spawnedObjects.Clear();
        }
    }

}