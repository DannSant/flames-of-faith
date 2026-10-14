using Game.Overworld;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>Guards the overworld map assets the run save relies on.</summary>
    public class MapDefinitionTests
    {
        private static List<MapDefinition> LoadMaps() =>
            AssetDatabase.FindAssets("t:MapDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MapDefinition>)
                .Where(m => m != null)
                .ToList();

        // A run save stores the act's mapId and Continue looks the map up by it. Two maps sharing
        // an id restore the wrong one, and the save gets deleted as "doesn't match this build"
        // (Act1MapC was a copy of Act1MapB that kept its id).
        [Test]
        public void MapIds_AreUnique()
        {
            var maps = LoadMaps();
            Assert.IsNotEmpty(maps, "No MapDefinition assets found");

            var seen = new Dictionary<string, string>();
            foreach (var map in maps)
            {
                Assert.IsFalse(string.IsNullOrEmpty(map.mapId), $"{map.name} has no mapId");
                Assert.IsFalse(seen.TryGetValue(map.mapId, out var other),
                    $"{map.name} and {other} share the mapId '{map.mapId}'");
                seen[map.mapId] = map.name;
            }
        }
    }
}
