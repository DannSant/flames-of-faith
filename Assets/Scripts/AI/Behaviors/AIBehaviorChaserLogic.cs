using Game.Enemies;
using Game.Misc;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Update/ChaserLogic")]
    public class AIBehaviorChaserLogic : AIUpdateBehavior
    {
        [Tooltip("Distance to the player under which the enemy stops moving.")]
        [SerializeField] private float minRange = 0f;
        [Tooltip("It starts moving again once the player is farther than Min Range + this.")]
        [SerializeField] private float buffer = 0.2f;
       

        public override void Tick(BehaviorContext context)
        {
            if (context.playerTransform == null || context.enemyTransform == null)
                return;

            Vector2 dir = (context.playerTransform.position - context.enemyTransform.position);
           
            float distance = Vector2.Distance(context.enemyTransform.position, context.playerTransform.position);
            if (distance < minRange)
            {
                context.isMoving = false; // Stop when too close
            }
            else if ( distance > minRange + buffer)
            {               
                context.moveDirection = dir.normalized;
                context.isMoving = true;
            }

        }
    }

}