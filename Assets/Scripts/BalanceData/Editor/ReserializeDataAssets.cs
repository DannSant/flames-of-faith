using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Re-saves the balance data assets with their current classes, so YAML values that no field
    /// reads any more (removed or renamed fields) drop out of the files. Unity keeps those values
    /// until an asset is saved again, and they look editable in a text diff even though nothing uses them.
    /// </summary>
    public static class ReserializeDataAssets
    {
        public static readonly string[] Folders =
        {
            "Assets/Resources/Enemies",
            "Assets/Resources/Waves",
            "Assets/Resources/Behaviors",
        };

        public static readonly string[] Files =
        {
            "Assets/Prefabs/Pickups/HealthPickup.prefab",
        };

        [MenuItem("Tools/Flames of Faith/Balance Data/Reserialize Data Assets")]
        public static void Run()
        {
            var paths = CollectPaths();
            if (!EditorUtility.DisplayDialog("Reserialize data assets",
                    $"Re-saves {paths.Count} assets so values no field reads drop out of the files. Continue?", "Run", "Cancel"))
            {
                return;
            }
            Reserialize(paths);
        }

        /// <summary>Batchmode entry (-executeMethod Game.BalanceData.Editor.ReserializeDataAssets.RunBatch).</summary>
        public static void RunBatch() => Reserialize(CollectPaths());

        private static List<string> CollectPaths()
        {
            return AssetDatabase.FindAssets("", Folders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".asset"))
                .Concat(Files)
                .Distinct()
                .OrderBy(p => p)
                .ToList();
        }

        private static void Reserialize(List<string> paths)
        {
            AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
            Debug.Log($"[ReserializeDataAssets] Re-saved {paths.Count} assets.");
        }
    }
}
