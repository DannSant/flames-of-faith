using Game.UI;
using Game.UI.Items;
using Game.UI.Style;
using NUnit.Framework;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The end-of-wave item bag (UI revamp step 5): a 6-column grid of the Inventory's slots, no built-in
    /// description panel (the shared tooltip shows the selected item), a subtitle, the selected-slot outline,
    /// and the Inventory opened alongside. The window's shared layout is in CenterWindowStyleTests.
    /// </summary>
    public class ItemBagStyleTests
    {
        private const string PanePath = "Assets/Prefabs/UI/ItemBag/ItemBagPane.prefab";
        private const string SlotPath = "Assets/Prefabs/UI/Inventory/EffectIconUI.prefab";

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing {path}");
            return prefab;
        }

        private static RectTransform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        private static UISettings Settings() => AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");

        [Test]
        public void Items_UseTheInventorySlotInASixColumnGrid()
        {
            var settings = Settings();
            var pane = Load(PanePath);
            var ui = new SerializedObject(pane.GetComponent<ItemBagControllerUI>());

            var prefab = ui.FindProperty("itemIconPrefab").objectReferenceValue as EffectIconUI;
            Assert.IsNotNull(prefab, "itemIconPrefab must be the Inventory's slot");
            Assert.AreEqual(SlotPath, AssetDatabase.GetAssetPath(prefab));

            var grid = ui.FindProperty("itemIconsContainer").objectReferenceValue as Transform;
            Assert.IsNotNull(grid, "itemIconsContainer not set");
            var layout = grid.GetComponent<GridLayoutGroup>();
            Assert.AreEqual(GridLayoutGroup.Constraint.FixedColumnCount, layout.constraint);
            Assert.AreEqual(6, layout.constraintCount);
            Assert.AreEqual(new Vector2(settings.SmallIconSize, settings.SmallIconSize), layout.cellSize, "Same slot size as the Inventory");
            Assert.AreEqual(0, grid.childCount, "Placeholder icons left in the grid");
        }

        [Test]
        public void DescriptionPanel_IsGone()
        {
            var pane = Load(PanePath);
            foreach (string leftover in new[] { "ItemBagDescriptionPanel", "ItemName", "ItemDesc", "ItemsPanel", "ContinueButton" })
            {
                Assert.IsFalse(pane.GetComponentsInChildren<Transform>(true).Any(t => t.name == leftover), $"{leftover} is still in the pane");
            }
            Assert.IsEmpty(pane.GetComponentsInChildren<UpgradeButtonUI>(true), "Stray UpgradeButtonUI left on a button");
        }

        [Test]
        public void Subtitle_IsSmallAndMuted()
        {
            var subtitle = Child(Load(PanePath), "SubtitleText").GetComponent<TMP_Text>();
            var style = subtitle.GetComponent<UIStyleText>();
            Assert.AreEqual(UIFontSize.Small, style.Size);
            Assert.AreEqual(UIColorRole.Muted, style.ColorRole);
        }

        [Test]
        public void SellLabel_ShowsThePrice()
        {
            Assert.AreEqual("Sell (25)", ItemBagControllerUI.FormatSellLabel("Sell ({0})", 25));
            var ui = new SerializedObject(Load(PanePath).GetComponent<ItemBagControllerUI>());
            var label = ui.FindProperty("itemSellText").objectReferenceValue as TMP_Text;
            Assert.IsNotNull(label, "itemSellText not set");
            Assert.AreEqual("SellButton", label.transform.parent.name, "The price goes on the Sell button's label");
        }

        [Test]
        public void Slot_HasAHiddenSelectedOutline()
        {
            var settings = Settings();
            var slot = Load(SlotPath);
            var mark = new SerializedObject(slot.GetComponent<EffectIconUI>()).FindProperty("selectedMark").objectReferenceValue as GameObject;
            Assert.IsNotNull(mark, "selectedMark not set");
            Assert.IsFalse(mark.activeSelf, "Only the selected slot shows it");
            var image = mark.GetComponent<Image>();
            Assert.AreEqual(settings.SelectedOutline, image.sprite);
            Assert.IsFalse(image.raycastTarget, "The outline must not take the slot's clicks");
            Assert.AreEqual(UIColorRole.Highlight, mark.GetComponent<UIStyleColor>()?.ColorRole, "Yellow outline");
        }

        [Test]
        public void InventoryOpensAlongside()
        {
            // The scene wires the item bag to UIMain's Inventory
            string yaml = File.ReadAllText("Assets/Scenes/UIMain.unity");
            Assert.IsTrue(yaml.Contains("propertyPath: inventoryUI"), "ItemBagControllerUI.inventoryUI is not set in UIMain");
        }

        [Test]
        public void SpawnedSlot_IsInItsCell_WhenTheTooltipIsPlaced()
        {
            // The tooltip is placed beside a slot spawned the same frame; it showed above the buttons
            var canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            try
            {
                var pane = (GameObject)PrefabUtility.InstantiatePrefab(Load(PanePath), canvas.transform);
                var ui = new SerializedObject(pane.GetComponent<ItemBagControllerUI>());
                var grid = (RectTransform)ui.FindProperty("itemIconsContainer").objectReferenceValue;
                var mainPane = (GameObject)ui.FindProperty("mainPane").objectReferenceValue;
                var layout = grid.GetComponent<GridLayoutGroup>();
                float step = layout.cellSize.x + layout.spacing.x;
                var slot = Load(SlotPath);
                mainPane.SetActive(true);

                // Every slot spawns at the same spot; only the layout moves them into their cells
                var taken = (RectTransform)Object.Instantiate(slot, grid).transform;
                var kept = (RectTransform)Object.Instantiate(slot, grid).transform;
                ItemBagControllerUI.LayOutNow(grid);
                Vector3 firstCell = taken.localPosition;
                Assert.AreEqual(step, kept.localPosition.x - taken.localPosition.x, 0.01f, "The grid was not laid out");

                // Take / Sell: the old slots go inactive (their Destroy waits for the frame's end), new ones spawn
                taken.gameObject.SetActive(false);
                kept.gameObject.SetActive(false);
                var first = (RectTransform)Object.Instantiate(slot, grid).transform;
                var second = (RectTransform)Object.Instantiate(slot, grid).transform;
                ItemBagControllerUI.LayOutNow(grid);

                Assert.AreEqual(firstCell, first.localPosition, "The first new slot must take the first cell");
                Assert.AreEqual(step, second.localPosition.x - first.localPosition.x, 0.01f, "The second slot must be in the next cell");
            }
            finally
            {
                Object.DestroyImmediate(canvas);
            }
        }
    }
}
