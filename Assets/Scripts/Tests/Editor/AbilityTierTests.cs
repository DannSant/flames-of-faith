using Game.Effects;
using Game.Effects.Abilities;
using Game.Progression;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tests
{
    public class AbilityTierTests
    {
        private class Row
        {
            public int value;
            public Row(int value) { this.value = value; }
        }

        // --- Tier table lookup -------------------------------------------------------------

        [TestCase(0, 3, 0)]
        [TestCase(2, 3, 2)]
        [TestCase(5, 3, 2)]
        [TestCase(-1, 3, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(3, 0, -1)]
        public void ClampIndex_StaysInsideTheTable(int tier, int count, int expected)
        {
            Assert.AreEqual(expected, AbilityTiers.ClampIndex(tier, count));
        }

        [Test]
        public void Get_TierPastTheTable_UsesTheLastRow()
        {
            var rows = new List<Row> { new Row(10), new Row(11), new Row(12) };
            Assert.AreEqual(10, AbilityTiers.Get(rows, 0).value);
            Assert.AreEqual(12, AbilityTiers.Get(rows, 2).value);
            Assert.AreEqual(12, AbilityTiers.Get(rows, 7).value);
        }

        [Test]
        public void Get_EmptyOrMissingTable_IsNull()
        {
            Assert.IsNull(AbilityTiers.Get(new List<Row>(), 0));
            Assert.IsNull(AbilityTiers.Get<Row>(null, 0));
        }

        // --- Starting-only items -----------------------------------------------------------

        [Test]
        public void IsStartingOnly_ByFlag()
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            Assert.IsFalse(effect.IsStartingOnly);

            effect.StartingOnly = true;
            Assert.IsTrue(effect.IsStartingOnly);
            Object.DestroyImmediate(effect);
        }

        [Test]
        public void IsStartingOnly_ItemsWithStatBonusesCountToo()
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            effect.StatDependencies = new List<StatDependencyRule>
            {
                new StatDependencyRule { sourceStat = StatType.Retribution, targetStat = StatType.Armor, perPoints = 1, amount = 1 }
            };
            Assert.IsTrue(effect.IsStartingOnly);
            Object.DestroyImmediate(effect);
        }

        [Test]
        public void Tier_IsNeverNegative()
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            effect.Tier = -2;
            Assert.AreEqual(0, effect.Tier);
            Object.DestroyImmediate(effect);
        }
    }
}
