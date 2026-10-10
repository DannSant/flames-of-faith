using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Armor mitigates a proportion of incoming damage rather than subtracting a flat amount:
    /// reduction = armor / (armor + constant), never below minDamagePercent of the raw hit.
    ///
    /// Shared by PlayerHealth (the real hit) and EnemyDamageCalculator (sizing enemy damage
    /// against the expected armor), so the two can never disagree on the math.
    /// </summary>
    public static class ArmorMitigation
    {
        /// <summary>
        /// Fraction of a raw hit that gets through the given armor (1 = no reduction).
        /// Negative armor mirrors the positive curve and increases damage instead:
        /// multiplier = 2 - constant / (constant - armor). At 25: armor -5 = +17%, -25 = +50%,
        /// and it approaches but never reaches +100%.
        /// </summary>
        public static float Multiplier(float armor, float effectivenessConstant, float minDamagePercent)
        {
            float constant = Mathf.Max(1f, effectivenessConstant);
            if (armor < 0f)
            {
                return 2f - constant / (constant - armor);
            }

            float reduction = armor / (armor + constant);
            return Mathf.Max(minDamagePercent, 1f - reduction);
        }

        public static float Apply(float amount, float armor, float effectivenessConstant, float minDamagePercent)
        {
            if (amount <= 0f) return 0f;
            return amount * Multiplier(armor, effectivenessConstant, minDamagePercent);
        }
    }
}
