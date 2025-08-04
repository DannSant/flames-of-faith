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

            // Reverse direction
            float x = state.moveDirection.x * -1f + Random.Range(-0.2f, 0.2f);
            float y = state.moveDirection.y * -1f + Random.Range(-0.2f, 0.2f);
            state.moveDirection = new Vector2(x, y).normalized;
           
        }
    }

}