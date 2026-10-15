using Game.Effects;
using Game.Progression;
using NUnit.Framework;
using System.Collections.Generic;

namespace Game.Tests
{
    /// <summary>
    /// The final stat math shared by the real stats and the level-up preview.
    /// </summary>
    public class FinalStatsCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        private static StatModifier Flat(StatType stat, float value) =>
            new StatModifier { stat = stat, value = value, type = ModifierType.Flat };

        private static StatModifier Percent(StatType stat, float value) =>
            new StatModifier { stat = stat, value = value, type = ModifierType.PercentAdd };

        private static StatDependencyRule Rule(StatType source, StatType target, float amount, int perPoints = 1) =>
            new StatDependencyRule { sourceStat = source, targetStat = target, amount = amount, perPoints = perPoints };

        [Test]
        public void FlatThenPercent()
        {
            var result = FinalStatsCalculator.Calculate(
                new Dictionary<StatType, int> { { StatType.Armor, 5 } },
                new[] { Flat(StatType.Armor, 5), Percent(StatType.Armor, 0.2f) },
                null, null);

            Assert.AreEqual(12f, result[StatType.Armor], Tolerance); // (5 + 5) x 1.2
        }

        [Test]
        public void CorruptionIsSubtractedBeforeRules()
        {
            var pre = new Dictionary<StatType, float>();
            var result = FinalStatsCalculator.Calculate(
                new Dictionary<StatType, int> { { StatType.Armor, 10 }, { StatType.MaxHealth, 0 } },
                null,
                stat => stat == StatType.Armor ? 4f : 0f,
                new List<(StatDependencyRule, int)> { (Rule(StatType.Armor, StatType.MaxHealth, 1f), 1) },
                pre);

            Assert.AreEqual(6f, result[StatType.Armor], Tolerance);
            Assert.AreEqual(6f, pre[StatType.Armor], Tolerance);
            Assert.AreEqual(6f, result[StatType.MaxHealth], Tolerance, "the rule reads Armor after corruption");
        }

        [Test]
        public void ASelfRule_DoublesThePickedPoints()
        {
            // The Archer Emblem: every Evasion point gives +1 Evasion, so +2 picked = +4 total
            var rules = new List<(StatDependencyRule, int)> { (Rule(StatType.Evasion, StatType.Evasion, 1f), 1) };
            var before = FinalStatsCalculator.Calculate(new Dictionary<StatType, int> { { StatType.Evasion, 3 } }, null, null, rules);
            var after = FinalStatsCalculator.Calculate(new Dictionary<StatType, int> { { StatType.Evasion, 5 } }, null, null, rules);

            Assert.AreEqual(4f, after[StatType.Evasion] - before[StatType.Evasion], Tolerance);
        }

        [Test]
        public void RulesDontChain()
        {
            var rules = new List<(StatDependencyRule, int)>
            {
                (Rule(StatType.Retribution, StatType.Armor, 1f), 1),
                (Rule(StatType.Armor, StatType.MaxHealth, 1f), 1),
            };
            var result = FinalStatsCalculator.Calculate(
                new Dictionary<StatType, int> { { StatType.Retribution, 3 }, { StatType.Armor, 0 }, { StatType.MaxHealth, 0 } },
                null, null, rules);

            Assert.AreEqual(3f, result[StatType.Armor], Tolerance);
            Assert.AreEqual(0f, result[StatType.MaxHealth], Tolerance, "Armor gained from a rule doesn't feed another rule");
        }

        [Test]
        public void ARuleOnAStatNotInTheBase_IsIgnored()
        {
            var result = FinalStatsCalculator.Calculate(
                new Dictionary<StatType, int> { { StatType.Retribution, 2 } },
                null, null,
                new List<(StatDependencyRule, int)> { (Rule(StatType.Retribution, StatType.Armor, 1f), 1) });

            Assert.IsFalse(result.ContainsKey(StatType.Armor));
        }
    }
}
