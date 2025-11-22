using Game.Effects;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat
{
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
                damageableObject.TakeDamage(totalDamage);
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
            /*int damage = 0;
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
           
            EffectMultiplierConfig effectMultiplierConfig = effectStore.GetEffectMultiplierConfig(effectID);
            

            return Mathf.FloorToInt(baseDamage + damage + effectStore.GetEffectMultiplierConfig(effectID).GetMultiplier());*/
        }
        /*public void SetEffectStore(EffectStore effectStore)
        {
            //this.effectStore = effectStore;
        }*/

        public void SetEffectID(string effectID)
        {
            this.effectID = effectID;
        }

    }
}
