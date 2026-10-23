using Game.Combat;
using Game.Misc;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Collision/Knockback On Collision")]
    public class KnockbackBehavior : AICollisionBehavior
    {
        [Tooltip("Strength of the push the player gets when this enemy touches them.")]
        [SerializeField] private float force = 5f;
        [Tooltip("Unity force mode. Currently the knockback ignores it and uses Force only.")]
        [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;

        public override void HandleCollision(Collision2D collision, BehaviorContext context)
        {
           
            if (collision == null || context.enemyTransform == null) return;
            
            Knockback knockback = collision.gameObject.GetComponent<Knockback>();
            if (knockback == null) return;          
            knockback.ApplyKnockback(context.enemyTransform, force);

            /*Rigidbody2D targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (targetRb == null) return;
            Debug.Log("Rigidbody2D not null");
            Vector2 direction = (collision.transform.position - context.enemyTransform.position).normalized;
            targetRb.AddForce(direction * force, forceMode);*/


        }
    }

}