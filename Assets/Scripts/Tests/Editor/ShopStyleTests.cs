using Game.RunEncounters;
using Game.UI;
using Game.UI.Navigation;
using Game.UI.RunEncounters;
using Game.UI.Style;
using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Guards the restyled shop (its shared center-window layout is in CenterWindowStyleTests): a grid of cells that
    /// reuse the Inventory slot with the price under it, room for every item the shop can offer, the shared tooltip
    /// instead of the shop's own description panel, the error above Continue, and Continue wired in code.
    /// </summary>
    public class ShopStyleTests
    {
        private const string PanePath = "Assets/Prefabs/UI/Shop/ShopPane.prefab";
        private const string CellPath = "Assets/Prefabs/UI/Shop/ShopIconUI.prefab";
        private const string SlotPath = "Assets/Prefabs/UI/Inventory/EffectIconUI.prefab";

        private static UISettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");
            Assert.IsNotNull(settings, "Missing UISettings");
            return settings;
        }

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing {path}");
            return prefab;
        }

        private static T Ref<T>(Component owner, string field) where T : Object
        {
            var property = new SerializedObject(owner).FindProperty(field);
            Assert.IsNotNull(property, $"{owner.GetType().Name} has no field {field}");
            var value = property.objectReferenceValue as T;
            Assert.IsNotNull(value, $"{owner.GetType().Name}.{field} is not set");
            return value;
        }

        private static ShopControllerUI Controller() => Load(PanePath).GetComponent<ShopControllerUI>();

        [Test]
        public void Pane_HasNoLeftovers()
        {
            var pane = Load(PanePath);
            foreach (string leftover in new[] { "DescriptionPanel", "EffectName", "EffectDesc", "InputPrompts", "ItemsScrollView", "Scrollbar" })
            {
                Assert.IsFalse(pane.GetComponentsInChildren<Transform>(true).Any(t => t.name == leftover), $"Leftover {leftover}");
            }
            Assert.IsNull(new SerializedObject(Controller()).FindProperty("effectNameText"), "The shared tooltip replaces the shop's own panel");
            Assert.IsEmpty(pane.GetComponentsInChildren<ShopIconUI>(true), "Placeholder cells left in the grid");
        }

        [Test]
        public void Grid_FitsEveryItemTheShopCanOffer()
        {
            var settings = Settings();
            var grid = Ref<GameObject>(Controller(), "itemsContainer");
            Assert.AreEqual("Grid", grid.name);
            var layout = grid.GetComponent<GridLayoutGroup>();
            Assert.IsNotNull(layout);
            Assert.AreEqual(GridLayoutGroup.Constraint.FixedColumnCount, layout.constraint);
            Assert.AreEqual(5, layout.constraintCount);
            Assert.AreEqual(new Vector2(settings.Spacing, settings.Spacing), layout.spacing);
            Assert.AreEqual(((RectTransform)Load(CellPath).transform).sizeDelta, layout.cellSize, "Cells are the cell prefab's size");
            Assert.IsNull(grid.GetComponentInParent<ScrollRect>(true), "No scroll view: everything fits");

            // The shop's item cap (LootSettings' Shop Max Item Count) fits without scrolling
            var loot = AssetDatabase.LoadAssetAtPath<Game.Loot.LootSettings>("Assets/Resources/Loot/LootSettings.asset");
            Assert.IsNotNull(loot, "Missing LootSettings");
            int maxItems = new SerializedObject(loot).FindProperty("shopMaxItemCount").intValue;
            int rows = Mathf.CeilToInt(maxItems / (float)layout.constraintCount);
            float needed = rows * layout.cellSize.y + (rows - 1) * layout.spacing.y;
            float width = layout.constraintCount * layout.cellSize.x + (layout.constraintCount - 1) * layout.spacing.x;
            float windowHeight = ((RectTransform)Load(PanePath).transform).sizeDelta.y;
            var rect = (RectTransform)grid.transform;
            float available = windowHeight + rect.sizeDelta.y; // stretched: sizeDelta.y is minus the insets
            Assert.LessOrEqual(needed, available, $"{maxItems} items need {needed} of height, the grid has {available}");
            Assert.LessOrEqual(width, ((RectTransform)Load(PanePath).transform).sizeDelta.x + rect.sizeDelta.x, "The columns fit the window width");
        }

        [Test]
        public void Cell_IsTheInventorySlotWithThePriceUnderIt()
        {
            var settings = Settings();
            var cell = Load(CellPath);
            var ui = cell.GetComponent<ShopIconUI>();
            Assert.IsNull(cell.GetComponent<Button>(), "The slot is the button now");

            var slot = Ref<EffectIconUI>(ui, "slot");
            Assert.AreEqual(SlotPath, AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(slot.gameObject)),
                "The cell nests the Inventory's slot prefab");
            Assert.AreEqual(Vector2.one * settings.SmallIconSize, ((RectTransform)slot.transform).sizeDelta, "Small slot");
            Assert.AreSame(slot.GetComponent<Button>(), Ref<Button>(ui, "slotButton"));

            var cost = Ref<TextMeshProUGUI>(ui, "costText");
            Assert.AreEqual(UIFontSize.Small, cost.GetComponent<UIStyleText>()?.Size);
            Assert.AreEqual(UIColorRole.None, cost.GetComponent<UIStyleText>()?.ColorRole, "ShopIconUI colors the price");
            var coin = cost.transform.parent.Find("CoinsIcon");
            Assert.IsNotNull(coin, "The coin sits next to the price");
            Assert.AreEqual(UIIconSize.Tiny, coin.GetComponent<UIStyleIcon>()?.Size);
        }

        [Test]
        public void Error_IsSmallRedAboveContinue()
        {
            var settings = Settings();
            var controller = Controller();
            var error = Ref<TextMeshProUGUI>(controller, "errorTextUI");
            Assert.AreEqual(UIFontSize.Small, error.GetComponent<UIStyleText>()?.Size);
            Assert.AreEqual(UIColorRole.Negative, error.GetComponent<UIStyleText>()?.ColorRole);
            Assert.IsNull(error.GetComponent<UIStyleColor>(), "Leftover color-only style");

            var button = Ref<Button>(controller, "continueButton");
            Assert.Greater(error.rectTransform.anchoredPosition.y, ((RectTransform)button.transform).anchoredPosition.y, "Above Continue");
            Assert.AreEqual(0, button.onClick.GetPersistentEventCount(), "Continue is wired in code");
        }

        [Test]
        public void Window_TakesFocusOnOpen()
        {
            var window = Load(PanePath).transform.Find("MainPanel").GetComponent<UIWindow>();
            var so = new SerializedObject(window);
            Assert.AreEqual((int)UIWindowFocusMode.AutoFocus, so.FindProperty("focusMode").enumValueIndex, "The shop is modal");
            Assert.AreEqual(50, so.FindProperty("priority").intValue);
        }

        [Test]
        public void UIMainInstance_HasNoLayoutOverridesOnTheWindowsChildren()
        {
            SceneInstanceLayout.AssertNoChildLayoutOverrides("Assets/Scenes/UIMain.unity", PanePath);
        }

        [Test]
        public void PriceLine_TurnsRedWhenUnaffordable()
        {
            Assert.AreEqual("Price: 25 coins", ShopControllerUI.FormatPriceLine(25, true, "Price: {0} coins", "Not enough coins", "F00"));
            Assert.AreEqual("<color=#F00>Price: 25 coins - Not enough coins</color>",
                ShopControllerUI.FormatPriceLine(25, false, "Price: {0} coins", "Not enough coins", "F00"));
        }
    }
}
