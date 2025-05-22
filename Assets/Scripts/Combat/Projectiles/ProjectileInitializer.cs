using Game.Scene;
using UnityEngine;
using Game.Progression;

namespace Game.Combat.Projectiles
{
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

            //If the weapon is a melee weapon, get the melee damage stat, if not get the ranged damage stat
            int statDamage = weaponData.weaponClass == WeaponClass.Melee ?
                playerProgression.GetStatTotal(StatType.MeleeDamage) : playerProgression.GetStatTotal(StatType.RangedDamage);

            int damage = weaponData.baseDamage + statDamage;         
            int pierce = infinitePierce ? 9999 : weaponData.pierceAmount + playerProgression.GetStatTotal(StatType.PierceAmount);


            // Initialize and launch
            projectile.Initialize(direction, damage, pierce, lifetime, damageToPlayer, 0);
            projectile.ConfigureAfterSpawn();
        }
        
    }
}