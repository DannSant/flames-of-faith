using Game.AI;
using Game.Combat.Elemental;
using Game.Control;
using Game.Effects;
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
        private EffectStore effectStore;

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
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
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

            if (damageableObject==null)
            {
                return;
            }

            if (damageableObject.IsDead())
            {
                return;
            }

            if (weaponData != null)
            {

              
                var damageRequest = new DamageCalculationRequest(weaponData.baseDamage, null, "", WeaponClass.Melee, playerProgression, weaponData.attackScale);
                float damageAmount = DamageCalculator.CalculateTotalDamage(damageRequest);
                damageableObject.TakeDamage(new DamageRequest(damageAmount, weaponData.weaponClass, true));

                if (damageableObject.ShouldSpawnDamageNumber() && !damageableObject.IsImmune())
                {
                    DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(collision.transform.position, damageAmount, weaponData.weaponClass);
                }

                OnDamageDealt?.Invoke(damageAmount, transform.gameObject);
                if (WeaponData.shouldApplyKnockback && knockback != null)
                {
                    var playerTransform = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
                    knockback.ApplyKnockback(playerTransform, WeaponData.knockbackForce);
                }

                // Elemental Debuff (optional)
                TryApplyElementalDebuff(collision);
            }
        }

        /// <summary>
        /// Mirrors DamageSourceBase.TryApplyElementalDebuff for melee hits. This component only ever
        /// sits on a weapon hitbox, so the damage origin is always DamageOriginType.Weapon.
        /// </summary>
        private void TryApplyElementalDebuff(Collider2D collision)
        {
            var debuffHandler = collision.GetComponent<DebuffHandler>();
            if (debuffHandler == null) return;

            //Weapon debuff check
            if (weaponData.elementalDebuffData != null && weaponData.elementalDebuffData.ElementalType != ElementalType.None)
            {
                int debuffStrengthStat = playerProgression.GetStatTotal(StatType.MastowAffinity);
                debuffHandler.TryToApplyDebuff(weaponData.elementalDebuffData, debuffStrengthStat);
            }

            //Effect debuff check
            if (effectStore == null) return;

            var debuffsToApply = effectStore.GetElementalTypesToApply(DamageOriginType.Weapon, weaponData.weaponClass);
            foreach (var debuffData in debuffsToApply)
            {
                int debuffStrengthStat = playerProgression.GetStatTotal(StatType.MastowAffinity) + debuffData.count;
                debuffHandler.TryToApplyDebuff(debuffData.elementalDebuffData, debuffStrengthStat);
            }
        }

    }

}