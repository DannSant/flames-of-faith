using System.Collections.Generic;

namespace Game.Metaprogression
{
    /// <summary>
    /// Pure skill tree rules. Each node is a linear path: tier N can only be bought from tier N-1,
    /// one tier at a time.
    /// </summary>
    public static class SkillTreeRules
    {
        /// <summary>A saved tier clamped into the node's range (a node that lost tiers keeps its top one).</summary>
        public static int ClampTier(int tier, int tierCount)
        {
            if (tierCount <= 0) return 0;
            if (tier < 0) return 0;
            return tier >= tierCount ? tierCount - 1 : tier;
        }

        /// <summary>
        /// Favors to go from <paramref name="currentTier"/> to the next one, or -1 when the node is
        /// already at its top tier. A missing cost entry counts as 0.
        /// </summary>
        public static int GetUpgradeCost(IReadOnlyList<int> upgradeCosts, int tierCount, int currentTier)
        {
            int tier = ClampTier(currentTier, tierCount);
            if (tier >= tierCount - 1) return -1;
            if (upgradeCosts == null || tier >= upgradeCosts.Count) return 0;
            return upgradeCosts[tier] < 0 ? 0 : upgradeCosts[tier];
        }

        public static bool CanUpgrade(IReadOnlyList<int> upgradeCosts, int tierCount, int currentTier, int spendableFavors)
        {
            int cost = GetUpgradeCost(upgradeCosts, tierCount, currentTier);
            return cost >= 0 && spendableFavors >= cost;
        }
    }
}
