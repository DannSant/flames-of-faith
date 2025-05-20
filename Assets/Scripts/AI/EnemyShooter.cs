using Game.Combat;
using Game.Control;
using Game.Misc;
using Game.Scene;
using UnityEngine;

namespace Game.AI {
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class EnemyShooter : EnemyAIBase
    {
        private Rigidbody2D rb;
        private Animator animator;
        private Knockback knockback;

        [Header("Shooter Settings")]
        [SerializeField] private float minRange = 5f;
        [SerializeField] private float maxRange = 7f;
        [SerializeField] private float shootCooldown = 2f;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        private float speed = 5f;
        private float shootTimer = 0f;
        private bool isShooting = false;
        private int projectileDamage = 1;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            knockback = GetComponent<Knockback>();
        }

        protected override void Start()
        {
            base.Start();
            //player = PlayerController.Instance.transform;
            player = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
        }

        protected override void Update()
        {
            base.Update();

            if (player == null || isShooting) return;

            float distance = Vector2.Distance(transform.position, player.position);

            if ( distance <= maxRange)
            {
                // Within shooting range
                if (shootTimer <= 0f)
                {
                    Shoot();
                }
                else
                {
                    shootTimer -= Time.deltaTime;
                }
            }
        }

        private void FixedUpdate()
        {
            if (player == null || isShooting) return;
            if (knockback.IsKnockbacked)
            {
                return;
            }

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > maxRange)
            {
                // Chase if outside max range
                Vector2 direction = ((Vector2)player.position - rb.position).normalized;
                Vector2 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;
                rb.MovePosition(nextPosition);
            }
        }

        private void Shoot()
        {
            isShooting = true;
            shootTimer = shootCooldown;
            animator.SetTrigger("Shoot");
            // Movement paused while animating
            // Actual projectile spawn happens via animation event
        }

        // Animation event can call this to spawn the projectile mid-animation
        public void SpawnProjectile()
        {
            if (projectilePrefab == null || firePoint == null || player == null) return;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 direction = (player.position - firePoint.position).normalized;
            projectile.GetComponent<ProjectileMovement>()?.SetDirection(direction);
            projectile.GetComponent<EnemyDamage>()?.SetDamageAmount(projectileDamage);
        }

        // Called by animation event at the end of "Shoot" animation
        public void OnShootAnimationFinished()
        {
            isShooting = false;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = Mathf.Max(0f, newSpeed);
        }

        public override void SetProjectileDamage(int amount)
        {
            projectileDamage = amount;
        }
    }
}
