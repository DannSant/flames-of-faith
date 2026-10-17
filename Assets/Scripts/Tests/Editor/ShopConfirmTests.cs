using Game.UI;
using Game.UI.RunEncounters;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The shop asks before buying and before leaving, on its ConfirmDialogUI. Without the dialog
    /// assigned, ShopControllerUI falls back to buying and leaving directly, so guard the wiring.
    /// </summary>
    public class ShopConfirmTests
    {
        private const string ShopPanePrefab = "Assets/Prefabs/UI/Shop/ShopPane.prefab";

        private static ShopControllerUI LoadShop()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopPanePrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {ShopPanePrefab}");
            var shop = prefab.GetComponentInChildren<ShopControllerUI>(true);
            Assert.IsNotNull(shop, "ShopPane has no ShopControllerUI");
            return shop;
        }

        [Test]
        public void Shop_HasAConfirmDialog()
        {
            var dialog = new SerializedObject(LoadShop()).FindProperty("confirmDialog")?.objectReferenceValue as ConfirmDialogUI;
            Assert.IsNotNull(dialog, "ShopControllerUI.confirmDialog is not set: buying and leaving won't ask first");
        }

        [Test]
        public void BuyQuestion_ShowsTheItemAndThePrice()
        {
            string question = new SerializedObject(LoadShop()).FindProperty("buyItemQuestion")?.stringValue;
            Assert.IsFalse(string.IsNullOrWhiteSpace(question), "ShopControllerUI.buyItemQuestion is empty");

            string text = string.Format(question, "ITEM", 123);
            StringAssert.Contains("ITEM", text, "The buy question doesn't show the item ({0})");
            StringAssert.Contains("123", text, "The buy question doesn't show the price ({1})");
        }
    }
}
