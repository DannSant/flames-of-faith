using Game.Effects;
using System;
using System.Collections.Generic;

namespace Game.Progression
{
    /// <summary>
    /// The player's final stat totals, as pure math. Used by PlayerProgression for the real stats
    /// and for previews (level-up choices), so a preview can never disagree with the result.
    /// </summary>
    public static class FinalStatsCalculator
    {
        /// <summary>
        /// For every stat in <paramref name="baseStats"/>:
        /// 1. (base + flat item bonuses) × (1 + % item bonuses) − corruption penalty;
        /// 2. then the stat bonus rules, read against the totals from step 1 (rules never chain).
        /// </summary>
        /// <param name="preBonusTotals">Optional; filled with the step 1 totals.</param>
        public static Dictionary<StatType, float> Calculate(
            IReadOnlyDictionary<StatType, int> baseStats,
            IEnumerable<StatModifier> modifiers,
            Func<StatType, float> corruptionPenalty,
            IEnumerable<(StatDependencyRule rule, int count)> rules,
            Dictionary<StatType, float> preBonusTotals = null)
        {
            var finalStats = new Dictionary<StatType, float>();
            if (baseStats == null) return finalStats;

            var modifierList = modifiers != null ? new List<StatModifier>(modifiers) : new List<StatModifier>();

            foreach (var pair in baseStats)
            {
                float flatBonus = 0f;
                float percentBonus = 0f;

                foreach (var modifier in modifierList)
                {
                    if (modifier == null || modifier.stat != pair.Key) continue;

                    if (modifier.type == ModifierType.Flat)
                        flatBonus += modifier.value;
                    else if (modifier.type == ModifierType.PercentAdd)
                        percentBonus += modifier.value;
                }

                float total = (pair.Value + flatBonus) * (1f + percentBonus);
                // Negative Grace (Corrupted) reduces the stats flagged as affected by corruption
                finalStats[pair.Key] = total - (corruptionPenalty != null ? corruptionPenalty(pair.Key) : 0f);
            }

            if (preBonusTotals != null)
            {
                preBonusTotals.Clear();
                foreach (var pair in finalStats) preBonusTotals[pair.Key] = pair.Value;
            }

            // Stat bonus rules (e.g. every Retribution point: +1 Armor), read against the totals
            // above, in one pass, so a rule never feeds another rule.
            var snapshot = new Dictionary<StatType, float>(finalStats);
            var bonuses = StatDependencyCalculator.CalculateBonuses(snapshot, rules);
            foreach (var bonus in bonuses)
            {
                if (finalStats.ContainsKey(bonus.Key))
                {
                    finalStats[bonus.Key] += bonus.Value;
                }
            }

            return finalStats;
        }
    }
}
