using Game.Combat;
using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class ShooterBehaviorState
    {
        public float shootTimer = 0f;
        public bool isShooting = false;
    }

    [CreateAssetMenu(menuName = "Behaviors/Shooter")]
    public class AIBehaviorShooter : AIUpdateBehavior, IAnimationEventReceiver
    {
        [SerializeField] private float minRange = 5f;
        [SerializeField] private float maxRange = 7f;
        [SerializeField] private float shootCooldown = 2f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private string firePointName = "FirePoint";

        public override void Tick(BehaviorContext context)
        {
            if (context.enemyTransform == null || context.playerTransform == null) return;

            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            var animator = context.enemyTransform.GetComponent<Animator>();
            var firePoint = context.enemyTransform.Find(firePointName);

            if (rb == null || animator == null || firePoint == null) return;

            float distance = Vector2.Distance(rb.position, context.playerTransform.position);

            var state = context.GetState<ShooterBehaviorState>(this);

            if (state.isShooting) return;

            if (distance <= maxRange)
            {
                if (state.shootTimer <= 0f)
                {
                    Shoot(animator, state);
                }
                else
                {
                    state.shootTimer -= Time.deltaTime;
                }
            }
        }
        private void Shoot(Animator animator, ShooterBehaviorState state)
        {
            state.isShooting = true;
            state.shootTimer = shootCooldown;

            if (animator != null)
                animator.SetTrigger("Shoot");
        }
        // Call this from animation event
        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            var firePoint = context.enemyTransform.Find(firePointName);
            if (projectilePrefab == null || firePoint == null || context.playerTransform == null) return;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 direction = (context.playerTransform.position - firePoint.position).normalized;

            if (projectile.TryGetComponent(out ProjectileMovement movement))
                movement.SetDirection(direction);

            if (projectile.TryGetComponent(out EnemyDamage damage))
            {              
                int damageAmount = GetRangedDamageAmount(context);                
                damage.SetDamageAmount(damageAmount);
            }

            if (projectile.TryGetComponent(out EnemyTriggerDamage damageTrigger))
            {               
                int damageAmount = GetRangedDamageAmount(context);                
                damageTrigger.SetDamageAmount(damageAmount);
            }
        }

        // Call this from animation event
        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
            var state = context.GetState<ShooterBehaviorState>(this);
            state.isShooting = false;
        }

        
    }

}