using Game.AI;
using Game.AI.Behaviors;
using Game.AI.Steering;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Update/MoveToObject")]
    public class AIBehaviorMoveToObject : AIUpdateBehavior
    {
        [Tooltip("Speed of this scripted walk (e.g. DrekoEyeAdd walking to the altar), separate from EnemyData.moveSpeed on purpose.")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private float arrivalThreshold = 0.3f;
        [Tooltip("Steer around obstacles (trees) instead of pressing into them. Tuned in ObstacleAvoidanceSettings.")]
        [SerializeField] private bool avoidObstacles = true;


        public override void Tick(BehaviorContext context)
        {
            if (context == null) return;
            if(context.aiFixedTarget == null) return;
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            // Move toward the snapshot point
            Vector2 dir = ((Vector2)context.aiFixedTarget.transform.position - rb.position);
            float distance = Vector2.Distance(rb.position, context.aiFixedTarget.transform.position);
            Debug.DrawLine(rb.position, context.aiFixedTarget.transform.position, Color.red);

            if (distance <= arrivalThreshold)
            {              
                context.isMoving = false;
                return;
            }

            Vector2 direction = avoidObstacles
                ? ObstacleSteering.SteerAroundObstacles(context, rb, dir, speed, Time.deltaTime)
                : dir.normalized;
            Vector2 step = direction * speed * Time.deltaTime;
            rb.MovePosition(rb.position + step);
            context.isMoving = true;
            context.moveDirection = dir.normalized;
        }
    }

}