using Game.AI.Steering;
using Game.Misc;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Fixed Update/ChaserMovement")]
    public class AIBehaviorChaserMovement : AIFixedUpdateBehavior
    {
        [Tooltip("Steer around obstacles (trees) instead of pressing into them. Tuned in ObstacleAvoidanceSettings.")]
        [SerializeField] private bool avoidObstacles = true;

        public override void FixedTick(BehaviorContext context)
        {
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            // While knocked back, physics owns the body - don't fight the impulse.
            var knockback = context.enemyTransform.GetComponent<Knockback>();
            if (knockback != null && knockback.IsKnockbacked) return;

            // The body is Dynamic with no linear damping, so any velocity it picks up (a
            // knockback impulse, a collision shove) would persist forever and stack with the
            // next one. Writing the velocity every FixedUpdate - zero included - means the AI
            // fully reclaims the body the moment knockback ends, so nothing can accumulate.
            if (!context.isMoving)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float speed = context.enemyData.moveSpeed * context.speedMultiplier;
            // context.moveDirection stays the logic's wanted direction (it also drives facing);
            // only the applied velocity goes around obstacles.
            Vector2 direction = avoidObstacles
                ? ObstacleSteering.SteerAroundObstacles(context, rb, context.moveDirection, speed, Time.fixedDeltaTime)
                : context.moveDirection;
            rb.linearVelocity = direction * speed;
        }
    }

}