using Game.Misc;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Update/MoveToSnapshot")]
    public class AIBehaviorMoveToSnapshot : AIUpdateBehavior
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float arrivalThreshold = 0.3f;
        [SerializeField] private BehaviorSharedStateGroup sharedStateGroup;
        public override void Tick(BehaviorContext context)
        {
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            var knockback = context.enemyTransform.GetComponent<Knockback>();
            if (knockback != null && knockback.IsKnockbacked) return;

            var state = context.GetState<MoveShootCycleState>(sharedStateGroup);

            // Hold position while a burst is in progress. Without this, anything that displaces
            // the enemy mid-burst (knockback, a shove from another enemy) puts it further than
            // arrivalThreshold from the now-stale snapshot point, which would re-engage movement
            // and drag moveDirection toward where the player *used* to be - and the animation
            // controller would then aim the sprite there, often the opposite way from the player.
            if (context.isShooting)
            {
                context.isMoving = false;
                rb.linearVelocity = Vector2.zero;
                return;
            }

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
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Velocity-driven, and written every tick (zero included) for the same reason as
            // AIBehaviorChaserMovement: the body is Dynamic with no linear damping, so any
            // velocity it picks up - a knockback impulse, a collision shove - would otherwise
            // persist forever and stack with the next one.
            context.isMoving = true;
            context.moveDirection = dir.normalized;
            rb.linearVelocity = context.moveDirection * speed;
        }
    }
}
