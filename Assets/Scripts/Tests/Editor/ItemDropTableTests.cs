using Game.Effects;
using Game.Loot;
using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The real drop table and loot settings: every lootable Effect (not starting-only) has exactly one row, no class
    /// kit item can drop, and every source has its quality odds. A new item fails here until
    /// Tools/Flames of Faith/Loot/Sync Item Drop Table has run.
    /// </summary>
    public class ItemDropTableTests
    {
        private static ItemDropTable Table => Resources.Load<ItemDropTable>(ItemDropTable.ResourcePath);
        private static LootSettings Settings => Resources.Load<LootSettings>(LootSettings.ResourcePath);

        [Test]
        public void EveryLootableItem_HasExactlyOneRow()
        {
            Assert.IsNotNull(Table, $"Missing Resources/{ItemDropTable.ResourcePath}");
            var rows = Table.Entries.Where(e => e?.item != null).Select(e => e.item).ToList();
            CollectionAssert.AllItemsAreUnique(rows, "An item has two rows");

            foreach (var effect in Resources.LoadAll<Effect>("Effects").Where(e => !e.IsStartingOnly))
            {
                Assert.Contains(effect, rows, $"{effect.name} can't drop anywhere: run Tools/Flames of Faith/Loot/Sync Item Drop Table");
            }
        }

        [Test]
        public void NoRow_IsEmptyOrAStartingOnlyItem()
        {
            foreach (var entry in Table.Entries)
            {
                Assert.IsNotNull(entry?.item, "A row has no item: run Sync Item Drop Table");
                Assert.IsFalse(entry.item.IsStartingOnly, $"{entry.item.name} is a starting-only (class kit) item and can't drop");
            }
        }

        [Test]
        public void EverySource_HasOneEntryWithSomeOdds()
        {
            Assert.IsNotNull(Settings, $"Missing Resources/{LootSettings.ResourcePath}");
            foreach (LootSource source in Enum.GetValues(typeof(LootSource)))
            {
                var entries = Settings.Sources.Where(s => s.source == source).ToList();
                Assert.AreEqual(1, entries.Count, $"{source} needs exactly one entry in LootSettings");
                Assert.Greater(LootRoller.GetTierWeights(entries[0], 0).Sum(), 0f, $"{source} has all quality weights at 0");
            }
        }

        /// <summary>
        /// An empty shop is a broken level. The other sources may hold everything back early on purpose: an enemy
        /// pickup then drops nothing, an event gives nothing, and a treasure gives coins.
        /// </summary>
        [Test]
        public void TheShop_HasStockAtTheStartOfARun()
        {
            var eligible = LootRoller.GetEligible(Table.Entries, LootSource.Shop, 0, e => e.UnlockedByDefault);
            Assert.GreaterOrEqual(eligible.Count, Settings.GetShopItemCount(0),
                "The shop can't fill its 0-Luck slots at 0 levels beaten (check the Shop checkboxes and min levels beaten)");
            Assert.Greater(LootRoller.GetTierOdds(eligible, Settings.GetSource(LootSource.Shop), 0).Sum(), 0f,
                "The shop's quality weights exclude every item it has");
        }
    }
}
