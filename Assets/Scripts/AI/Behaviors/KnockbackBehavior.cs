using Game.Combat;
using Game.Misc;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Collision/Knockback On Collision")]
    public class KnockbackBehavior : AICollisionBehavior
    {
        [Tooltip("Strength of the push the player gets when this enemy touches them.")]
        [SerializeField] private float force = 5f;

        public override void HandleCollision(Collision2D collision, BehaviorContext context)
        {
           
            if (collision == null || context.enemyTransform == null) return;
            
            Knockback knockback = collision.gameObject.GetComponent<Knockback>();
            if (knockback == null) return;          
            knockback.ApplyKnockback(context.enemyTransform, force);
        }
    }

}