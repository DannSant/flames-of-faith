using Game.Misc;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/ChaserMovement")]
    public class AIBehaviorChaserMovement : AIFixedUpdateBehavior
    {
        public override void FixedTick(BehaviorContext context)
        {
            if (!context.isMoving) return;

            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            var knockback = context.enemyTransform.GetComponent<Knockback>();
            if (knockback != null && knockback.IsKnockbacked) return;
            //Debug.Log($"MoveDir: {context.moveDirection}");
            float speed = context.enemyData.speedBase * context.speedMultiplier;          
            Vector2 target = rb.position + context.moveDirection * speed * Time.fixedDeltaTime;
            
            rb.MovePosition(target);
        }
    }

}