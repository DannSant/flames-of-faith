using Game.Combat;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PierceTests
    {
        private const float Tolerance = 0.0001f;

        // --- Extra targets: 0.25 per point by default, rounded down ---------------------------

        [TestCase(0, 0)]
        [TestCase(-4, 0)]
        [TestCase(1, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 1)]
        [TestCase(7, 1)]
        [TestCase(8, 2)]
        [TestCase(40, 10)]
        public void ExtraPierce_OneEveryFourPoints(int points, int expected)
        {
            Assert.AreEqual(expected, StatsCalculations.CalculateExtraPierce(points));
        }

        [Test]
        public void ExtraPierce_UsesTheGivenRate()
        {
            Assert.AreEqual(1, StatsCalculations.CalculateExtraPierce(2, 0.5f));
            Assert.AreEqual(3, StatsCalculations.CalculateExtraPierce(3, 1f));
            Assert.AreEqual(0, StatsCalculations.CalculateExtraPierce(10, 0f));
        }

        [Test]
        public void ExtraPierce_ExactMultiplesDontRoundDown()
        {
            // 0.1 isn't exact in binary: 10 x 0.1 must still give 1, not 0.
            Assert.AreEqual(1, StatsCalculations.CalculateExtraPierce(10, 0.1f));
            Assert.AreEqual(3, StatsCalculations.CalculateExtraPierce(30, 0.1f));
        }

        // --- Split chance: 3% per point, capped at 40% ----------------------------------------

        [TestCase(0, 0f)]
        [TestCase(-2, 0f)]
        [TestCase(1, 0.03f)]
        [TestCase(10, 0.30f)]
        [TestCase(13, 0.39f)]
        [TestCase(14, 0.40f)]
        [TestCase(100, 0.40f)]
        public void SplitChance_GrowsPerPointUpToTheCap(int points, float expected)
        {
            Assert.AreEqual(expected, StatsCalculations.CalculateSplitChance(points), Tolerance);
        }

        [Test]
        public void SplitChance_CapIsClampedToOne()
        {
            Assert.AreEqual(1f, StatsCalculations.CalculateSplitChance(100, 0.5f, 5f), Tolerance);
        }

        [Test]
        public void Settings_DefaultsMatchTheFormulas()
        {
            var settings = ScriptableObject.CreateInstance<PierceSettings>();
            try
            {
                Assert.AreEqual(1, settings.GetExtraPierce(4));
                Assert.AreEqual(0.12f, settings.GetSplitChance(4), Tolerance);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        // --- The rename keeps old saves loading -----------------------------------------------

        [Test]
        public void StatNames_CurrentAndLegacyNamesParse()
        {
            Assert.IsTrue(StatTypeNames.TryParse("Pierce", out var current));
            Assert.AreEqual(StatType.Pierce, current);

            Assert.IsTrue(StatTypeNames.TryParse("PierceAmount", out var legacy), "saves from before the rename");
            Assert.AreEqual(StatType.Pierce, legacy);

            Assert.IsTrue(StatTypeNames.TryParse("Evasion", out var evasion));
            Assert.AreEqual(StatType.Evasion, evasion);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("NotAStat")]
        [TestCase("11")]
        [TestCase("-1")]
        [TestCase("999")]
        public void StatNames_RejectUnknownNamesAndNumbers(string name)
        {
            Assert.IsFalse(StatTypeNames.TryParse(name, out _));
        }

        [Test]
        public void Pierce_KeepsIndexEleven_SoItemsAndTheDatabaseStillPointToIt()
        {
            Assert.AreEqual(11, (int)StatType.Pierce);
        }
    }
}
