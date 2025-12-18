using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/Interval Roamer")]
    public class AIBehaviorRoamOnAnimIntervals : AIUpdateBehavior, IAnimationEventReceiver
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
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            Vector2 direction = new Vector2(x, y).normalized;
            rb.AddForce(direction * speed, ForceMode2D.Force);
        }

        public override void Tick(BehaviorContext context)
        { }
    }
}
