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

        // Per-owner runtime state - see EffectBehaviorContext for why this can't live directly
        // on this ScriptableObject.
        private class RuntimeState
        {
            public WeaponDamageSource meleeSource;
            public BowWeapon bowSource;
            public ScepterWeapon scepterSource;
            public readonly List<DamageSourceBase> registeredProjectiles = new();
        }

        public override void Initialize(EffectBehaviorContext context, Effect parentEffect)
        {
            base.Initialize(context, parentEffect);

            var state = GetState<RuntimeState>();

            // Player�s current weapon
            var weaponManager = PlayerManager.Instance.GetPlayerComponent<WeaponManager>();
            var currentWeapon = weaponManager.GetCurrentWeapon();

            if (currentWeapon == null)
                return;

            // Sword / melee
            if (currentWeapon is SwordWeapon sword)
            {
                state.meleeSource = sword.GetDamageSource();

                if (state.meleeSource != null)
                    state.meleeSource.OnDamageDealt += HandleDamageDealt;
            }

            // Bow / ranged
            if (currentWeapon is BowWeapon bow)
            {
                state.bowSource = bow;
                state.bowSource.onBowAttackLaunched += RegisterProjectile;
            }

            if (currentWeapon is ScepterWeapon scepter)
            {
                state.scepterSource = scepter;
                state.scepterSource.onScepterAttackLaunched += RegisterProjectile;
            }
        }


        public override void Cleanup()
        {
            var state = GetState<RuntimeState>();

            // Unsubscribe melee
            if (state.meleeSource != null)
                state.meleeSource.OnDamageDealt -= HandleDamageDealt;

            // Unsubscribe bow
            if (state.bowSource != null)
                state.bowSource.onBowAttackLaunched -= RegisterProjectile;

            // Unsubscribe scepter
            if (state.scepterSource != null)
                state.scepterSource.onScepterAttackLaunched -= RegisterProjectile;

            // Unsubscribe all registered projectiles
            foreach (var p in state.registeredProjectiles)
            {
                if (p != null)
                    p.OnDamageDealtEvent -= HandleDamageDealt;
            }

            state.registeredProjectiles.Clear();
        }

        private void RegisterProjectile(DamageSourceBase projectile)
        {
            if (projectile == null) return;

            projectile.OnDamageDealtEvent += HandleDamageDealt;
            GetState<RuntimeState>().registeredProjectiles.Add(projectile);
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