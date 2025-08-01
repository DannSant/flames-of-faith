using Game.Control;
using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class EffectDamageSource : MonoBehaviour, IEffectMultiplier
    {
        [SerializeField] private int baseDamage = 5;
        [SerializeField] private WeaponClass weaponClass = WeaponClass.None;
        [SerializeField] private float knockbackForce = 5f;
        private PlayerProgression playerProgression;
        private EffectStore effectStore;
        private string effectID;
        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            ProcessDamageToEnemies(collision);
        }

        private void ProcessDamageToEnemies(Collider2D collision)
        {

            IDamageable damageableObject = collision.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                int totalDamage = CalculateTotalDamage();               
                damageableObject.TakeDamage(totalDamage);
            }

            Knockback knockback = collision.GetComponent<Knockback>();
            if (knockback != null)
            {
                var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                knockback.ApplyKnockback(playerTransform, knockbackForce);
            }
        }

        private int CalculateTotalDamage()
        {
            int damage = 0;
            if (weaponClass == WeaponClass.Melee)
            {
                damage = playerProgression.GetStatTotal(StatType.MeleeDamage);
            }
            else if (weaponClass == WeaponClass.Ranged)
            {
                damage = playerProgression.GetStatTotal(StatType.RangedDamage);
            }
            else if (weaponClass == WeaponClass.Magic)
            {
                damage = playerProgression.GetStatTotal(StatType.MagicDamage);
            }
            return Mathf.FloorToInt(baseDamage + damage + effectStore.GetEffectMultiplierConfig(effectID).GetMultiplier());
        }

        public void SetEffectStore(EffectStore effectStore)
        {
            //this.effectStore = effectStore;
        }

        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }
    }
}
