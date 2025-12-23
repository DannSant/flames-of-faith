using Game.Misc;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class RoamingState
    {
        public float directionChangeTimer;
        public Vector2 moveDirection = Vector2.zero;
    }

    [CreateAssetMenu(menuName = "Behaviors/On Update/Roamer")]
    public class RoamerBehavior : AIUpdateBehavior
    {
        [SerializeField] private float directionChangeInterval = 2f;
        public override void Tick(BehaviorContext context)
        {
            if (context.enemyTransform == null || context.enemyGameObject == null) return;

            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            var knockback = context.enemyTransform.GetComponent<Knockback>();
            if (rb == null || (knockback != null && knockback.IsKnockbacked)) return;

            // Get or create the state associated with *this behavior*
            var state = context.GetState<RoamingState>(this);

            state.directionChangeTimer -= Time.deltaTime;
            if (state.directionChangeTimer <= 0f)
            {
                PickNewDirection(state);
                state.directionChangeTimer = directionChangeInterval;
            }

            float speed = context.enemyData.speedBase;
            Vector2 nextPosition = rb.position + state.moveDirection * speed * Time.deltaTime;
            rb.MovePosition(nextPosition);
        }

        private void PickNewDirection(RoamingState state)
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            state.moveDirection = new Vector2(x, y).normalized;
        }
    }
}
