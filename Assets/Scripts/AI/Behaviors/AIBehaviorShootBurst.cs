using Game.Combat;
using Game.Enemies;
using System.Collections;
using UnityEngine;

namespace Game.AI.Behaviors
{
    [CreateAssetMenu(menuName = "Behaviors/On Update/ShootBurst")]
    public class AIBehaviorShootBurst : AIUpdateBehavior, IAnimationEventReceiver
    {
        [SerializeField] private int shootTimes = 3;
        [SerializeField] private int projectilesPerShot = 3;
        [SerializeField][Range(0f, 180f)] private float spreadAngle = 90f;
        [SerializeField] private float delayBetweenShots = 0.3f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private BehaviorSharedStateGroup sharedStateGroup;

        public override void Tick(BehaviorContext context)
        {
            var state = context.GetState<MoveShootCycleState>(sharedStateGroup);
          
         

            if (!state.reachedPoint)
                return; // not time to shoot

          

            if (!state.isShooting)
            {
                state.isShooting = true;
                context.isShooting = true;
                context.enemyTransform.GetComponent<MonoBehaviour>()
                    .StartCoroutine(ShootRoutine(context, state));
            }
        }

        private IEnumerator ShootRoutine(BehaviorContext context, MoveShootCycleState state)
        {
          
            for (int i = 0; i < shootTimes; i++)
            {
                ShootAtPlayer(context);
                yield return new WaitForSeconds(delayBetweenShots);
            }

            // Reset cycle
            state.ResetCycle();
            context.isShooting = false;
        }

        private void ShootAtPlayer(BehaviorContext context)
        {
            var animator = context.enemyAnimController;
          
            if (animator != null)
            {
                animator.PlayShoot();
            }
           
        }

        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            if (projectilePrefab == null) return;

            Vector2 baseDirection =
                (context.playerTransform.position - context.enemyTransform.position).normalized;

            // Middle of the arc will be the base direction
            float halfSpread = spreadAngle / 2f;

            // If only 1 projectile → just shoot straight
            if (projectilesPerShot == 1)
            {
                SpawnProjectile(context, baseDirection);
                return;
            }

            // Compute angle step
            float step = spreadAngle / (projectilesPerShot - 1);

            for (int i = 0; i < projectilesPerShot; i++)
            {
                // Angle offset from center
                float offset = -halfSpread + (step * i);

                // Rotate direction
                Vector2 rotatedDir = Quaternion.Euler(0, 0, offset) * baseDirection;

                SpawnProjectile(context, rotatedDir);
            }
        }

        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
           
        }

        private void SpawnProjectile(BehaviorContext context, Vector2 direction)
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                context.enemyTransform.position,
                Quaternion.identity
            );

            if (projectile.TryGetComponent(out ProjectileMovement movement))
            {
                movement.SetDirection(direction.normalized);
            }
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
    }
}
