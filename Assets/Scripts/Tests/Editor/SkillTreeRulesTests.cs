using Game.Metaprogression;
using NUnit.Framework;

namespace Game.Tests
{
    public class SkillTreeRulesTests
    {
        private static readonly int[] Costs = { 3, 6, 10 };
        private const int TierCount = 4;

        [TestCase(0, 0)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(7, 3)]
        [TestCase(-1, 0)]
        public void ClampTier_StaysInTheNode(int tier, int expected)
        {
            Assert.AreEqual(expected, SkillTreeRules.ClampTier(tier, TierCount));
        }

        [TestCase(0, 3)]
        [TestCase(1, 6)]
        [TestCase(2, 10)]
        [TestCase(3, -1)]
        [TestCase(9, -1)]
        public void UpgradeCost_IsTheNextTiersCost(int currentTier, int expected)
        {
            Assert.AreEqual(expected, SkillTreeRules.GetUpgradeCost(Costs, TierCount, currentTier));
        }

        [Test]
        public void UpgradeCost_MissingEntryIsFree()
        {
            Assert.AreEqual(0, SkillTreeRules.GetUpgradeCost(new[] { 3 }, TierCount, 2));
            Assert.AreEqual(0, SkillTreeRules.GetUpgradeCost(null, TierCount, 0));
        }

        [Test]
        public void UpgradeCost_SingleTierNodeCantUpgrade()
        {
            Assert.AreEqual(-1, SkillTreeRules.GetUpgradeCost(Costs, 1, 0));
        }

        [TestCase(0, 3, true)]
        [TestCase(0, 2, false)]
        [TestCase(2, 10, true)]
        [TestCase(2, 9, false)]
        [TestCase(3, 100, false)]
        public void CanUpgrade_NeedsEnoughFavorsAndANextTier(int currentTier, int favors, bool expected)
        {
            Assert.AreEqual(expected, SkillTreeRules.CanUpgrade(Costs, TierCount, currentTier, favors));
        }
    }
}
