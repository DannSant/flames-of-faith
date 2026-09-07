using Game.Effects;
using Game.Progression;
using Game.Scene;
using UnityEngine;

namespace Game.Combat
{
    public class DamageCalculationRequest
    {
        private float damageAmount;
        private EffectStore effectStore;
        private string effectID;
        private WeaponClass weaponClass;
        private PlayerProgression playerProgression;
        private float weaponScaleDamage;
        public DamageCalculationRequest(float damageAmount, EffectStore effectStore, string effectID, WeaponClass weaponClass, PlayerProgression playerProgression, float weaponScaleDamage)
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
        public float WeaponScaleDamage => weaponScaleDamage;

        public override string ToString()
        {
            return $"DamageRequest(DamageAmount: {damageAmount}, EffectStore: {effectStore}, EffectID: {effectID}, WeaponClass: {weaponClass}, PlayerProgression: {playerProgression}, WeaponScaleDamage: {weaponScaleDamage})";
        }
    }
    public class DamageCalculator : MonoBehaviour
    {
        private static PlayerGrace playerGrace;

        private static float graceDamageMultiplier = 0.1f;

        public static float CalculateTotalDamage(DamageCalculationRequest damageRequest)
        {
            if (playerGrace == null)
            {
                FindPlayerGrace();
            }
          
            float baseDamage = damageRequest.DamageAmount;
            float progressionStatDamage = 0;
            var playerProgression = damageRequest.PlayerProgression;
            WeaponClass weaponClass = damageRequest.WeaponClass;

            if(playerProgression == null)
            {
                Debug.LogWarning("DamageCalculator: Missing PlayerProgression.");
                return baseDamage;
            }

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

            // How much of the player's damage stat this source is entitled to. A source that
            // belongs to an effect (a spawned missile, an orbiting sword) takes the share defined
            // by that effect's scalingValue, so an ability can be tuned to deal a fraction of the
            // stat instead of all of it. Anything without an effect id is the weapon itself and
            // gets the full stat.
            //
            // The default of 1 matters: GetEffectMultiplierConfig returns an empty struct
            // (scaleValue 0) for an empty/unknown id, so multiplying by it directly would zero out
            // all normal weapon damage.
            //
            // Stack count is deliberately not part of this - it multiplies how many instances an
            // effect spawns, so folding it in here as well would make stacking scale quadratically.
            float effectStatScale = 1f;
            if (!string.IsNullOrEmpty(damageRequest.EffectID))
            {
                effectStatScale = effectStore.GetEffectMultiplierConfig(damageRequest.EffectID).scaleValue;
            }

            totalDamage = Mathf.FloorToInt(
                baseDamage + progressionStatDamage * effectStatScale * damageRequest.WeaponScaleDamage);
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