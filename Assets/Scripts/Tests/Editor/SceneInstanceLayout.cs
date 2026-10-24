using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Unity keeps a prefab instance root's layout (anchors, pivot, size, position) as overrides that a
    /// revert doesn't remove, so a restyled prefab can silently keep its old spot in a scene. This checks,
    /// from the scene file, that every root layout override equals the prefab's own value.
    /// </summary>
    public static class SceneInstanceLayout
    {
        public static void AssertMatchesPrefab(string scenePath, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing {prefabPath}");
            var rect = (RectTransform)prefab.transform;
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(rect, out string guid, out long rootId));

            var expected = new Dictionary<string, float>
            {
                ["m_AnchorMin.x"] = rect.anchorMin.x, ["m_AnchorMin.y"] = rect.anchorMin.y,
                ["m_AnchorMax.x"] = rect.anchorMax.x, ["m_AnchorMax.y"] = rect.anchorMax.y,
                ["m_Pivot.x"] = rect.pivot.x, ["m_Pivot.y"] = rect.pivot.y,
                ["m_SizeDelta.x"] = rect.sizeDelta.x, ["m_SizeDelta.y"] = rect.sizeDelta.y,
                ["m_AnchoredPosition.x"] = rect.anchoredPosition.x, ["m_AnchoredPosition.y"] = rect.anchoredPosition.y,
            };

            string yaml = File.ReadAllText(scenePath);
            Assert.IsTrue(yaml.Contains(guid), $"No {prefabPath} instance in {scenePath}");
            var overrides = Regex.Matches(yaml,
                $@"target: \{{fileID: {rootId}, guid: {guid}, type: 3\}}\s+propertyPath: (\S+)\s+value: (\S+)");
            foreach (Match match in overrides)
            {
                string property = match.Groups[1].Value;
                if (!expected.TryGetValue(property, out float value)) continue;
                Assert.AreEqual(value, float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), 0.01f,
                    $"{scenePath} overrides {prefab.name} {property} with a value the prefab doesn't have");
            }
        }

        /// <summary>
        /// No layout override (anchors, position, size, pivot) on a child of the prefab's instances in the scene that
        /// can change anything: a removed layout group leaves those behind (one pinned the wave results title to the
        /// bottom). The restylers revert them with UIBuildHelpers.RevertChildLayoutOverrides.
        /// Two kinds are harmless and ignored, because Unity writes them back whenever the scene is saved (it did twice
        /// to MainMenu.unity): a value the parent's layout group drives (it recomputes it on every rebuild), and a value
        /// equal to the prefab's own.
        /// </summary>
        public static void AssertNoChildLayoutOverrides(string scenePath, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab, $"Missing {prefabPath}");
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab.transform, out string guid, out long rootId));

            var rectsById = new Dictionary<string, RectTransform>();
            foreach (var rect in prefab.GetComponentsInChildren<RectTransform>(true))
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(rect, out _, out long id)) rectsById[id.ToString()] = rect;
            }

            string yaml = File.ReadAllText(scenePath);
            var overrides = Regex.Matches(yaml,
                $@"target: \{{fileID: (\d+), guid: {guid}, type: 3\}}\s+propertyPath: (m_AnchorMin|m_AnchorMax|m_AnchoredPosition|m_SizeDelta|m_Pivot)\.(x|y)\s+value: (\S+)");
            foreach (Match match in overrides)
            {
                string id = match.Groups[1].Value, property = match.Groups[2].Value, axis = match.Groups[3].Value;
                if (id == rootId.ToString()) continue;
                if (rectsById.TryGetValue(id, out var rect))
                {
                    if (IsDrivenByLayoutGroup(rect, property, axis)) continue;
                    float value = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    if (Mathf.Abs(value - PrefabValue(rect, property, axis)) < 0.01f) continue;
                }
                Assert.Fail($"{scenePath} overrides {property}.{axis} on {(rect != null ? rect.name : "a child")} of {prefab.name} " +
                    $"(fileID {id}) with a value the prefab doesn't have and no layout group drives");
            }
        }

        /// <summary>
        /// Whether the parent's layout group sets this property of <paramref name="rect"/> on every rebuild: always its
        /// anchors and position, and its size along an axis the group controls (a grid controls both).
        /// </summary>
        public static bool IsDrivenByLayoutGroup(RectTransform rect, string property, string axis)
        {
            if (rect.parent == null) return false;
            var group = rect.parent.GetComponent<LayoutGroup>();
            if (group == null || !group.enabled) return false;
            var element = rect.GetComponent<LayoutElement>();
            if (element != null && element.ignoreLayout) return false;

            switch (property)
            {
                case "m_AnchorMin":
                case "m_AnchorMax":
                case "m_AnchoredPosition":
                    return true;
                case "m_SizeDelta":
                    if (group is GridLayoutGroup) return true;
                    var line = group as HorizontalOrVerticalLayoutGroup;
                    return line != null && (axis == "x" ? line.childControlWidth : line.childControlHeight);
                default:
                    return false;
            }
        }

        private static float PrefabValue(RectTransform rect, string property, string axis)
        {
            Vector2 v = property switch
            {
                "m_AnchorMin" => rect.anchorMin,
                "m_AnchorMax" => rect.anchorMax,
                "m_AnchoredPosition" => rect.anchoredPosition,
                "m_SizeDelta" => rect.sizeDelta,
                _ => rect.pivot,
            };
            return axis == "x" ? v.x : v.y;
        }
    }
}
