using Game.Misc;
using UnityEngine;

namespace Game.Effects
{
    [CreateAssetMenu(fileName = "SpawnObjectOnPlayer", menuName = "Effects/SpawnObjectOnPlayer")]
    public class SpawnObjectOnPlayer : Effect
    {
        /*[SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private float offsetAnglePerObject = 180f;
        [SerializeField] private int numberOfObjects = 2;

        public override void Apply(GameObject config, EffectStore effectStore)
        {
            base.Apply(config, effectStore);
            if (prefabToSpawn == null || config == null)
            {
                Debug.LogWarning("SpawnObjectOnPlayer: Missing prefab or config reference.");
                return;
            }

            // Create a spawn position slightly above the player (Y offset)
            Vector3 spawnPosition = config.transform.position;

            float initialAngle = 0f;
            for (int i= 0; i < numberOfObjects; i++){
                // Instantiate the object as a child of the player
                GameObject spawned = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, config.transform);
                //spawned.GetComponent<OrbitingObject>()?.SetOrbitTarget(config.transform, initialAngle);
                initialAngle += offsetAnglePerObject;
               
                spawned.GetComponent<IEffectMultiplier>()?.SetEffectID(EffectID);
                
            }  
        }*/
      
    }

}