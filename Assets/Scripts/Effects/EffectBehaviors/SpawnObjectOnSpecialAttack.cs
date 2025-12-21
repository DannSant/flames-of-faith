using Game.Combat;
using Game.Combat.Projectiles;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/Spawn On Special Attack")]
    public class SpawnObjectOnSpecialAttack : EffectBehavior
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject prefabToSpawn;

        [SerializeField] private int spawnCount = 1;

        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [SerializeField] private bool randomRotation = true;

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
            for (int i = 0; i < spawnCount; i++)
            {
                if (Random.value <= chanceToSpawn)
                {
                    Vector2 spawnPosition = (Vector2)ownerObject.transform.position + spawnOffset;
                    Quaternion rotation = randomRotation ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;
                    var spawned = Instantiate(prefabToSpawn, spawnPosition, rotation);

                    var effectDamage = spawned.GetComponent<DamageSourceBase>();
                    effectDamage.SetEffectID(parentEffect.EffectID);

                    var effectCount = storeOwner.GetEffectMultiplierConfig(parentEffect.EffectID).count;
                    var effectStackModifier = spawned.GetComponent<IEffectStackModifier>();
                    effectStackModifier?.ModifyEffect(effectCount, stackBehavior);
                }
            }
        }
    }
}