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
            "Assets/Resources/Combat/DamageSources",
            "Assets/Resources/Debuffs",
            "Assets/Resources/CharacterData",
        };

        // Prefabs whose design numbers moved to an asset (damage sources -> DamageSourceData).
        public static readonly string[] PrefabFolders =
        {
            "Assets/Prefabs/Player/Projectiles",
            "Assets/Prefabs/Player/Other",
        };

        public static readonly string[] Files =
        {
            "Assets/Prefabs/Pickups/HealthPickup.prefab",
            // Grace, movement, invulnerability, Retribution and dash numbers moved to settings assets (step 3)
            "Assets/Prefabs/Player/PlayerBase.prefab",
            "Assets/Prefabs/Player/PlayerArcher.prefab",
            "Assets/Prefabs/Player/PlayerAugur.prefab",
            "Assets/Prefabs/Player/PlayerWarrior.prefab",
            "Assets/Resources/Combat/CorruptionSettings.asset",
            "Assets/Resources/Progression/PlayerStatSettings.asset",
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
                .Concat(AssetDatabase.FindAssets("t:Prefab", PrefabFolders).Select(AssetDatabase.GUIDToAssetPath))
                .Concat(Files)
                .Distinct()
                .OrderBy(p => p)
                .ToList();
        }

        private static void Reserialize(List<string> paths)
        {
            // ForceReserializeAssets drops removed fields from .asset files but keeps them in prefabs,
            // so prefabs are loaded and saved again instead.
            var prefabs = paths.Where(p => p.EndsWith(".prefab")).ToList();
            AssetDatabase.ForceReserializeAssets(paths.Except(prefabs).ToList(), ForceReserializeAssetsOptions.ReserializeAssets);
            foreach (string path in prefabs)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            Debug.Log($"[ReserializeDataAssets] Re-saved {paths.Count} assets ({prefabs.Count} prefabs).");
        }
    }
}
