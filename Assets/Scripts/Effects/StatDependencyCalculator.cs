using Game.Progression;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Pure maths for stat dependency rules. Bonuses are computed once, from the stat totals
    /// before any rule applies: a rule never sees another rule's bonus, so a rule that feeds its
    /// own source ("per Evasion point, +1 Evasion") simply doubles it, and rules never chain.
    /// </summary>
    public static class StatDependencyCalculator
    {
        /// <summary>
        /// Bonus one rule gives, applied <paramref name="stackCount"/> times (an item owned twice).
        /// A source of 0 or less gives nothing.
        /// </summary>
        public static float CalculateRuleBonus(StatDependencyRule rule, float sourceTotal, int stackCount = 1)
        {
            if (rule == null || stackCount <= 0) return 0f;

            int perPoints = Mathf.Max(1, rule.perPoints);
            int steps = Mathf.FloorToInt(Mathf.Max(0f, sourceTotal) / perPoints);
            return steps * rule.amount * stackCount;
        }

        /// <summary>
        /// Total bonus per target stat for every rule, read against <paramref name="totals"/>
        /// (pre-bonus stat totals; a missing stat counts as 0).
        /// </summary>
        public static Dictionary<StatType, float> CalculateBonuses(
            IReadOnlyDictionary<StatType, float> totals,
            IEnumerable<(StatDependencyRule rule, int count)> rules)
        {
            var bonuses = new Dictionary<StatType, float>();
            if (rules == null) return bonuses;

            foreach (var (rule, count) in rules)
            {
                if (rule == null) continue;

                float source = totals != null && totals.TryGetValue(rule.sourceStat, out float value) ? value : 0f;
                float bonus = CalculateRuleBonus(rule, source, count);
                if (bonus == 0f) continue;

                bonuses.TryGetValue(rule.targetStat, out float current);
                bonuses[rule.targetStat] = current + bonus;
            }
            return bonuses;
        }
    }
}
