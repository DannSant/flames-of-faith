using Game.AI.Behaviors;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/RoamerBounce")]
    public class RoamerBounceBehavior : AICollisionBehavior
    {
        public override void HandleCollision(Collision2D collision, BehaviorContext context)
        {
            var state = context.GetState<RoamingState>(this);
            if (state == null) return;

            // Reverse direction or pick a new one
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            state.moveDirection = new Vector2(x, y).normalized;
        }
    }

}