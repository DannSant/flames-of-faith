using Game.AI;
using Game.Control;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class DamageSource : MonoBehaviour
    {
        private PlayerProgression playerProgression;       

        private WeaponData weaponData;
        public Action<int, int, GameObject> OnDamageDealt;       

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
            if (damageableObject != null)
            {

                int damageAmount = weaponData.baseDamage + playerProgression.GetStatTotal(StatType.MeleeDamage) * weaponData.attackScale;               
                damageableObject.TakeDamage(damageAmount);               
                OnDamageDealt?.Invoke(damageAmount, WeaponData.graceGenerated, transform.gameObject);               
                if (WeaponData.shouldApplyKnockback && knockback != null)
                {
                    var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                    knockback.ApplyKnockback(playerTransform, WeaponData.knockbackForce);
                }
            }
        }
        
    }

}