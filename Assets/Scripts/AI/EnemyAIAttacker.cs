using Game.AI;
using Game.Control;
using Game.Misc;
using Game.Scene;
using UnityEngine;

namespace Game.AI
{
    public class EnemyAIAttacker : EnemyAIBase
    {
        //Settings
        [SerializeField] 
        private  float speed = 1f;
        [SerializeField]
        private float minDistanceToPlayer = 1f;
        [SerializeField]
        private float attackCooldown =2f;
        [SerializeField]
        private GameObject meleeCollider;

        //References
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;      
        private Knockback knockback;
        private Animator animator;

        //State
        private bool isAttacking = false;
        private float attackCooldownTimer = 0;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            knockback = GetComponent<Knockback>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            meleeCollider.SetActive(false);
        }

        protected override void Start()
        {
            base.Start();
            player = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
        }

        protected override void Update()
        {
            base.Update();
            if (attackCooldownTimer <= attackCooldown) {
                attackCooldownTimer += Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (player == null) return;
            if (knockback.IsKnockbacked)
            {
                return;
            }

            

            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            Vector2 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;

            SetEnemyDirection(direction.x);

            if (GetDistanceToPlayer()< minDistanceToPlayer && !isAttacking)
            {
                //trigger attack
                TriggerAttack();
                return;
            }

            if (isAttacking)
            {
                // Wait for the attack animation to finish
                return;
            }

            rb.MovePosition(nextPosition);
        }

        private void SetEnemyDirection(float xDirection)
        {
            if (xDirection > 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }else
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        private float GetDistanceToPlayer() 
        {
            return Vector2.Distance(rb.position, player.position);
        }

        private void TriggerAttack() 
        {
            if (attackCooldownTimer < attackCooldown)
            {
                // Still in cooldown
                return;
            }
            isAttacking = true;
            animator.SetTrigger("Attack");
            meleeCollider.SetActive(true);
        }

        public void OnEndAttackAnimation() 
        {
            Debug.Log("Attack animation ended");
            isAttacking = false;
            meleeCollider.SetActive(false);
            attackCooldownTimer = 0;
        }

        public override void SetProjectileDamage(int amount)
        {           
        }
    }

}