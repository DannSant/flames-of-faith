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
        private float stackDamageMultiplier;
        public DamageCalculationRequest(float damageAmount, EffectStore effectStore, string effectID, WeaponClass weaponClass, PlayerProgression playerProgression, float weaponScaleDamage, float stackDamageMultiplier = 1f)
        {
            this.damageAmount = damageAmount;
            this.effectStore = effectStore;
            this.effectID = effectID;
            this.weaponClass = weaponClass;
            this.playerProgression = playerProgression;
            this.weaponScaleDamage = weaponScaleDamage;
            this.stackDamageMultiplier = stackDamageMultiplier;
        }

        public float DamageAmount => damageAmount;
        public EffectStore EffectStore => effectStore;
        public string EffectID => effectID;
        public WeaponClass WeaponClass => weaponClass;
        public PlayerProgression PlayerProgression => playerProgression;
        public float WeaponScaleDamage => weaponScaleDamage;

        // Set by the spawning EffectBehavior when the effect authors a Damage scaling rule, and
        // left at 1 otherwise. Passed in rather than derived from EffectID inside the calculator
        // on purpose: an effect that scales its spawn count instead passes 1 here, which is what
        // makes the quadratic case structurally impossible rather than merely discouraged.
        public float StackDamageMultiplier => stackDamageMultiplier;

        public override string ToString()
        {
            return $"DamageRequest(DamageAmount: {damageAmount}, EffectStore: {effectStore}, EffectID: {effectID}, WeaponClass: {weaponClass}, PlayerProgression: {playerProgression}, WeaponScaleDamage: {weaponScaleDamage}, StackDamageMultiplier: {stackDamageMultiplier})";
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
                totalDamage = (baseDamage + progressionStatDamage * damageRequest.WeaponScaleDamage)
                    * damageRequest.StackDamageMultiplier;
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
            // Stack count is deliberately not looked up here - an effect that stacks its damage
            // hands the resolved multiplier in via StackDamageMultiplier instead. Deriving it from
            // the id at this point would apply it to effects that stack their spawn count too, and
            // N times as many objects each hitting N times as hard is quadratic.
            float effectStatScale = 1f;
            if (!string.IsNullOrEmpty(damageRequest.EffectID))
            {
                effectStatScale = effectStore.GetEffectMultiplierConfig(damageRequest.EffectID).scaleValue;
            }

            // The multiplier covers the whole sum, not just the progression term: effect prefabs
            // are commonly WeaponClass.None, where that term is 0 and scaling it would be a no-op.
            totalDamage = Mathf.FloorToInt(
                (baseDamage + progressionStatDamage * effectStatScale * damageRequest.WeaponScaleDamage)
                * damageRequest.StackDamageMultiplier);
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