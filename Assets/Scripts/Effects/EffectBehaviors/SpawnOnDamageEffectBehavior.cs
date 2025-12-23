using Game.Combat.Projectiles;
using Game.Combat;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects.EffectBehaviors
{
    [CreateAssetMenu(menuName = "Effects/Behaviors/Spawn On Damage")]
    public class SpawnOnDamageEffectBehavior : EffectBehavior
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject prefabToSpawn;

        [SerializeField] private int spawnCount = 1;

        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [SerializeField] private bool randomRotation = true;
        [SerializeField] private bool randomPositionOffset = false;

        [Range(0f, 1f)]
        [SerializeField] private float chanceToSpawn = 1f;

        

        // Runtime
        private WeaponDamageSource meleeSource;
        private BowWeapon bowSource;
        private readonly List<DamageSourceBase> registeredProjectiles = new();




        public override void Initialize(GameObject owner, EffectStore store, Effect parentEffect)
        {
            base.Initialize(owner, store, parentEffect);

            // Player’s current weapon
            var weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            var currentWeapon = weaponManager.GetCurrentWeapon();

            if (currentWeapon == null)
                return;

            // Sword / melee
            if (currentWeapon is SwordWeapon sword)
            {
                meleeSource = sword.GetDamageSource();
               
                if (meleeSource != null)
                    meleeSource.OnDamageDealt += HandleDamageDealt;
            }

            // Bow / ranged
            if (currentWeapon is BowWeapon bow)
            {
                bowSource = bow;
                bowSource.onBowAttackLaunched += RegisterProjectile;
            }
        }       


        public override void Cleanup()
        {
            // Unsubscribe melee
            if (meleeSource != null)
                meleeSource.OnDamageDealt -= HandleDamageDealt;

            // Unsubscribe bow
            if (bowSource != null)
                bowSource.onBowAttackLaunched -= RegisterProjectile;

            // Unsubscribe all registered projectiles
            foreach (var p in registeredProjectiles)
            {
                if (p != null)
                    p.OnDamageDealtEvent -= HandleDamageDealt;
            }

            registeredProjectiles.Clear();
        }

        private void RegisterProjectile(DamageSourceBase projectile)
        {
            if (projectile == null) return;

            projectile.OnDamageDealtEvent += HandleDamageDealt;
            registeredProjectiles.Add(projectile);
        }


        private void HandleDamageDealt(float damage, GameObject target)
        {
            var damageable = target.GetComponent<IDamageable>();

            if (damageable == null) { return; }

            if (!damageable.ShouldSpawnEffectObject()) { return; }
               

            // Roll chance
            float roll = Random.Range(0f, 1f);
            if (roll > chanceToSpawn)
                return;
            //TODO: Use the EffectStackBehavior to modify spawn count 
            // Number of instances depends on effect stack count
            int finalCount = spawnCount * storeOwner.GetEffectMultiplierConfig(parentEffect.EffectID).count;

            for (int i = 0; i < finalCount; i++)
            {
                Quaternion rot = randomRotation
                    ? Quaternion.Euler(0, 0, Random.Range(0, 360))
                    : Quaternion.identity;

                Vector2 spawnPos = (Vector2)target.transform.position + spawnOffset;
                if (randomPositionOffset)
                {
                    Vector2 randomOffsetVec = new Vector2(
                        Random.Range(-spawnOffset.x, spawnOffset.x),
                        Random.Range(-spawnOffset.y, spawnOffset.y)
                    );
                    spawnPos += randomOffsetVec;
                }

                GameObject instance = Instantiate(prefabToSpawn, spawnPos, rot);

                // Attach effect multiplier ID
                if (instance.TryGetComponent<IEffectMultiplier>(out var multiplier))
                {
                    multiplier.SetEffectID(parentEffect.EffectID);
                }
            }
        }
    }

}