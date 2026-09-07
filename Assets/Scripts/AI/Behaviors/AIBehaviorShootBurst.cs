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

            if (state.isShooting)
                return; // burst already running

            // Host the routine on the BehaviorController rather than whatever MonoBehaviour
            // happens to sit first on the GameObject - that lookup was order-dependent, and if
            // it resolved to a component that got disabled the routine would die mid-burst and
            // leave isShooting stuck true, freezing the facing for the rest of the enemy's life.
            var host = context.enemyTransform.GetComponent<BehaviorController>();
            if (host == null) return;

            state.isShooting = true;
            context.isShooting = true;
            host.StartCoroutine(ShootRoutine(context, state));
        }

        private IEnumerator ShootRoutine(BehaviorContext context, MoveShootCycleState state)
        {
            try
            {
                for (int i = 0; i < shootTimes; i++)
                {
                    ShootAtPlayer(context);
                    yield return new WaitForSeconds(delayBetweenShots);
                }
            }
            finally
            {
                // Runs even if the routine is stopped or the enemy is destroyed mid-burst, so
                // the shooting flags can never be left set.
                state.ResetCycle();
                context.isShooting = false;
            }
        }

        private void ShootAtPlayer(BehaviorContext context)
        {
            var animator = context.enemyAnimController;

            if (animator != null)
            {
                // Re-aims at the player's current position and commits that 8-way facing to the
                // context, which is what makes each shot in the burst turn to follow the player
                // while still holding its direction for the duration of that shot.
                animator.PlayShoot();
            }
        }

        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            if (projectilePrefab == null) return;

            // Fire along the facing the sprite is actually showing (committed by PlayShoot),
            // not a freshly computed vector - otherwise the shots and the animation can point
            // in different directions when the player moves between the two.
            Vector2 baseDirection = context.facingDirection;
            if (!IsometricDirectionHelper.IsValid(baseDirection))
            {
                baseDirection = IsometricDirectionHelper.SnapTo8(
                    context.playerTransform.position - context.enemyTransform.position);
            }

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
