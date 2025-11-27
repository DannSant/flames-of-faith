using Unity.VisualScripting;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/MoveToSnapshot")]
    public class AIBehaviorMoveToSnapshot : AIUpdateBehavior
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float arrivalThreshold = 0.3f;
        [SerializeField] private BehaviorSharedStateGroup sharedStateGroup;
        public override void Tick(BehaviorContext context)
        {
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            var state = context.GetState<MoveShootCycleState>(sharedStateGroup);

            // If we don't have a target yet → capture it
            if (!state.hasTarget)
            {
                state.targetPoint = context.playerTransform.position;
                state.hasTarget = true;
            }

            // Move toward the snapshot point
            Vector2 dir = (state.targetPoint - (Vector2)rb.position);
            float distance = Vector2.Distance(rb.position, state.targetPoint);
            Debug.DrawLine(rb.position, state.targetPoint, Color.red);
           
            if (distance <= arrivalThreshold)
            {
               
                state.reachedPoint = true;
                context.isMoving = false;
                return;
            }

            Vector2 step = dir.normalized * speed * Time.deltaTime;
            rb.MovePosition(rb.position + step);
            context.isMoving = true;
            context.moveDirection = dir.normalized;
        }
    }
}
