using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/Spawn On Special Attack")]
    public class SpawnObjectOnSpecialAttack : SpawningEffectBehavior
    {
        [Header("Spawn Settings")]

        [Tooltip("How many spawn per special attack.")]
        [SerializeField] private int spawnCount = 1;

        [Tooltip("Offset from the player where they appear (X right, Y up).")]
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [Tooltip("Each one starts facing a random direction.")]
        [SerializeField] private bool randomRotation = true;

        [Tooltip("Chance (0 to 1) per special attack, rolled for each copy of the item.")]
        [Range(0f, 1f)]
        [SerializeField] private float chanceToSpawn = 1f;

        public override void OnTrigger(EffectTrigger trigger)
        {
            if (trigger == EffectTrigger.OnSpecialAttack)
            {
                TrySpawnObjects();
            }
        }

        private void TrySpawnObjects()
        {
            int finalCount = ResolveSpawnCount(spawnCount);

            for (int i = 0; i < finalCount; i++)
            {
                if (Random.value <= chanceToSpawn)
                {
                    Vector2 spawnPosition = (Vector2)ownerObject.transform.position + spawnOffset;
                    Quaternion rotation = randomRotation ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;

                    SpawnEffectObject(prefabToSpawn, spawnPosition, rotation);
                }
            }
        }
    }
}
