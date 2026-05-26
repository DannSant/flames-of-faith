using Game.AI;
using Game.Control;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class WeaponDamageSource : MonoBehaviour
    {
        private PlayerProgression playerProgression;       

        private WeaponData weaponData;
        public Action<float, GameObject> OnDamageDealt;       

        public WeaponData WeaponData
        {
            get => weaponData;
            set => weaponData = value;
        }

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();           
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {            
            ProcessDamageToEnemies(collision);
        }


        private void ProcessDamageToEnemies(Collider2D collision) 
        {
            IDamageable damageableObject = collision.GetComponent<IDamageable>();
            Transform transform = collision.transform;
            Knockback knockback = collision.GetComponent<Knockback>();
            if (damageableObject != null && weaponData != null)
            {

              
                var damageRequest = new DamageCalculationRequest(weaponData.baseDamage, null, "", WeaponClass.Melee, playerProgression, weaponData.attackScale);
                float damageAmount = DamageCalculator.CalculateTotalDamage(damageRequest);
                damageableObject.TakeDamage(new DamageRequest(damageAmount, weaponData.weaponClass, true));

                if (damageableObject.ShouldSpawnDamageNumber() && !damageableObject.IsImmune())
                {
                    DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(collision.transform.position, damageAmount);
                }

                OnDamageDealt?.Invoke(damageAmount, transform.gameObject);               
                if (WeaponData.shouldApplyKnockback && knockback != null)
                {
                    var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                    knockback.ApplyKnockback(playerTransform, WeaponData.knockbackForce);
                }
            }
        }
        
    }

}