using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Death/Spawn Explosion")]
    public class AIBehaviorSpawnExplotionOnDeath : AIDeathBehavior
    {
        [SerializeField] private EnemyExplotion explosionPrefab;

        public override void OnDeath(BehaviorContext context)
        {
            var explotionObj = Instantiate(explosionPrefab, context.enemyGameObject.transform.position, Quaternion.identity);

            explotionObj.Initialize(GetDamageAmount(context));
        }
    }
}
