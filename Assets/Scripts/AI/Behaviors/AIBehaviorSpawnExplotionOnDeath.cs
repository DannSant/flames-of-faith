using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Death/Spawn Explosion")]
    public class AIBehaviorSpawnExplotionOnDeath : AIDeathBehavior
    {
        [SerializeField] private EnemyExplotion explosionPrefab;

        public override void OnDeath(BehaviorContext context)
        {
            // A silent death is a forced kill (e.g. clearing the board at wave end) rather than
            // a real combat death — spawning the explosion here would leave live projectiles
            // that can still hit the player after the wave has already ended.
            if (context.diedSilently)
            {
                return;
            }

            var explotionObj = Instantiate(explosionPrefab, context.enemyGameObject.transform.position, Quaternion.identity);

            explotionObj.Initialize(GetRangedDamageAmount(context));
        }
    }
}
