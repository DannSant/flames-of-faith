using Game.Control;
using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat
{
    [Obsolete("Use DamageSourceBase instead")]
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
                float totalDamage = CalculateTotalDamage();
                DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(collision.transform.position, totalDamage);
                damageableObject.TakeDamage(totalDamage);
            }

            Knockback knockback = collision.GetComponent<Knockback>();
            if (knockback != null)
            {
                var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                knockback.ApplyKnockback(playerTransform, knockbackForce);
            }
        }

        private float CalculateTotalDamage()
        {
            return DamageCalculator.CalculateTotalDamage(
               new DamageRequest(
                   baseDamage,
                   effectStore,
                   effectID,
                   weaponClass,
                   playerProgression,
                   1
                   ));
        }



        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }
    }
}
