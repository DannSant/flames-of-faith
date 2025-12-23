using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Combat
{
    [Obsolete("Use DamageSourceBase instead")]
    public class ContinuousDamageSource : MonoBehaviour, IEffectMultiplier
    {
        [SerializeField] private float baseDurationTime = 3f;
        [SerializeField] private float timeInterval = 1f;
        [SerializeField] private int baseDamage = 5;
        [SerializeField] private WeaponClass weaponClass = WeaponClass.None;

        private PlayerProgression playerProgression;

        private float damageTimer = 0;
        private EffectStore effectStore;
        private string effectID;


        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            float totalTime = baseDurationTime + playerProgression.GetStatTotal(StatType.SkillDuration);
           
            Destroy(gameObject, totalTime);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            int statDamage = 0;
            if (weaponClass == WeaponClass.Melee)
            {
                statDamage = playerProgression.GetStatTotal(StatType.MeleeDamage);
            }
            else if (weaponClass == WeaponClass.Ranged)
            {
                statDamage = playerProgression.GetStatTotal(StatType.RangedDamage);
            }else if (weaponClass == WeaponClass.Magic)
            {
                statDamage = playerProgression.GetStatTotal(StatType.MagicDamage);
            }

            if (damageTimer < timeInterval)
            {
                damageTimer += Time.deltaTime;
                return;
            }

            damageTimer = 0;

            IDamageable damageableObject = collision.GetComponent<IDamageable>();
            if (damageableObject != null) {
                float totalDamage = CalculateTotalDamage();
                DamageNumberSpawner.Instance.SpawnDamageToEnemyNumber(collision.transform.position, totalDamage);
               
            }

        }
        private float CalculateTotalDamage()
        {
            return DamageCalculator.CalculateTotalDamage(
                new DamageCalculationRequest(
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
