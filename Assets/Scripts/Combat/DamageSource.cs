using Game.AI;
using Game.Control;
using Game.Misc;
using Game.Progression;
using UnityEngine;

namespace Game.Combat
{
    public class DamageSource : MonoBehaviour
    {
        
        [SerializeField] private bool shouldApplyKnockback = true;
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private int graceGenerated = 1;

      
        private WeaponData weaponData;

        public WeaponData WeaponData
        {
            get => weaponData;
            set => weaponData = value;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {            
            ProcessDamageToEnemies(collision);

        }


        private void ProcessDamageToEnemies(Collider2D collision) 
        {
            IDamageable damageableObject = collision.GetComponent<IDamageable>();
            Knockback knockback = collision.GetComponent<Knockback>();
            if (damageableObject != null)
            {

                int damageAmount = weaponData.baseDamage + PlayerProgression.Instance.GetStatTotal(StatType.MeleeDamage) * weaponData.attackScale;               
                damageableObject.TakeDamage(damageAmount);
                PlayerGrace.Instance.AddGrace(graceGenerated);
                if (shouldApplyKnockback && knockback != null)
                {
                    knockback.ApplyKnockback(PlayerController.Instance.transform, knockbackForce);
                }
            }
        }
        
    }

}