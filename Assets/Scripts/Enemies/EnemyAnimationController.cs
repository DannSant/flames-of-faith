using Game.AI.Behaviors;
using Game.Combat;
using Game.Scene;
using UnityEngine;

namespace Game.Enemies
{
    public class EnemyAnimationController : MonoBehaviour
    {
        private Animator animator;
        private static readonly int AttackSpeedParameter = Animator.StringToHash("AttackSpeed");
        private bool hasAttackSpeedParameter;
        private Rigidbody2D rb;
        private BehaviorContext context;
        private Transform playerTransform;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            foreach (var parameter in animator.parameters)
            {
                if (parameter.nameHash == AttackSpeedParameter) hasAttackSpeedParameter = true;
            }           
           
        }

        private void Start()
        {
            context = GetComponent<BehaviorController>().GetBehaviorContext();
            playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>().transform;
        }

        public void PlayShoot()
        {
            CommitFacingToPlayer();
            animator.SetTrigger("Shoot");
        }

        /// <param name="speed">
        /// Speed of the attack animation (its state's AttackSpeed parameter, 1 = normal). A melee attacker slows
        /// it so the hit lands when its warning is full. Ignored by controllers without the parameter.
        /// </param>
        public void PlayAttack(float speed = 1f)
        {
            CommitFacingToPlayer();
            if (hasAttackSpeedParameter) animator.SetFloat(AttackSpeedParameter, speed);
            animator.SetTrigger("Attack");
        }

        /// <summary>
        /// Locks in the 8-way facing toward the player for the action being started. Recording it
        /// on the context is what lets the action hold that facing until it re-aims, and lets the
        /// animation event that spawns projectiles fire along the exact direction being shown.
        /// </summary>
        private void CommitFacingToPlayer()
        {
            Vector2 direction = DirectionToPlayer();

            if (context != null)
            {
                context.facingDirection = direction;
            }

            ApplyDirection(direction);
        }

        private void Update()
        {
            UpdateAnimatorParameters();
        }

        private void UpdateAnimatorParameters()
        {
            if (context == null) return;

            // Priority chain with no gaps: an in-progress shot owns the facing (and holds it
            // until the next shot re-aims), movement owns it while moving, and otherwise the
            // enemy idles looking at the player. Previously the "shooting and not moving" case
            // fell through writing nothing, so whichever writer ran last won the frame.
            if (context.isShooting)
            {
                ApplyDirection(IsometricDirectionHelper.IsValid(context.facingDirection)
                    ? context.facingDirection
                    : DirectionToPlayer());
            }
            else if (context.isMoving)
            {
                ApplyDirection(IsometricDirectionHelper.SnapTo8(context.moveDirection));
            }
            else
            {
                ApplyDirection(DirectionToPlayer());
            }

            animator.SetBool("IsMoving", context.isMoving);
        }

        private Vector2 DirectionToPlayer()
        {
            return IsometricDirectionHelper.SnapTo8(playerTransform.position - transform.position);
        }

        private void ApplyDirection(Vector2 direction)
        {
            // A degenerate direction means "nothing to say" - keep the previous facing rather
            // than snapping the sprite to an arbitrary one.
            if (!IsometricDirectionHelper.IsValid(direction)) return;

            animator.SetFloat("DirectionX", direction.x);
            animator.SetFloat("DirectionY", direction.y);
        }

    }

}