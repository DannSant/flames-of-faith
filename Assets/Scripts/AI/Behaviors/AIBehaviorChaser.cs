using Game.Enemies;
using Game.Misc;
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

            var knockback = context.enemyTransform.GetComponent<Knockback>();
            if (knockback != null && knockback.IsKnockbacked) return;

            float speed = context.enemyData.speedBase *context.speedMultiplier;
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
         
        }
    }

}