using Game.AI.Behaviors;
using Game.Combat;
using Game.Scene;
using UnityEngine;

namespace Game.Enemies
{
    public class EnemyAnimationController : MonoBehaviour
    {
        private Animator animator;
        private Rigidbody2D rb;
        private BehaviorContext context;
        private Transform playerTransform;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();           
           
        }

        private void Start()
        {
            context = GetComponent<BehaviorController>().GetBehaviorContext();
            playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>().transform;
        }

        public void PlayShoot()
        {
            var playerPosition = playerTransform.position;
            var enemyPosition = transform.position;
            var dir = (playerPosition - enemyPosition).normalized;
            
            animator.SetFloat("DirectionX", dir.x);
            animator.SetFloat("DirectionY", dir.y);
            animator.SetTrigger("Shoot");

        }
        public void PlayAttack() => animator.SetTrigger("Attack");

        private void Update()
        {
            UpdateAnimatorParameters();
        }

        private void UpdateAnimatorParameters()
        {
            var dir = context.moveDirection;
            bool isMoving = context.isMoving;

            if (isMoving)
            {
                animator.SetFloat("DirectionX", dir.x);
                animator.SetFloat("DirectionY", dir.y);
            }else if (!context.isShooting)
            {
                var playerPosition = playerTransform.position;
                var enemyPosition = transform.position;
                var dirTowardsPlayer = (playerPosition - enemyPosition).normalized;
                animator.SetFloat("DirectionX", dirTowardsPlayer.x);
                animator.SetFloat("DirectionY", dirTowardsPlayer.y);
            }

            animator.SetBool("IsMoving", context.isMoving);
        }

    }

}