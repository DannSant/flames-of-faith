using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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
    }
}
