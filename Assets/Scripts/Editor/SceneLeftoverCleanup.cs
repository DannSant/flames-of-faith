using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// ONE-SHOT cleanup (Oct 2026), delete this file after running it once.
    /// Removes leftovers from every scene under Assets/Scenes, then saves the scenes it changed:
    /// - instances of the old test enemy prefabs (Prefabs/Enemies/old) and of the obsolete NodeTransitioner;
    /// - "Enemies" containers left empty by that;
    /// - NavMeshPlus NavMeshModifier components (no NavMesh is used) and the legacy MapUtils component.
    /// Also strips the NavMeshModifier from the Oak_Tree prefab.
    /// </summary>
    public static class SceneLeftoverCleanup
    {
        private static readonly string[] PrefabsToRemove =
        {
            "Assets/Prefabs/Enemies/old/FlameShooter.prefab",
            "Assets/Prefabs/Enemies/old/LightDevourer.prefab",
            "Assets/Prefabs/Enemies/old/SlimeChaser.prefab",
            "Assets/Prefabs/Enemies/old/SlimeRoamer.prefab",
            "Assets/Prefabs/Level/NodeTransitioner.prefab",
        };

        private static readonly string[] ComponentTypesToRemove =
        {
            "NavMeshPlus.Components.NavMeshModifier",
            "Game.Map.MapUtils",
        };

        private const string OakTreePrefab = "Assets/Prefabs/Oak_Tree.prefab";
        private const string EnemiesContainerName = "Enemies";

        [MenuItem("Tools/Flames of Faith/One-shot/Remove Scene Leftovers")]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Remove scene leftovers",
                    "Opens every scene under Assets/Scenes, removes the old test enemies, NodeTransitioners, " +
                    "NavMeshModifiers and MapUtils, and saves the scenes it changed. Continue?", "Run", "Cancel"))
            {
                return;
            }

            var originalSetup = EditorSceneManager.GetSceneManagerSetup();
            var prefabPaths = new HashSet<string>(PrefabsToRemove);
            var log = new List<string>();

            string[] scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .ToArray();

            try
            {
                for (int i = 0; i < scenePaths.Length; i++)
                {
                    string path = scenePaths[i];
                    EditorUtility.DisplayProgressBar("Remove scene leftovers", path, (float)i / scenePaths.Length);
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    int removed = CleanScene(scene, prefabPaths, log, path);
                    if (removed > 0)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        log.Add($"{path}: removed {removed}");
                    }
                }
                CleanOakTreePrefab(log);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (originalSetup != null && originalSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }

            Debug.Log("[SceneLeftoverCleanup] Done.\n" + (log.Count > 0 ? string.Join("\n", log) : "Nothing to remove."));
        }

        private static int CleanScene(UnityEngine.SceneManagement.Scene scene, HashSet<string> prefabPaths,
            List<string> log, string scenePath)
        {
            int removed = 0;
            var all = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToList();

            // Prefab instances first (whole objects), then loose components on what's left.
            foreach (var go in all)
            {
                if (go == null || !PrefabUtility.IsOutermostPrefabInstanceRoot(go)) continue;
                string source = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go));
                if (!prefabPaths.Contains(source)) continue;

                if (go.transform.parent != null && PrefabUtility.IsPartOfPrefabInstance(go.transform.parent))
                {
                    log.Add($"{scenePath}: SKIPPED '{go.name}' ({source}), it sits inside another prefab instance");
                    continue;
                }
                Object.DestroyImmediate(go);
                removed++;
            }

            foreach (var go in all)
            {
                if (go == null) continue;
                foreach (var component in go.GetComponents<Component>())
                {
                    if (component == null || !ComponentTypesToRemove.Contains(component.GetType().FullName)) continue;
                    if (PrefabUtility.IsPartOfPrefabInstance(component) && !PrefabUtility.IsAddedComponentOverride(component))
                    {
                        // Comes from a prefab (e.g. Oak_Tree): removed in the prefab itself.
                        continue;
                    }
                    Object.DestroyImmediate(component);
                    removed++;
                }
            }

            foreach (var go in all)
            {
                if (go == null || go.name != EnemiesContainerName) continue;
                if (go.transform.childCount > 0 || go.GetComponents<Component>().Length > 1) continue;
                if (PrefabUtility.IsPartOfPrefabInstance(go)) continue;
                Object.DestroyImmediate(go);
                removed++;
            }
            return removed;
        }

        private static void CleanOakTreePrefab(List<string> log)
        {
            var root = PrefabUtility.LoadPrefabContents(OakTreePrefab);
            try
            {
                int removed = 0;
                foreach (var component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component == null || !ComponentTypesToRemove.Contains(component.GetType().FullName)) continue;
                    Object.DestroyImmediate(component);
                    removed++;
                }
                if (removed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, OakTreePrefab);
                    log.Add($"{OakTreePrefab}: removed {removed}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
