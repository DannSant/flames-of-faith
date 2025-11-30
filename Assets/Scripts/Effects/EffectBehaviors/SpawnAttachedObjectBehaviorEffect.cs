using Game.Misc;
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

        private readonly List<GameObject> spawnedObjects = new();


        public override void Initialize(GameObject owner, EffectStore store, Effect effect)
        {
            base.Initialize(owner, store, effect);
            SpawnObjects();
        }

        private void SpawnObjects()
        {
            if (prefabToSpawn == null || ownerObject == null)
            {
                Debug.LogWarning("SpawnAttachedObjectsBehavior: Missing prefab or ownerObject");
                return;
            }

            float currentAngle = 0f;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefabToSpawn, ownerObject.transform.position, Quaternion.identity, ownerObject.transform);

                // Give it initial rotation offset
                IOrbitInitializer orbitInit = obj.GetComponent<IOrbitInitializer>();
                orbitInit?.InitializeOrbit(ownerObject.transform, currentAngle);

                // Optional: effect multiplier
                obj.GetComponent<IEffectMultiplier>()?.SetEffectID(parentEffect.EffectID);

                spawnedObjects.Add(obj);
                currentAngle += angleStep;
            }
        }

        public override void Cleanup()
        {
            // Destroy spawned objects
            foreach (var obj in spawnedObjects)
            {
                if (obj != null)
                    GameObject.Destroy(obj);
            }

            spawnedObjects.Clear();
        }
    }

}