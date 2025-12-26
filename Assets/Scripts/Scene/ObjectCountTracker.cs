using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scene
{

    public static class ObjectCountTracker
    {
        // ObjectName -> List of counts per level
        private static readonly Dictionary<string, List<int>> history = new();

        private static int levelIndex = 0;

        public static void Snapshot(string levelName)
        {
            levelIndex++;

            var objects = Resources.FindObjectsOfTypeAll<GameObject>();

            // Count by name
            Dictionary<string, int> currentCounts = new();

            foreach (var go in objects)
            {
                if (ShouldIgnore(go))
                    continue;

                if (!currentCounts.TryAdd(go.name, 1))
                    currentCounts[go.name]++;
            }

            // Merge into history
            foreach (var kvp in currentCounts)
            {
                if (!history.ContainsKey(kvp.Key))
                {
                    // Pad missing previous levels with 0
                    history[kvp.Key] = Enumerable.Repeat(0, levelIndex - 1).ToList();
                }

                history[kvp.Key].Add(kvp.Value);
            }

            // Also pad objects that did NOT appear this level
            foreach (var list in history.Values)
            {
                if (list.Count < levelIndex)
                    list.Add(0);
            }

            Debug.Log($"[ObjectCountTracker] Snapshot taken for level {levelIndex} ({levelName})");
        }

        public static void PrintReport(int minGrowth = 5)
        {
            Debug.Log("===== OBJECT COUNT REPORT =====");

            foreach (var kvp in history.OrderByDescending(h => h.Value.Last()))
            {
                var counts = kvp.Value;
                int growth = counts.Last() - counts.First();

                if (growth < minGrowth)
                    continue;

                string line = $"{kvp.Key,-30} | " + string.Join(" | ", counts);
                Debug.Log(line);
            }

            Debug.Log("===== END REPORT =====");
        }

        private static bool ShouldIgnore(GameObject go)
        {
            // Filter editor-only and internal Unity objects
            if (go.hideFlags != HideFlags.None)
                return true;

            // Optional noise filters
            if (go.name.StartsWith("Editor") ||
                go.name.StartsWith("SceneCamera") ||
                go.name.StartsWith("Canvas") && go.scene.name == null)
                return true;

            return false;
        }
    }
}

