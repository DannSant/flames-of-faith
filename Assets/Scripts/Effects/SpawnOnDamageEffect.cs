using Game.Combat;
using Game.Combat.Projectiles;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects
{

    [CreateAssetMenu(fileName = "SpawnOnDamageEffect", menuName = "Effects/Spawn On Damage Effect")]
    public class SpawnOnDamageEffect : Effect
    {
        [SerializeField] private GameObject prefabToSpawn;
        [SerializeField] private int spawnCount = 1;
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;
        [SerializeField] private bool spawnAtRandomRotation = true;
        [Range(0f, 1f)]
        [SerializeField] private float chanceToSpawn = 1.0f;

        
        private WeaponDamageSource damageSource;
        private BowWeapon bow;
        private readonly List<ProjectileBase> registeredProjectiles = new();


        public override void Apply(GameObject target, EffectStore effectStore)
        {
            base.Apply(target, effectStore);
            var currentWeapon = PlayerManager.Instance.GetPlayerComponent<WeaponManager>().GetCurrentWeapon();

            if(currentWeapon!=null && currentWeapon is SwordWeapon)
            {
                damageSource = (currentWeapon as SwordWeapon).GetDamageSource();
                if (damageSource != null)
                {
                    damageSource.OnDamageDealt += HandleDamageDealt;
                }
            }
            
            if(currentWeapon != null && currentWeapon is BowWeapon)
            {
                bow = currentWeapon as BowWeapon;
                bow.onBowAttackLaunched += RegisterEventForBow;
                
            }

        }

        private void RegisterEventForBow(ProjectileBase projectile)
        {
            if (projectile != null)
            {
                projectile.OnDamageDealtEvent += HandleDamageDealt;
                registeredProjectiles.Add(projectile);
            }
        }

        public override void Cleanup()
        {
            if (damageSource != null)
            {
                damageSource.OnDamageDealt -= HandleDamageDealt;
            }

            if (bow != null)
            {
                bow.onBowAttackLaunched -= RegisterEventForBow;
            }

            foreach (var proj in registeredProjectiles)
            {
                if (proj != null)
                    proj.OnDamageDealtEvent -= HandleDamageDealt;
            }
        }

        private void HandleDamageDealt(int damage, int grace, GameObject target)
        {
            bool shouldSpawn = Random.Range(0f, 1f) <= chanceToSpawn;
            if (!shouldSpawn) return;

            for (int i = 0; i < spawnCount; i++)
            {
                Quaternion rotation = spawnAtRandomRotation
                    ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
                    : Quaternion.identity;

                Vector2 spawnPosition = (Vector2)target.transform.position + spawnOffset;
                Instantiate(prefabToSpawn, spawnPosition, rotation);
                var effectMultiplier = prefabToSpawn.GetComponent<IEffectMultiplier>();
                if (effectMultiplier!=null)
                {
                    effectMultiplier.SetEffectStore(ownerStore);
                    effectMultiplier.SetEffectID(EffectID);
                }
            }
        }
    }


}