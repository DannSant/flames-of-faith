using Game.Enemies;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class ChaserBehaviorState
    {
        public bool isChasing = true;
    }
    [CreateAssetMenu(menuName = "Behaviors/Chaser")]
    public class AIBehaviorChaser : AIUpdateBehavior
    {
        [SerializeField] private float minRange = 0f;
        [SerializeField] private float buffer = 0.2f;
        [SerializeField] private bool useChasePause = false;
       

        public override void Tick(BehaviorContext context)
        {
            if (context==null || context.playerTransform == null || context.enemyTransform == null) return;

            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
          
            if (rb == null) return;

            float speed = context.enemyData.speedBase;
            float distance = Vector2.Distance(rb.position, context.playerTransform.position);

            var state = context.GetState<ChaserBehaviorState>(this);

            // Handle pause/resume logic
            if (useChasePause)
            {
                if (state.isChasing && distance < minRange)
                {
                    state.isChasing = false; // Stop when too close
                }
                else if (!state.isChasing && distance > minRange + buffer)
                {
                    state.isChasing = true; // Resume when far enough
                }
            }
            else
            {
                state.isChasing = true; // Always chase
            }

            if (!state.isChasing) return;

          

            Vector2 direction = (context.playerTransform.position - context.enemyTransform.position).normalized;
            Vector2 targetPosition = rb.position + direction * speed * Time.deltaTime;

            rb.MovePosition(targetPosition);
            /*if (context.playerTransform == null || context.enemyTransform == null) return;

            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            float speed = context.enemyData.speedBase;
            Vector2 direction = (context.playerTransform.position - context.enemyTransform.position).normalized;
            Vector2 targetPosition = rb.position + direction * speed * Time.deltaTime;

            if (Vector2.Distance(rb.position, context.playerTransform.position) < minRange)
            {
                // If within min range, stop moving
                targetPosition = rb.position;
            }

            rb.MovePosition(targetPosition);*/
        }
    }

}