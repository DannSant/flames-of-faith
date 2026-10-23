using System.Collections.Generic;
using System.Linq;
using Game.Effects;
using UnityEngine;

namespace Game.Utils
{
    // Converts a stat value (typically Luck) into a weighted distribution over EffectQuality
    // tiers, then rolls a single tier from it. Common always keeps weight 1 no matter how high
    // the stat gets, so it can never be fully priced out; higher tiers grow exponentially more
    // likely as the stat grows, with no point at which extra stat stops mattering.
    public static class EffectQualityScaler
    {
        private static readonly int TierCount = System.Enum.GetValues(typeof(EffectQuality)).Length;

        // Negative stat mirrors the positive curve (1 / (1 + |stat| / scale)), so higher tiers get rarer
        // but every weight stays positive. 1 + stat / scale used to reach 0 at -2 Luck and go negative
        // below it, which zeroed or flipped the higher tiers: only Common could roll, and the shop
        // threw once its few Commons ran out, killing the level load behind the black screen.
        public static float[] GetTierWeights(int statValue, float scalePerQualityStep)
        {
            float steps = scalePerQualityStep > 0f ? Mathf.Abs(statValue) / scalePerQualityStep : 0f;
            float luckFactor = statValue >= 0 ? 1f + steps : 1f / (1f + steps);

            var weights = new float[TierCount];
            for (int i = 0; i < TierCount; i++)
            {
                weights[i] = Mathf.Pow(luckFactor, i);
            }
            return weights;
        }

        public static EffectQuality RollQualityTier(int statValue, float scalePerQualityStep)
        {
            return RollTierFromWeights(GetTierWeights(statValue, scalePerQualityStep));
        }

        // Rolls a tier restricted to (and renormalized over) only the tiers actually present in
        // availableEffects, so a pool missing e.g. Legendary items never wastes a roll on it.
        public static EffectQuality RollQualityTier(int statValue, float scalePerQualityStep, IEnumerable<Effect> availableEffects)
        {
            var weights = GetTierWeights(statValue, scalePerQualityStep);
            var presentTiers = new HashSet<EffectQuality>(availableEffects.Where(e => e != null).Select(e => e.Quality));

            for (int i = 0; i < weights.Length; i++)
            {
                if (!presentTiers.Contains((EffectQuality)i))
                {
                    weights[i] = 0f;
                }
            }

            return weights.Sum() > 0f ? RollTierFromWeights(weights) : EffectQuality.Common;
        }

        // Weighted-average tier, rounded to the nearest one — a stable "what you'll typically
        // get" summary for UI, since the single highest-weight tier is a poor summary here
        // (it's always the top eligible tier the moment luckFactor exceeds 1, even barely).
        public static EffectQuality GetExpectedQualityTier(int statValue, float scalePerQualityStep)
        {
            var weights = GetTierWeights(statValue, scalePerQualityStep);
            float totalWeight = weights.Sum();
            float weightedIndex = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                weightedIndex += i * weights[i];
            }
            weightedIndex /= totalWeight;

            int index = Mathf.Clamp(Mathf.RoundToInt(weightedIndex), 0, TierCount - 1);
            return (EffectQuality)index;
        }

        private static EffectQuality RollTierFromWeights(float[] weights)
        {
            float total = weights.Sum();
            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            int lastWeighted = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f) continue;
                lastWeighted = i;
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return (EffectQuality)i;
                }
            }
            // Float rounding can leave the roll a hair above the running sum: never fall to a tier with no weight
            return (EffectQuality)lastWeighted;
        }
    }
}
