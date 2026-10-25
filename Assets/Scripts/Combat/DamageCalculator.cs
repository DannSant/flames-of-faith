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
            float finalDamage = 0;
            if (effectStore == null)
            {
                totalDamage = (baseDamage + progressionStatDamage * damageRequest.WeaponScaleDamage)
                    * damageRequest.StackDamageMultiplier;
                finalDamage = totalDamage * GetGraceDamageMultiplier();
                if (finalDamage <= 0)
                {
                    return 1f;
                }
                return Mathf.FloorToInt(finalDamage);
                 
            }

            // The share of the damage stat is the source's own (WeaponScaleDamage): its spawner set it, from the
            // item behavior's damage profile, the ability tier or the weapon (the item's old scalingValue is folded
            // into the item behaviors' Stat Scale, Oct 2026).
            //
            // Stack count is deliberately not looked up here - an effect that stacks its damage
            // hands the resolved multiplier in via StackDamageMultiplier instead. Deriving it from
            // the id at this point would apply it to effects that stack their spawn count too, and
            // N times as many objects each hitting N times as hard is quadratic.

            // The multiplier covers the whole sum, not just the progression term: effect prefabs
            // are commonly WeaponClass.None, where that term is 0 and scaling it would be a no-op.
            totalDamage = Mathf.FloorToInt(
                (baseDamage + progressionStatDamage * damageRequest.WeaponScaleDamage)
                * damageRequest.StackDamageMultiplier);
            finalDamage = totalDamage * GetGraceDamageMultiplier();
            if (finalDamage<=0)
            {
                return 1f;
            }
            return finalDamage;
        }

        // Positive Grace increases damage, negative Grace (Corrupted) reduces it down to a configured floor
        private static float GetGraceDamageMultiplier()
        {
            return playerGrace != null ? playerGrace.GetDamageMultiplier() : 1f;
        }

        private static void FindPlayerGrace()
        {
            playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
        }
    }
}