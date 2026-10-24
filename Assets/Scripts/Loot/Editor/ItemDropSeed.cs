using Game.Pickups;
using Game.RunEncounters;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Loot.Editor
{
    /// <summary>
    /// One-time migration (Oct 2026): fills the drop table and the loot settings from the hand-written lists they
    /// replace (RandomItemPickup, the treasure tables, the event actions, Effect.availableForShop).
    /// Deleted once the old fields are removed.
    /// </summary>
    public static class ItemDropSeed
    {
        /// <summary>Batchmode entry (-executeMethod Game.Loot.Editor.ItemDropSeed.RunBatch).</summary>
        public static void RunBatch()
        {
            ItemDropTableSync.Sync();
            var table = AssetDatabase.LoadAssetAtPath<ItemDropTable>(ItemDropTableSync.TableAssetPath);

            var pickupItems = new HashSet<Object>();
            var pickupPrefab = AssetDatabase.LoadAssetAtPath<RandomItemPickup>("Assets/Prefabs/Pickups/RandomItemPickup.prefab");
            var list = new SerializedObject(pickupPrefab).FindProperty("possibleItems");
            for (int i = 0; i < list.arraySize; i++)
            {
                pickupItems.Add(list.GetArrayElementAtIndex(i).FindPropertyRelative("itemEffect").objectReferenceValue);
            }

            var treasureItems = AssetDatabase.FindAssets("t:TreasureEncounterData")
                .Select(g => AssetDatabase.LoadAssetAtPath<TreasureEncounterData>(AssetDatabase.GUIDToAssetPath(g)))
                .SelectMany(d => d.rewardTable).Where(r => r.item != null).Select(r => (Object)r.item).ToHashSet();

            var eventItems = AssetDatabase.FindAssets("t:GiveRandomItemOptionAction")
                .Select(g => AssetDatabase.LoadAssetAtPath<GiveRandomItemOptionAction>(AssetDatabase.GUIDToAssetPath(g)))
                .SelectMany(a => a.PossibleItems).Where(e => e.item != null).Select(e => (Object)e.item).ToHashSet();

            foreach (var entry in table.Entries)
            {
                entry.shop = entry.item.AvailableForShop;
                entry.enemyPickup = pickupItems.Contains(entry.item);
                entry.treasure = treasureItems.Contains(entry.item);
                entry.events = eventItems.Contains(entry.item);
                entry.minLevelsBeaten = 0;
                entry.weight = 1f;
            }
            EditorUtility.SetDirty(table);

            // Today's odds at Luck 0 for today's pools: the pickup's per-item weights summed per quality, the treasure's
            // uniform Rare/Heroic list, and the equal tiers of the shop and events
            var settings = ItemDropTableSync.LoadOrCreate<LootSettings>(ItemDropTableSync.SettingsAssetPath);
            settings.Sources.Clear();
            settings.Sources.Add(Source(LootSource.EnemyPickup, 35, 25, 20, 20));
            settings.Sources.Add(Source(LootSource.Shop, 25, 25, 25, 25));
            settings.Sources.Add(Source(LootSource.Treasure, 0, 0, 40, 60));
            settings.Sources.Add(Source(LootSource.Event, 25, 25, 25, 25));
            EditorUtility.SetDirty(settings);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ItemDropSeed] {table.Entries.Count} rows: {table.Entries.Count(e => e.shop)} shop, " +
                $"{table.Entries.Count(e => e.enemyPickup)} enemy pickup, {table.Entries.Count(e => e.treasure)} treasure, {table.Entries.Count(e => e.events)} event.");
        }

        private static LootSourceSettings Source(LootSource source, float common, float uncommon, float rare, float heroic) => new()
        {
            source = source, commonWeight = common, uncommonWeight = uncommon, rareWeight = rare, heroicWeight = heroic,
            epicWeight = 0f, legendaryWeight = 0f, luckScalePerStep = 2f,
        };
    }
}
