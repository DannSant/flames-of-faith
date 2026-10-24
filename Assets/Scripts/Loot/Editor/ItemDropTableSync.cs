using Game.Effects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Loot.Editor
{
    /// <summary>
    /// Keeps <see cref="ItemDropTable"/> in step with the lootable Effects (every Effect that isn't starting-only):
    /// adds a row for a new item (offered in the shop, nothing else, from the start, weight 1) and removes rows for
    /// deleted or starting-only items. Rows are sorted by item name, so the balance CSV stays stable. Run it after
    /// generating the Effect ScriptableObjects; ItemDropTableTests fails until it has run.
    /// </summary>
    public static class ItemDropTableSync
    {
        public const string TableAssetPath = "Assets/Resources/Loot/ItemDropTable.asset";
        public const string SettingsAssetPath = "Assets/Resources/Loot/LootSettings.asset";

        [MenuItem("Tools/Flames of Faith/Loot/Sync Item Drop Table")]
        public static void SyncMenu()
        {
            var (added, removed) = Sync();
            EditorUtility.DisplayDialog("Item drop table", $"Added {added.Count} and removed {removed.Count} rows." +
                (added.Count > 0 ? "\n\nAdded (shop only, from the start): " + string.Join(", ", added) : string.Empty) +
                (removed.Count > 0 ? "\n\nRemoved: " + string.Join(", ", removed) : string.Empty), "OK");
        }

        /// <summary>Batchmode entry (-executeMethod Game.Loot.Editor.ItemDropTableSync.SyncBatch).</summary>
        public static void SyncBatch()
        {
            var (added, removed) = Sync();
            Debug.Log($"[ItemDropTableSync] Added {added.Count} ({string.Join(", ", added)}), removed {removed.Count} ({string.Join(", ", removed)}).");
        }

        /// <summary>The Effects that can drop: every one that isn't a starting-only (class kit) item.</summary>
        public static List<Effect> LootableEffects() =>
            Resources.LoadAll<Effect>("Effects").Where(e => e != null && !e.IsStartingOnly).OrderBy(e => e.name).ToList();

        public static (List<string> added, List<string> removed) Sync()
        {
            var table = LoadOrCreate<ItemDropTable>(TableAssetPath);
            var lootable = LootableEffects();
            var lootableSet = new HashSet<Effect>(lootable);

            Undo.RecordObject(table, "Sync Item Drop Table");
            var removed = table.Entries.Where(e => e == null || e.item == null || !lootableSet.Contains(e.item))
                .Select(e => e?.item != null ? e.item.name : "(missing item)").ToList();
            table.Entries.RemoveAll(e => e == null || e.item == null || !lootableSet.Contains(e.item));

            var present = new HashSet<Effect>(table.Entries.Select(e => e.item));
            var added = new List<string>();
            foreach (var effect in lootable.Where(e => !present.Contains(e)))
            {
                table.Entries.Add(new ItemDropEntry { item = effect, shop = true });
                added.Add(effect.name);
            }

            table.Entries.Sort((a, b) => string.CompareOrdinal(a.item.name, b.item.name));
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            return (added, removed);
        }

        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
