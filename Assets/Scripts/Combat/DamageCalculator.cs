using Game.Effects;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat
{
    public class DamageRequest
    {
        private float damageAmount;
        private EffectStore effectStore;
        private string effectID;
        private WeaponClass weaponClass;
        private PlayerProgression playerProgression;
        private int weaponScaleDamage;
        public DamageRequest(float damageAmount, EffectStore effectStore, string effectID, WeaponClass weaponClass, PlayerProgression playerProgression, int weaponScaleDamage)
        {
            this.damageAmount = damageAmount;
            this.effectStore = effectStore;
            this.effectID = effectID;
            this.weaponClass = weaponClass;
            this.playerProgression = playerProgression;
            this.weaponScaleDamage = weaponScaleDamage;
        }

        public float DamageAmount => damageAmount;
        public EffectStore EffectStore => effectStore;
        public string EffectID => effectID;
        public WeaponClass WeaponClass => weaponClass;
        public PlayerProgression PlayerProgression => playerProgression;
        public int WeaponScaleDamage => weaponScaleDamage;
    }
    public class DamageCalculator : MonoBehaviour
    {
        private static PlayerGrace playerGrace;

        private static float graceDamageMultiplier = 0.1f;

        public static float CalculateTotalDamage(DamageRequest damageRequest)
        {
            if (playerGrace == null)
            {
                FindPlayerGrace();
            }

            float baseDamage = damageRequest.DamageAmount;
            float progressionStatDamage = 0;
            var playerProgression = damageRequest.PlayerProgression;
            WeaponClass weaponClass = damageRequest.WeaponClass;
            if (weaponClass == WeaponClass.Melee)
            {
                progressionStatDamage = playerProgression.GetStatTotal(StatType.MeleeDamage);
            }
            else if (weaponClass == WeaponClass.Ranged)
            {
                progressionStatDamage = playerProgression.GetStatTotal(StatType.RangedDamage);
            }
            else if (weaponClass == WeaponClass.Magic)
            {
                progressionStatDamage = playerProgression.GetStatTotal(StatType.MagicDamage);
            }

            var effectStore = damageRequest.EffectStore;

            float totalDamage = 0;
            float graceDamage = 0;
            float finalDamage = 0;
            if (effectStore == null)
            {
                totalDamage = baseDamage + progressionStatDamage * damageRequest.WeaponScaleDamage;
                graceDamage = GetGraceDamage(totalDamage, playerGrace.CurrentGrace);
                finalDamage = totalDamage + graceDamage;
                if (finalDamage <= 0)
                {
                    return 1f;
                }
                return Mathf.FloorToInt(finalDamage);
                 
            }
            totalDamage = Mathf.FloorToInt(baseDamage + progressionStatDamage + effectStore.GetEffectMultiplierConfig(damageRequest.EffectID).GetMultiplier());
            graceDamage = GetGraceDamage(totalDamage, playerGrace.CurrentGrace);
            finalDamage = totalDamage + graceDamage;
            if (finalDamage<=0)
            {
                return 1f;
            }
            return finalDamage;
        }

        private static float GetGraceDamage(float damage, float grace)
        {
            return damage * (graceDamageMultiplier * grace);
        }

        private static void FindPlayerGrace()
        {
            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
        }
    }
}