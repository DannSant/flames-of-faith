using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat.Projectiles
{


    [Obsolete("Use DamageSourceBase and a child of ProjectileMovementBase instead.")]
    public abstract class ProjectileBase : MonoBehaviour, IEffectMultiplier
    {

        [SerializeField] protected float speed=5;
        [SerializeField] protected ProjectileType projectileType = ProjectileType.Linear;
        
        protected float baseDamage=0;

        protected float lifetime;
        protected int pierceCount;     
        protected Vector2 moveDirection;
        protected bool damageToPlayer = false;
        protected int graceGenerated = 1;
        protected PlayerProgression playerProgression;
        protected WeaponData weaponData;
        private float timer;
        private EffectStore effectStore;
        private string effectID;

        int enemyLayerMask;
        int playerLayerMask;

        public Action<float, int, GameObject> OnDamageDealtEvent;

        private void Awake()
        {
            enemyLayerMask = LayerMask.NameToLayer("Enemy");
            playerLayerMask = LayerMask.NameToLayer("Player");
        }

        private void Start()
        {
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        }

        public virtual void Initialize(Vector2 direction,float baseDamage, int pierce, float lifeTime, bool damageToPlayer, int graceGenerated, PlayerProgression playerProgression, WeaponData weaponData)
        {
            this.moveDirection = direction.normalized;
            this.baseDamage = baseDamage;
            this.pierceCount = pierce;
            this.lifetime = lifeTime;
            this.damageToPlayer = damageToPlayer;
            this.graceGenerated = graceGenerated;
            this.playerProgression = playerProgression;
            this.weaponData = weaponData;
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
                OnDamageDealtEvent?.Invoke(baseDamage, graceGenerated, gameObject);
            }
            else if (!damageToPlayer && other.gameObject.layer == enemyLayerMask)
            {
                DamageEnemy(other.GetComponent<EnemyHealth>());
                OnDamageDealtEvent?.Invoke(baseDamage, graceGenerated,gameObject);
            }         

        }

        public virtual void ConfigureAfterSpawn()
        {

        }

        private void DamagePlayer(PlayerHealth playerHealth)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(baseDamage);
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

                float totalDamage = CalculateTotalDamage();
                DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(enemyHealth.transform.position, totalDamage);
                enemyHealth.TakeDamage(totalDamage);
                pierceCount--;
                if (pierceCount <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        private float CalculateTotalDamage()
        {
            return DamageCalculator.CalculateTotalDamage(
                new DamageRequest(
                    baseDamage,
                    effectStore,
                    effectID,
                    damageToPlayer ? WeaponClass.None : WeaponClass.Ranged,
                    playerProgression,
                    weaponData.attackScale
                )
            );
          
        }

        public ProjectileType GetProjectileType() => projectileType;


        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }
    }
}