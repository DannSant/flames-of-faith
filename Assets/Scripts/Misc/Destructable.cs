using Game.Combat;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Misc
{
    public class Destructable : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health = 1f;
        [SerializeField] private GameObject destroyVFX;
        [SerializeField] private List<WeaponClass> immuneDamageTypes = new();

        private float currentHealth;
        private void Awake()
        {
            currentHealth = health;
        }

        public void TakeDamage(DamageRequest damageRequest)
        {
            if (damageRequest == null)
            {
                Debug.LogWarning("DamageRequest is null!");
                return;
            }

            float damage = damageRequest.baseDamage;
            WeaponClass weaponClass = damageRequest.originWeaponClass;

            if (immuneDamageTypes.Contains(weaponClass))
            {
                return;
            }

            currentHealth -= damage;

          

            if (currentHealth>0)
            {
                return;
            }

            if(destroyVFX != null)
            {
                Instantiate(destroyVFX, transform.position, Quaternion.identity);
            }
        
            Destroy(gameObject);
        }

        public bool ShouldSpawnDamageNumber()
        {
            return false;
        }

        public bool ShouldSpawnEffectObject()
        {
            return false;
        }
    }

}