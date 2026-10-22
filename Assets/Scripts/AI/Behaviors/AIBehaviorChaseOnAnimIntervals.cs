using Game.AI.Steering;
using Game.Misc;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class ChaserOnIntervalsBehaviorState
    {
        public float chaseTimer = 0f;
    }
    [CreateAssetMenu(menuName = "Behaviors/On Update/Interval Chaser")]
    public class AIBehaviorChaseOnAnimIntervals : AIUpdateBehavior, IAnimationEventReceiver
    {
        [Tooltip("Aim each hop around obstacles (trees). Tuned in ObstacleAvoidanceSettings.")]
        [SerializeField] private bool avoidObstacles = true;


        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
            if (context == null || context.playerTransform == null || context.enemyTransform == null)
            { 
                return; 
            }
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
        }

        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            if (context == null || context.playerTransform == null || context.enemyTransform == null)
            { 
                return; 
            }
            float speed = context.enemyData.hopForce * context.speedMultiplier;
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();           
            Vector2 direction = (context.playerTransform.position - context.enemyTransform.position).normalized;
            if (avoidObstacles)
            {
                // One push per hop, so no stuck check (expected speed 0): just aim the hop around obstacles.
                direction = ObstacleSteering.SteerAroundObstacles(context, rb, direction, 0f, 0f);
            }
            rb.AddForce(direction * speed,ForceMode2D.Force);
        }

        public override void Tick(BehaviorContext context)
        {}
    }

}