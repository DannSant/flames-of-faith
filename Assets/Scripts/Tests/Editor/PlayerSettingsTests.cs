using Game.Combat;
using Game.Control;
using Game.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Numbers that moved off the player prefabs (data-driven step 3): Grace onto CorruptionSettings, and each class's
    /// dash onto its CharacterClassData, which the dash finds through the PlayerClassManager.
    /// </summary>
    public class PlayerSettingsTests
    {
        [TestCase("Assets/Prefabs/Player/PlayerWarrior.prefab")]
        [TestCase("Assets/Prefabs/Player/PlayerArcher.prefab")]
        [TestCase("Assets/Prefabs/Player/PlayerAugur.prefab")]
        public void EveryClassDash_FindsItsClassData(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, path);
            var dash = prefab.GetComponentInChildren<DashBase>(true);
            Assert.IsNotNull(dash, $"{path} has no dash");

            var classManager = dash.GetComponentInParent<PlayerClassManager>(true);
            Assert.IsNotNull(classManager, $"{path}: the dash can't find a PlayerClassManager above it");
            Assert.IsNotNull(classManager.ClassData, $"{path}: the PlayerClassManager has no class data");
            Assert.Greater(classManager.ClassData.dashCooldown, 0f, path);
            Assert.GreaterOrEqual(classManager.ClassData.dashMaxCharges, 1, path);
        }

        [Test]
        public void GraceDamageMultiplier_AddsPerPointAndStopsAtTheFloor()
        {
            var settings = ScriptableObject.CreateInstance<CorruptionSettings>();
            try
            {
                // Defaults: +2% per point, floor 0.1
                Assert.AreEqual(1f, settings.GetGraceDamageMultiplier(0f), 0.0001f);
                Assert.AreEqual(1.2f, settings.GetGraceDamageMultiplier(10f), 0.0001f);
                Assert.AreEqual(0.8f, settings.GetGraceDamageMultiplier(-10f), 0.0001f);
                Assert.AreEqual(0.1f, settings.GetGraceDamageMultiplier(-50f), 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }
    }
}
