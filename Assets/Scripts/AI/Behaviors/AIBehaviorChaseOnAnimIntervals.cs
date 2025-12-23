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

        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
        }

        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            float speed = context.enemyData.speedBase * context.speedMultiplier;
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();           
            Vector2 direction = (context.playerTransform.position - context.enemyTransform.position).normalized;
            rb.AddForce(direction * speed,ForceMode2D.Force);
        }

        public override void Tick(BehaviorContext context)
        {}
    }

}