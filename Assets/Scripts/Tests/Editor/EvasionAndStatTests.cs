using System;
using System.Collections.Generic;
using System.Linq;
using Game.Enemies;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    public class EvasionAndStatTests
    {
        private const float Tolerance = 0.0001f;

        // --- Evasion formula (defaults: 0.5% per point to 50, then 0.1%, capped at 30%) ---

        [TestCase(0, 0f)]
        [TestCase(-10, 0f)]
        [TestCase(1, 0.005f)]
        [TestCase(20, 0.10f)]
        [TestCase(50, 0.25f)]
        [TestCase(51, 0.251f)]
        [TestCase(75, 0.275f)]
        [TestCase(100, 0.30f)]
        [TestCase(500, 0.30f)]
        public void EvasionChance_FollowsTheTwoRatesAndTheCap(int points, float expected)
        {
            Assert.AreEqual(expected, StatsCalculations.CalculateEvasionChance(points), Tolerance);
        }

        [Test]
        public void EvasionChance_NeverDecreasesAsPointsGoUp()
        {
            float previous = 0f;
            for (int points = 0; points <= 200; points++)
            {
                float chance = StatsCalculations.CalculateEvasionChance(points);
                Assert.GreaterOrEqual(chance, previous, $"at {points} points");
                previous = chance;
            }
        }

        [Test]
        public void EvasionChance_UsesTheSettingsCurve()
        {
            var settings = ScriptableObject.CreateInstance<EnemyDamageSettings>();
            try
            {
                settings.ConfigureEvasion(softCapPoints: 10, perPointBefore: 0.02f, perPointAfter: 0.01f, maxChance: 0.25f);

                Assert.AreEqual(0.20f, settings.GetEvasionChance(10), Tolerance);
                Assert.AreEqual(0.23f, settings.GetEvasionChance(13), Tolerance);
                Assert.AreEqual(0.25f, settings.GetEvasionChance(40), Tolerance, "capped");
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        // --- Old saves missing a newer stat ---------------------------------------------

        [Test]
        public void Backfill_AddsMissingStats_AndKeepsSavedValues()
        {
            var saved = new Dictionary<StatType, int> { { StatType.MaxHealth, 45 }, { StatType.Armor, 3 } };
            var initial = new[]
            {
                new KeyValuePair<StatType, int>(StatType.MaxHealth, 20),
                new KeyValuePair<StatType, int>(StatType.Armor, 0),
                new KeyValuePair<StatType, int>(StatType.Evasion, 0),
            };

            int added = StatBackfill.FillMissing(saved, initial);

            Assert.AreEqual(1, added);
            Assert.AreEqual(45, saved[StatType.MaxHealth]);
            Assert.AreEqual(3, saved[StatType.Armor]);
            Assert.IsTrue(saved.ContainsKey(StatType.Evasion));
            Assert.AreEqual(0, saved[StatType.Evasion]);
        }

        // --- Guards on the real stat data ---------------------------------------------------

        [Test]
        public void StatDatabase_HasExactlyOneEntryForEveryStatType()
        {
            var database = Resources.Load<StatDatabase>("Progression/StatDatabase");
            Assert.IsNotNull(database, "Missing Resources/Progression/StatDatabase.asset");

            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                int count = database.Stats.Count(s => s.StatType == stat);
                Assert.AreEqual(1, count, $"{stat} must have exactly one StatDatabase entry, or it stays at 0 in game");
            }
        }

        [Test]
        public void Evasion_IsAppendedAtTheEnd_SoStoredStatIndicesDontShift()
        {
            // Effect assets and the effects DB store stats by index; 21 is the last one that existed before Evasion.
            Assert.AreEqual(21, (int)StatType.Retribution);
            Assert.AreEqual(22, (int)StatType.Evasion);
        }
    }
}
