using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat.Projectiles
{
    public enum ProjectileType
    {
        Linear,
        Homing,
        Bouncing,
        Explosive
    }

    public abstract class ProjectileBase : MonoBehaviour, IEffectMultiplier
    {
        [SerializeField] protected float speed=5;
        [SerializeField] protected ProjectileType projectileType = ProjectileType.Linear;
        [SerializeField] protected int baseDamage=0;

        protected float lifetime;
        protected int pierceCount;
        protected int damageAmount;
        protected Vector2 moveDirection;
        protected bool damageToPlayer = false;
        protected int graceGenerated = 1;
        private float timer;
        private EffectStore effectStore;
        private string effectID;

        int enemyLayerMask;
        int playerLayerMask;

        public Action<int, int, GameObject> OnDamageDealtEvent;

        private void Awake()
        {
            enemyLayerMask = LayerMask.NameToLayer("Enemy");
            playerLayerMask = LayerMask.NameToLayer("Player");
        }

        private void Start()
        {
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        }

        public virtual void Initialize(Vector2 direction, int damageAmount, int pierce, float lifeTime, bool damageToPlayer, int graceGenerated)
        {
            this.moveDirection = direction.normalized;
            this.damageAmount = damageAmount;
            this.pierceCount = pierce;
            this.lifetime = lifeTime;
            this.damageToPlayer = damageToPlayer;
            this.graceGenerated = graceGenerated;
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
                OnDamageDealtEvent?.Invoke(damageAmount, graceGenerated, gameObject);
            }
            else if (!damageToPlayer && other.gameObject.layer == enemyLayerMask)
            {
                DamageEnemy(other.GetComponent<EnemyHealth>());
                OnDamageDealtEvent?.Invoke(damageAmount, graceGenerated,gameObject);
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

        private void DamageEnemy(EnemyHealth enemyHealth)
        {
            if (enemyHealth != null)
            {

                int totalDamage = CalculateTotalDamage();
                DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(enemyHealth.transform.position, totalDamage);
                enemyHealth.TakeDamage(totalDamage);
                pierceCount--;
                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        private int CalculateTotalDamage()
        {
            
            return Mathf.FloorToInt(baseDamage + damageAmount +  effectStore.GetEffectMultiplierConfig(effectID).GetMultiplier());
        }

        public ProjectileType GetProjectileType() => projectileType;


        /*public void SetEffectStore(EffectStore effectStore)
        {
           
        }*/

        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }
    }
}