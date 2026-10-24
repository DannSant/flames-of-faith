using Game.Effects;
using Game.Progression;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Loot
{
    /// <summary>
    /// The one way gameplay gets a random item: the enemy pickup, the shop, treasures and events all roll here, from
    /// <see cref="ItemDropTable"/> and <see cref="LootSettings"/>, with the player's levels beaten and the unlocked
    /// items. The math is <see cref="LootRoller"/>. Never throws: drops can happen inside a level load, where an
    /// exception hangs the loading screen.
    /// </summary>
    public static class ItemProvider
    {
        /// <summary>One item from <paramref name="source"/> at <paramref name="luck"/>, or null when nothing can drop yet.</summary>
        public static Effect RollOne(LootSource source, int luck)
        {
            var items = Roll(source, luck, 1, false);
            return items.Count > 0 ? items[0] : null;
        }

        /// <summary>Up to <paramref name="count"/> items; unique ones never repeat (the shop).</summary>
        public static List<Effect> Roll(LootSource source, int luck, int count, bool unique)
        {
            try
            {
                var eligible = GetEligible(source);
                var settings = LootSettings.Instance.GetSource(source);
                var items = LootRoller.Roll(eligible, settings, luck, count, unique, () => Random.value);
                if (items.Count == 0 && count > 0)
                {
                    Debug.Log($"[ItemProvider] No item can drop from {source} yet (levels beaten {LevelsBeaten}).");
                }
                return items;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return new List<Effect>();
            }
        }

        /// <summary>The quality an item from <paramref name="source"/> typically has at <paramref name="luck"/>.</summary>
        public static EffectQuality GetExpectedQuality(LootSource source, int luck) =>
            LootRoller.GetExpectedQuality(GetEligible(source), LootSettings.Instance.GetSource(source), luck);

        /// <summary>The player's total of <paramref name="stat"/> (Luck by default), or 0 without a player.</summary>
        public static int GetPlayerStat(StatType stat = StatType.Luck)
        {
            var progression = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerComponent<PlayerProgression>() : null;
            return progression != null ? progression.GetStatTotal(stat) : 0;
        }

        private static int LevelsBeaten => GameSession.Instance != null ? GameSession.Instance.LevelsBeaten : 0;

        private static List<ItemDropEntry> GetEligible(LootSource source)
        {
            // Unlocked = in the provider's available list (unlocked by default, or by meta-progression).
            // Without the provider (tests, a scene run on its own), the default unlock state.
            var available = EffectsDatabaseProvider.Instance != null ? EffectsDatabaseProvider.Instance.GetAvailableEffects() : null;
            var unlocked = available != null ? new HashSet<Effect>(available) : null;
            return LootRoller.GetEligible(ItemDropTable.Instance.Entries, source, LevelsBeaten,
                item => unlocked != null ? unlocked.Contains(item) : item.UnlockedByDefault);
        }
    }
}
