using Game.AI;
using Game.Control;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat
{
    public class DamageSource : MonoBehaviour
    {
        
        [SerializeField] private bool shouldApplyKnockback = true;
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private int graceGenerated = 1;

        private PlayerProgression playerProgression;

        private WeaponData weaponData;

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
            Knockback knockback = collision.GetComponent<Knockback>();
            if (damageableObject != null)
            {

                int damageAmount = weaponData.baseDamage + playerProgression.GetStatTotal(StatType.MeleeDamage) * weaponData.attackScale;               
                damageableObject.TakeDamage(damageAmount);
                //TODO create event for this
                //PlayerGrace.Instance.AddGrace(graceGenerated);
                if (shouldApplyKnockback && knockback != null)
                {
                    var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                    knockback.ApplyKnockback(playerTransform, knockbackForce);
                }
            }
        }
        
    }

}