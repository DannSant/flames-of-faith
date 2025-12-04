using Game.Scene;
using UnityEngine;
using Game.Progression;
using System;

namespace Game.Combat.Projectiles
{
    [Obsolete("Use DamageSourceBase and a child of ProjectileMovementBase instead.")]
    public class ProjectileInitializer : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private bool damageToPlayer = false;
        [SerializeField] private bool infinitePierce = false;

        private ProjectileBase projectile;
        private void Awake()
        {
            projectile = GetComponent<ProjectileBase>();
        }

        private void Start()
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

            // Get currently equipped weapon
            var currentWeapon = PlayerManager.Instance.GetPlayerComponent<WeaponManager>().GetCurrentWeapon();
            var weaponData = currentWeapon.GetWeaponData();

            //Get player progression object
            var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();

            //
            //int statDamage =  playerProgression.GetStatTotal(StatType.RangedDamage);

            //int damage = statDamage;         
            int pierce = infinitePierce ? 9999 :  playerProgression.GetStatTotal(StatType.PierceAmount);


            // Initialize and launch
            projectile.Initialize(direction, weaponData.baseDamage, pierce, lifetime, damageToPlayer, 0, playerProgression, weaponData);
            projectile.ConfigureAfterSpawn();
        }
        
    }
}