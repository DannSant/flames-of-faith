using Game.Combat;
using Game.Enemies;
using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class ShooterBehaviorState
    {
        public float shootTimer = 0f;
        // Holding still for the shot in progress (holdStillWhileShooting), until the attack's end event or holdUntil
        public bool isHolding = false;
        public float holdUntil = 0f;
    }

    [CreateAssetMenu(menuName = "Behaviors/On Update/Shooter")]
    public class AIBehaviorShooter : AIUpdateBehavior, IAnimationEventReceiver
    {
        [Tooltip("Not read by this behavior at the moment (only Max Range is).")]
        [SerializeField] private float minRange = 5f;
        [Tooltip("The enemy shoots only while the player is closer than this.")]
        [SerializeField] private float maxRange = 7f;
        [Tooltip("Seconds between shots.")]
        [SerializeField] private float shootCooldown = 2f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private string firePointName = "FirePoint";

        [Tooltip("Stand still for the whole shot: from the attack's start to its OnAnimationEventEnd. For enemies that walk " +
            "(the Corruptor); a floating one (FlameShooter) can keep moving. Sets context.isShooting, which the movement " +
            "behaviors (ChaserMovement, MoveToSnapshot) hold position for.")]
        [SerializeField] private bool holdStillWhileShooting = false;
        [Tooltip("Safety net for Hold Still While Shooting: the enemy moves again after this many seconds even if the " +
            "attack's end event never arrives. Keep it above the attack clip's length.")]
        [Min(0.1f)]
        [SerializeField] private float maxHoldTime = 1.5f;

        public override void Tick(BehaviorContext context)
        {
            if (context.enemyTransform == null || context.playerTransform == null) return;

            var state = context.GetState<ShooterBehaviorState>(this);
            if (state.isHolding && Time.time >= state.holdUntil)
            {
                ReleaseHold(context, state);
            }

            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();

            var firePoint = context.enemyTransform.Find(firePointName);

            if (rb == null  || firePoint == null) return;

            float distance = Vector2.Distance(rb.position, context.playerTransform.position);

            if (distance <= maxRange)
            {
                if (state.shootTimer <= 0f)
                {
                    var animator = context.enemyAnimController;
                    Shoot(animator, state);
                    if (holdStillWhileShooting)
                    {
                        state.isHolding = true;
                        state.holdUntil = Time.time + maxHoldTime;
                        context.isShooting = true;
                    }
                }
                else
                {
                    state.shootTimer -= Time.deltaTime;
                }
            }
        }
        private void Shoot(EnemyAnimationController animator, ShooterBehaviorState state)
        {
            state.shootTimer = shootCooldown;
           
            if (animator != null) { 
                animator.PlayShoot();
            }
                
        }
        // Call this from animation event
        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            var firePoint = context.enemyTransform.Find(firePointName);
            if (projectilePrefab == null || firePoint == null || context.playerTransform == null) return;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            MarkIfCorrupted(context, projectile);
            Vector2 direction = (context.playerTransform.position - firePoint.position).normalized;

            if (projectile.TryGetComponent(out ProjectileMovement movement))
                movement.SetDirection(direction);

            if (projectile.TryGetComponent(out EnemyDamage damage))
            {              
                float damageAmount = GetRangedDamageAmount(context);                
                damage.SetDamageAmount(damageAmount);
            }

            if (projectile.TryGetComponent(out EnemyTriggerDamage damageTrigger))
            {               
                float damageAmount = GetRangedDamageAmount(context);                
                damageTrigger.SetDamageAmount(damageAmount);
            }
        }

        // Call this from animation event: the attack is over, so a held enemy can move again
        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
            var state = context.GetState<ShooterBehaviorState>(this);
            if (state.isHolding)
            {
                ReleaseHold(context, state);
            }
        }

        // A stun or a gameplay freeze cuts the attack short (the animator freezes, so its end event may never
        // come): release the hold rather than leave isShooting set, which would pin the enemy in place
        public override void OnStunStateChanged(BehaviorContext context, bool isStunned)
        {
            if (!isStunned) return;

            var state = context.GetState<ShooterBehaviorState>(this);
            if (state.isHolding)
            {
                ReleaseHold(context, state);
            }
        }

        private static void ReleaseHold(BehaviorContext context, ShooterBehaviorState state)
        {
            state.isHolding = false;
            context.isShooting = false;
        }

        
    }

}