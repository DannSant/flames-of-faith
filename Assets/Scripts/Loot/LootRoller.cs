using Game.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Loot
{
    /// <summary>
    /// The item roll, pure: which items are eligible, the quality odds with Luck, and the pick. The random number
    /// comes in as a function returning [0, 1), so tests are deterministic. <see cref="ItemProvider"/> feeds it the
    /// real table, settings and player state; the balance report uses the same odds.
    /// </summary>
    public static class LootRoller
    {
        public static readonly int TierCount = Enum.GetValues(typeof(EffectQuality)).Length;

        /// <summary>
        /// The Luck multiplier per quality step: 1 + |Luck| / scale for positive Luck, and its mirror
        /// 1 / (1 + |Luck| / scale) for negative Luck, so higher qualities get rarer but no weight reaches 0 (the old
        /// 1 + Luck / scale hit 0 at -2 Luck and emptied the shop). A scale of 0 ignores Luck.
        /// </summary>
        public static float GetLuckFactor(int luck, float scalePerStep)
        {
            float steps = scalePerStep > 0f ? Mathf.Abs(luck) / scalePerStep : 0f;
            return luck >= 0 ? 1f + steps : 1f / (1f + steps);
        }

        /// <summary>Each quality's weight at this Luck: base weight x luckFactor^tier (Common = tier 0).</summary>
        public static float[] GetTierWeights(LootSourceSettings settings, int luck)
        {
            float factor = GetLuckFactor(luck, settings.luckScalePerStep);
            var weights = new float[TierCount];
            for (int i = 0; i < TierCount; i++)
            {
                weights[i] = Mathf.Max(0f, settings.GetBaseWeight((EffectQuality)i)) * Mathf.Pow(factor, i);
            }
            return weights;
        }

        /// <summary>
        /// The items that can drop from <paramref name="source"/> now: flagged for it, with a weight above 0, a min
        /// levels beaten the player has reached, not starting-only (class kit items), and unlocked.
        /// </summary>
        public static List<ItemDropEntry> GetEligible(IEnumerable<ItemDropEntry> entries, LootSource source, int levelsBeaten,
            Func<Effect, bool> isUnlocked)
        {
            return entries
                .Where(e => e != null && e.item != null && e.DropsFrom(source) && e.weight > 0f
                    && e.minLevelsBeaten <= levelsBeaten && !e.item.IsStartingOnly && (isUnlocked == null || isUnlocked(e.item)))
                .ToList();
        }

        /// <summary>
        /// The chance of each quality among <paramref name="eligible"/> (0..1, summing to 1, or all 0 when nothing can
        /// drop): the tier weights with every quality that has no eligible item left out.
        /// </summary>
        public static float[] GetTierOdds(IReadOnlyList<ItemDropEntry> eligible, LootSourceSettings settings, int luck)
        {
            var weights = GetTierWeights(settings, luck);
            for (int i = 0; i < TierCount; i++)
            {
                if (!eligible.Any(e => (int)e.item.Quality == i)) weights[i] = 0f;
            }
            float total = weights.Sum();
            for (int i = 0; i < TierCount; i++)
            {
                weights[i] = total > 0f ? weights[i] / total : 0f;
            }
            return weights;
        }

        /// <summary>One item from <paramref name="eligible"/>, or null when nothing can drop.</summary>
        public static Effect RollOne(IReadOnlyList<ItemDropEntry> eligible, LootSourceSettings settings, int luck, Func<double> random)
        {
            var odds = GetTierOdds(eligible, settings, luck);
            int tier = Pick(odds, random());
            if (tier < 0) return null;

            var inTier = eligible.Where(e => (int)e.item.Quality == tier).ToList();
            int index = Pick(inTier.Select(e => e.weight).ToArray(), random());
            return index < 0 ? null : inTier[index].item;
        }

        /// <summary>
        /// Up to <paramref name="count"/> items, each rolled on its own. With <paramref name="unique"/> an item is
        /// taken out of the pool once picked (the shop), so fewer come back when the pool runs out.
        /// </summary>
        public static List<Effect> Roll(IReadOnlyList<ItemDropEntry> eligible, LootSourceSettings settings, int luck, int count,
            bool unique, Func<double> random)
        {
            var pool = new List<ItemDropEntry>(eligible);
            var result = new List<Effect>();
            for (int i = 0; i < count; i++)
            {
                var item = RollOne(pool, settings, luck, random);
                if (item == null) break;
                result.Add(item);
                if (unique) pool.RemoveAll(e => e.item == item);
            }
            return result;
        }

        /// <summary>The weighted-average quality at this Luck, rounded: a stable "what you'll typically get" for the UI.</summary>
        public static EffectQuality GetExpectedQuality(IReadOnlyList<ItemDropEntry> eligible, LootSourceSettings settings, int luck)
        {
            var odds = GetTierOdds(eligible, settings, luck);
            float average = 0f;
            for (int i = 0; i < TierCount; i++) average += i * odds[i];
            return (EffectQuality)Mathf.Clamp(Mathf.RoundToInt(average), 0, TierCount - 1);
        }

        /// <summary>
        /// How many items the shop offers: the base count plus (or, with negative Luck, minus) the Luck bonus, kept
        /// between the min and the max. If the min is set above the max, the min wins.
        /// </summary>
        public static int GetShopItemCount(int baseCount, int luck, int perLuck, int minCount, int maxCount)
        {
            return Mathf.Clamp(baseCount + luck * perLuck, minCount, Mathf.Max(minCount, maxCount));
        }

        /// <summary>The index picked by <paramref name="roll"/> (0..1) over <paramref name="weights"/>; -1 if they're all 0.</summary>
        public static int Pick(IReadOnlyList<float> weights, double roll)
        {
            float total = 0f;
            for (int i = 0; i < weights.Count; i++) total += Mathf.Max(0f, weights[i]);
            if (total <= 0f) return -1;

            double target = Math.Clamp(roll, 0d, 1d) * total;
            double cumulative = 0d;
            int last = -1;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] <= 0f) continue;
                last = i;
                cumulative += weights[i];
                if (target < cumulative) return i;
            }
            // A roll of exactly 1, or float rounding: the last weighted entry, never one with no weight
            return last;
        }
    }
}
