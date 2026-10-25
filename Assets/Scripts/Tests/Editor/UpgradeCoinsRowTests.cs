using Game.UI;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The level-up window's "Coins gained" row (tester bug, 2026-10-25): after the item bag of an event or a
    /// treasure, which give no wave coins, the window showed a lone coin icon with an empty text.
    /// </summary>
    public class UpgradeCoinsRowTests
    {
        private const string PanePath = "Assets/Prefabs/UI/Progression/SelectUpgradePane.prefab";

        [TestCase(0, false)]
        [TestCase(-1, false)]
        [TestCase(1, true)]
        [TestCase(18, true)]
        public void CoinsRow_ShowsOnlyWhenCoinsWereGained(int coins, bool shows)
        {
            Assert.AreEqual(shows, UpgradeSelectionUI.ShowsCoinsGained(coins));
        }

        [Test]
        public void CoinsRow_HoldsTheCoinIcon()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanePath);
            Assert.IsNotNull(prefab, $"Missing {PanePath}");
            var ui = new SerializedObject(prefab.GetComponentInChildren<UpgradeSelectionUI>(true));
            var row = ui.FindProperty("coinsGainedObject").objectReferenceValue as GameObject;
            Assert.IsNotNull(row, "UpgradeSelectionUI.coinsGainedObject not set");

            var names = row.GetComponentsInChildren<Transform>(true).Select(t => t.name).ToList();
            Assert.Contains("CoinsIcon", names, "The coin icon must hide with the row");
            Assert.Contains("CoinsText", names, "The coins text must hide with the row");
        }
    }
}
