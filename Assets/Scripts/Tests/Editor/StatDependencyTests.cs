using System.Collections.Generic;
using System.Linq;
using Game.Effects;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class StatDependencyTests
    {
        private const float Tolerance = 0.0001f;

        private static StatDependencyRule Rule(StatType source, int perPoints, StatType target, float amount)
        {
            return new StatDependencyRule { sourceStat = source, perPoints = perPoints, targetStat = target, amount = amount };
        }

        private static Dictionary<StatType, float> Totals(params (StatType stat, float value)[] values)
        {
            return values.ToDictionary(v => v.stat, v => v.value);
        }

        // --- One rule -------------------------------------------------------------------------

        [TestCase(0f, 0f)]
        [TestCase(1f, 1f)]
        [TestCase(7f, 7f)]
        public void EveryPoint_GivesTheAmountPerPoint(float source, float expected)
        {
            var rule = Rule(StatType.Retribution, 1, StatType.Armor, 1f);
            Assert.AreEqual(expected, StatDependencyCalculator.CalculateRuleBonus(rule, source), Tolerance);
        }

        [TestCase(4f, 0f)]
        [TestCase(5f, -1f)]
        [TestCase(9f, -1f)]
        [TestCase(10f, -2f)]
        public void EveryNPoints_CountsOnlyCompleteSteps(float source, float expected)
        {
            var rule = Rule(StatType.Evasion, 5, StatType.AttackSpeed, -1f);
            Assert.AreEqual(expected, StatDependencyCalculator.CalculateRuleBonus(rule, source), Tolerance);
        }

        [Test]
        public void NegativeOrZeroSource_GivesNothing()
        {
            var rule = Rule(StatType.MastowAffinity, 1, StatType.SkillDuration, 1f);
            Assert.AreEqual(0f, StatDependencyCalculator.CalculateRuleBonus(rule, -3f), Tolerance);
            Assert.AreEqual(0f, StatDependencyCalculator.CalculateRuleBonus(rule, 0f), Tolerance);
        }

        [Test]
        public void FractionalSource_IsFloored()
        {
            var rule = Rule(StatType.Evasion, 1, StatType.Evasion, 1f);
            Assert.AreEqual(3f, StatDependencyCalculator.CalculateRuleBonus(rule, 3.9f), Tolerance);
        }

        [Test]
        public void StackCount_MultipliesTheBonus()
        {
            var rule = Rule(StatType.Retribution, 1, StatType.MaxHealth, 1f);
            Assert.AreEqual(8f, StatDependencyCalculator.CalculateRuleBonus(rule, 4f, stackCount: 2), Tolerance);
        }

        [Test]
        public void PerPointsBelowOne_IsTreatedAsOne()
        {
            var rule = Rule(StatType.Retribution, 0, StatType.Armor, 1f);
            Assert.AreEqual(3f, StatDependencyCalculator.CalculateRuleBonus(rule, 3f), Tolerance);
        }

        // --- Several rules ----------------------------------------------------------------------

        [Test]
        public void WarriorStyleRules_GiveEveryTarget()
        {
            var rules = new[]
            {
                (Rule(StatType.Retribution, 1, StatType.Armor, 1f), 1),
                (Rule(StatType.Retribution, 1, StatType.MaxHealth, 1f), 1),
                (Rule(StatType.Retribution, 1, StatType.MoveSpeed, -1f), 1),
            };

            var bonuses = StatDependencyCalculator.CalculateBonuses(Totals((StatType.Retribution, 6f)), rules);

            Assert.AreEqual(6f, bonuses[StatType.Armor], Tolerance);
            Assert.AreEqual(6f, bonuses[StatType.MaxHealth], Tolerance);
            Assert.AreEqual(-6f, bonuses[StatType.MoveSpeed], Tolerance);
        }

        [Test]
        public void SeveralRulesOnOneTarget_AddUp()
        {
            var rules = new[]
            {
                (Rule(StatType.Retribution, 1, StatType.Armor, 1f), 1),
                (Rule(StatType.Evasion, 2, StatType.Armor, 1f), 1),
            };

            var bonuses = StatDependencyCalculator.CalculateBonuses(
                Totals((StatType.Retribution, 3f), (StatType.Evasion, 4f)), rules);

            Assert.AreEqual(5f, bonuses[StatType.Armor], Tolerance);
        }

        [Test]
        public void SelfReference_DoublesWithoutLooping()
        {
            var rules = new[] { (Rule(StatType.Evasion, 1, StatType.Evasion, 1f), 1) };

            var bonuses = StatDependencyCalculator.CalculateBonuses(Totals((StatType.Evasion, 20f)), rules);

            Assert.AreEqual(20f, bonuses[StatType.Evasion], Tolerance, "20 Evasion + 20 bonus = 40, not more");
        }

        [Test]
        public void RulesDontChain()
        {
            // Retribution -> Armor, Armor -> MaxHealth: the second reads the pre-bonus Armor (0).
            var rules = new[]
            {
                (Rule(StatType.Retribution, 1, StatType.Armor, 1f), 1),
                (Rule(StatType.Armor, 1, StatType.MaxHealth, 10f), 1),
            };

            var bonuses = StatDependencyCalculator.CalculateBonuses(Totals((StatType.Retribution, 5f)), rules);

            Assert.AreEqual(5f, bonuses[StatType.Armor], Tolerance);
            Assert.IsFalse(bonuses.ContainsKey(StatType.MaxHealth));
        }

        [Test]
        public void MissingSourceStat_CountsAsZero()
        {
            var rules = new[] { (Rule(StatType.Retribution, 1, StatType.Armor, 1f), 1) };

            var bonuses = StatDependencyCalculator.CalculateBonuses(new Dictionary<StatType, float>(), rules);

            Assert.IsEmpty(bonuses);
        }

        // --- Data -------------------------------------------------------------------------------

        [Test]
        public void Rules_SurviveTheDatabaseJsonRoundTrip()
        {
            var rules = new List<StatDependencyRule>
            {
                Rule(StatType.Evasion, 5, StatType.AttackSpeed, -1f),
                Rule(StatType.Retribution, 1, StatType.Armor, 1.5f),
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(rules);
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StatDependencyRule>>(json);

            Assert.AreEqual(2, back.Count);
            Assert.AreEqual(StatType.Evasion, back[0].sourceStat);
            Assert.AreEqual(5, back[0].perPoints);
            Assert.AreEqual(StatType.AttackSpeed, back[0].targetStat);
            Assert.AreEqual(-1f, back[0].amount, Tolerance);
            Assert.AreEqual(1.5f, back[1].amount, Tolerance);
        }

        // --- Guards: bonus items are starting items only ----------------------------------------

        // Items with rules never drop: ItemDropTableTests.NoRow_IsEmptyOrAStartingOnlyItem guards the drop table,
        // which replaced the hand-written treasure, event and pickup lists checked here before.
    }
}
