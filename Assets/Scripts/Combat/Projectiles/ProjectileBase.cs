using System;
using UnityEngine;

namespace Game.Combat.Projectiles
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        [SerializeField] protected float speed=5;
        [SerializeField] private int graceGenerated = 1;
        protected float lifetime;
        protected int pierceCount;
        protected int damageAmount;
        protected Vector2 moveDirection;
        protected bool damageToPlayer = false;
        private float timer;

        int enemyLayerMask;
        int playerLayerMask;

        public Action<int, int> OnDamageDealtEvent;

        private void Awake()
        {
            enemyLayerMask = LayerMask.NameToLayer("Enemy");
            playerLayerMask = LayerMask.NameToLayer("Player");
        }

        public virtual void Initialize(Vector2 direction, int amount, int pierce, float lifeTime, bool damageToPlayer)
        {
            this.moveDirection = direction.normalized;
            this.damageAmount = amount;
            this.pierceCount = pierce;
            this.lifetime = lifeTime;
            this.damageToPlayer = damageToPlayer;
            timer = 0f;
        }

        protected virtual void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            MoveBehavior();
        }

        protected abstract void MoveBehavior();

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (damageToPlayer && other.gameObject.layer == playerLayerMask)
            {
                DamagePlayer(other.GetComponent<PlayerHealth>());
                OnDamageDealtEvent?.Invoke(damageAmount, graceGenerated);
            }
            else if (!damageToPlayer && other.gameObject.layer == enemyLayerMask)
            {
                DamageEnemy(other.GetComponent<Health>());
                OnDamageDealtEvent?.Invoke(damageAmount, graceGenerated);
            }         

        }

        public virtual void ConfigureAfterSpawn()
        {

        }

        private void DamagePlayer(PlayerHealth playerHealth)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                pierceCount--;
                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void DamageEnemy(Health enemyHealth)
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
                pierceCount--;
                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}