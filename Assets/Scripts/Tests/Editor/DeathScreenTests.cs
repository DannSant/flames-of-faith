using Game.UI;
using Game.UI.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The death screen also opens the Stats pane and the Inventory. They must stay Secondary
    /// windows, or one of them could take gamepad focus from the death screen's Retry/Exit buttons.
    /// </summary>
    public class DeathScreenTests
    {
        private const string DeathScreenPrefab = "Assets/Prefabs/UI/Systems/DeathscreenPane.prefab";
        private const string StatsPanePrefab = "Assets/Prefabs/UI/Progression/StatsPane.prefab";
        private const string InventoryPanePrefab = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing prefab {path}");
            return prefab;
        }

        [Test]
        public void DeathScreen_OpensStatsAndInventory()
        {
            var death = Load(DeathScreenPrefab).GetComponentInChildren<DeathscreenController>(true);
            Assert.IsNotNull(death, "DeathscreenPane has no DeathscreenController");
            Assert.IsTrue(new SerializedObject(death).FindProperty("showStatsAndInventoryOnDeath").boolValue,
                "The death screen should open the Stats pane and the Inventory");
        }

        [TestCase(StatsPanePrefab)]
        [TestCase(InventoryPanePrefab)]
        public void ReviewPanes_AreSecondaryWindows(string path)
        {
            var window = Load(path).GetComponentInChildren<UIWindow>(true);
            Assert.IsNotNull(window, $"{path} has no UIWindow");
            Assert.AreEqual(UIWindowRole.Secondary, window.Role,
                $"{path} must stay a Secondary window so it can't take focus from the death screen");
        }

        [Test]
        public void DeathScreen_IsAPrimaryWindow()
        {
            var window = Load(DeathScreenPrefab).GetComponentInChildren<UIWindow>(true);
            Assert.IsNotNull(window, "DeathscreenPane has no UIWindow");
            Assert.AreEqual(UIWindowRole.Primary, window.Role, "The death screen must take gamepad focus");
        }
    }
}
