using Game.Metaprogression;
using NUnit.Framework;
using System.Collections.Generic;

namespace Game.Tests
{
    public class FavorTests
    {
        // --- Wave Favors -------------------------------------------------------------------

        [TestCase(5f, 1, 1)]
        [TestCase(0.5f, 1, 1)]
        [TestCase(0f, 1, 0)]
        [TestCase(-3f, 1, 0)]
        [TestCase(5f, 2, 2)]
        [TestCase(5f, 0, 0)]
        [TestCase(5f, -1, 0)]
        public void WaveFavors_OnlyWithGraceAboveZero(float graceAfter, int perWave, int expected)
        {
            Assert.AreEqual(expected, FavorRules.CalculateWaveFavors(graceAfter, perWave));
        }

        // --- Spendable / spending ----------------------------------------------------------

        private static Dictionary<string, int> Pools() => new()
        {
            { "Crusader", 10 },
            { "Sentinel", 4 },
            { "Augur", 0 },
        };

        [Test]
        public void Spendable_OwnPoolOnly_WhenNotShared()
        {
            var pools = Pools();
            Assert.AreEqual(10, FavorRules.CalculateSpendable(pools, "Crusader", false));
            Assert.AreEqual(0, FavorRules.CalculateSpendable(pools, "Augur", false));
            Assert.AreEqual(0, FavorRules.CalculateSpendable(pools, "Rogue", false));
        }

        [Test]
        public void Spendable_AllPools_WhenShared()
        {
            Assert.AreEqual(14, FavorRules.CalculateSpendable(Pools(), "Augur", true));
        }

        [Test]
        public void Spend_NotShared_TakesFromOwnPool()
        {
            var pools = Pools();
            Assert.IsTrue(FavorRules.TrySpend(pools, "Crusader", 7, false));
            Assert.AreEqual(3, pools["Crusader"]);
            Assert.AreEqual(4, pools["Sentinel"]);
        }

        [Test]
        public void Spend_NotShared_CantUseOtherPools()
        {
            var pools = Pools();
            Assert.IsFalse(FavorRules.TrySpend(pools, "Sentinel", 5, false));
            Assert.AreEqual(4, pools["Sentinel"], "a failed spend changes nothing");
            Assert.AreEqual(10, pools["Crusader"]);
        }

        [Test]
        public void Spend_Shared_TakesOwnPoolFirstThenOthers()
        {
            var pools = Pools();
            Assert.IsTrue(FavorRules.TrySpend(pools, "Sentinel", 6, true));
            Assert.AreEqual(0, pools["Sentinel"]);
            Assert.AreEqual(8, pools["Crusader"]);
        }

        [Test]
        public void Spend_Shared_FailsWhenTotalIsTooLow()
        {
            var pools = Pools();
            Assert.IsFalse(FavorRules.TrySpend(pools, "Augur", 15, true));
            CollectionAssert.AreEquivalent(Pools(), pools);
        }

        [Test]
        public void Spend_ZeroCost_AlwaysSucceeds()
        {
            Assert.IsTrue(FavorRules.TrySpend(Pools(), "Augur", 0, false));
        }
    }
}
