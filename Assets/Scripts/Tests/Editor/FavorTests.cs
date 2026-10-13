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
            { "Warrior", 10 },
            { "Archer", 4 },
            { "Mage", 0 },
        };

        [Test]
        public void Spendable_OwnPoolOnly_WhenNotShared()
        {
            var pools = Pools();
            Assert.AreEqual(10, FavorRules.CalculateSpendable(pools, "Warrior", false));
            Assert.AreEqual(0, FavorRules.CalculateSpendable(pools, "Mage", false));
            Assert.AreEqual(0, FavorRules.CalculateSpendable(pools, "Rogue", false));
        }

        [Test]
        public void Spendable_AllPools_WhenShared()
        {
            Assert.AreEqual(14, FavorRules.CalculateSpendable(Pools(), "Mage", true));
        }

        [Test]
        public void Spend_NotShared_TakesFromOwnPool()
        {
            var pools = Pools();
            Assert.IsTrue(FavorRules.TrySpend(pools, "Warrior", 7, false));
            Assert.AreEqual(3, pools["Warrior"]);
            Assert.AreEqual(4, pools["Archer"]);
        }

        [Test]
        public void Spend_NotShared_CantUseOtherPools()
        {
            var pools = Pools();
            Assert.IsFalse(FavorRules.TrySpend(pools, "Archer", 5, false));
            Assert.AreEqual(4, pools["Archer"], "a failed spend changes nothing");
            Assert.AreEqual(10, pools["Warrior"]);
        }

        [Test]
        public void Spend_Shared_TakesOwnPoolFirstThenOthers()
        {
            var pools = Pools();
            Assert.IsTrue(FavorRules.TrySpend(pools, "Archer", 6, true));
            Assert.AreEqual(0, pools["Archer"]);
            Assert.AreEqual(8, pools["Warrior"]);
        }

        [Test]
        public void Spend_Shared_FailsWhenTotalIsTooLow()
        {
            var pools = Pools();
            Assert.IsFalse(FavorRules.TrySpend(pools, "Mage", 15, true));
            CollectionAssert.AreEquivalent(Pools(), pools);
        }

        [Test]
        public void Spend_ZeroCost_AlwaysSucceeds()
        {
            Assert.IsTrue(FavorRules.TrySpend(Pools(), "Mage", 0, false));
        }
    }
}
