using Game.Effects;
using Game.UI;
using Game.UI.Navigation;
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
    /// Guards the Inventory window (UI revamp step 2): framed, a Big bold title, small coins with a
    /// 28px icon, an 8-column grid of Small slots, no built-in description panel (items use the shared
    /// tooltip), and slots with a quality-tinted frame and a count badge.
    /// </summary>
    public class InventoryStyleTests
    {
        private const string InventoryPath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";
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

        private static RectTransform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        private static void AssertText(TMP_Text text, UIFontSize size, bool bold, UISettings settings)
        {
            var style = text.GetComponent<UIStyleText>();
            Assert.IsNotNull(style, $"{text.name} has no UIStyleText");
            Assert.AreEqual(size, style.Size, $"{text.name} size role");
            Assert.AreEqual(settings.GetFontSize(size), text.fontSize, $"{text.name} font size");
            Assert.AreEqual(bold, (text.fontStyle & FontStyles.Bold) != 0, $"{text.name} bold");
        }

        [Test]
        public void Window_IsFramedAndFitsOnScreen()
        {
            var settings = Settings();
            var pane = Load(InventoryPath);
            var frame = pane.GetComponent<UIStyleFrame>();
            Assert.IsNotNull(frame, "The pane has no UIStyleFrame");
            Assert.AreEqual(UIFrameKind.Panel, frame.Kind);
            Assert.AreEqual(Image.Type.Sliced, pane.GetComponent<Image>().type);

            var rect = (RectTransform)pane.transform;
            Assert.AreEqual(settings.Margin, rect.anchoredPosition.x, "Left margin");
            float halfHeight = rect.sizeDelta.y * 0.5f;
            float reference = settings.ReferenceResolution.y;
            Assert.LessOrEqual(reference * 0.5f + rect.anchoredPosition.y + halfHeight, reference - settings.Margin, "Too tall: past the top");
            Assert.GreaterOrEqual(reference * 0.5f + rect.anchoredPosition.y - halfHeight, settings.Margin, "Too tall: past the bottom");
        }

        [Test]
        public void UIMainInstance_UsesThePrefabsLayout()
        {
            SceneInstanceLayout.AssertMatchesPrefab("Assets/Scenes/UIMain.unity", InventoryPath);
        }

        [Test]
        public void Title_IsBigAndBold()
        {
            var title = Child(Load(InventoryPath), "Title").GetComponent<TMP_Text>();
            AssertText(title, UIFontSize.Big, true, Settings());
        }

        [Test]
        public void Coins_AreSmallWithAPromptSizedIcon()
        {
            var settings = Settings();
            var pane = Load(InventoryPath);
            AssertText(Child(pane, "CoinsText").GetComponent<TMP_Text>(), UIFontSize.Small, false, settings);

            var icon = Child(pane, "CoinsIcon");
            Assert.AreEqual(UIIconSize.Prompt, icon.GetComponent<UIStyleIcon>()?.Size, "Coin icon size role");
            Assert.AreEqual(28f, settings.PromptIconSize, "The coin icon is meant to be 28x28");
            Assert.AreEqual(new Vector2(28f, 28f), icon.sizeDelta);
        }

        [Test]
        public void DescriptionPanel_IsGone_ItemsUseTheSharedTooltip()
        {
            var pane = Load(InventoryPath);
            Assert.IsFalse(pane.GetComponentsInChildren<Transform>(true).Any(t => t.name == "DescriptionPanel"),
                "The built-in description panel should be replaced by GeneralTooltipPane");
            Assert.IsEmpty(pane.GetComponentsInChildren<EffectIconUI>(true), "Placeholder slot left in the prefab");

            var controller = new SerializedObject(pane.GetComponent<InventoryControllerUI>());
            Assert.IsNull(controller.FindProperty("effectNameText"), "Old tooltip field still serialized");
        }

        [Test]
        public void Grid_IsEightColumnsOfSmallSlots()
        {
            var settings = Settings();
            var pane = Load(InventoryPath);
            var gridRect = Child(pane, "Grid");
            var grid = gridRect.GetComponent<GridLayoutGroup>();
            Assert.IsNotNull(grid);
            Assert.AreEqual(GridLayoutGroup.Constraint.FixedColumnCount, grid.constraint);
            Assert.AreEqual(8, grid.constraintCount);
            Assert.AreEqual(UIIconSize.Small, gridRect.GetComponent<UIStyleGrid>()?.CellSize);
            Assert.AreEqual(new Vector2(settings.SmallIconSize, settings.SmallIconSize), grid.cellSize);
            Assert.AreEqual(new Vector2(settings.Spacing, settings.Spacing), grid.spacing);

            var controller = new SerializedObject(pane.GetComponent<InventoryControllerUI>());
            Assert.AreEqual(gridRect, controller.FindProperty("iconContainer").objectReferenceValue, "Slots must spawn in the grid");

            // The pane is wide enough for 8 columns inside its margins
            float needed = 2f * settings.Margin + UIStyleGrid.Span(8, settings.SmallIconSize, settings.Spacing);
            Assert.GreaterOrEqual(((RectTransform)pane.transform).sizeDelta.x, needed - 0.01f);
        }

        [Test]
        public void Window_UsesTheStandardSelectionPadding()
        {
            var window = Child(Load(InventoryPath), "MainPanel").GetComponent<UIWindow>();
            Assert.IsNotNull(window);
            Assert.AreEqual(UIWindowRole.Secondary, window.Role);
            Assert.IsFalse(window.OverrideMarkerSettings, "Old arrow-era marker override");
        }

        [Test]
        public void Slot_HasASlicedFrameAnIconAndASmallBadge()
        {
            var settings = Settings();
            var slot = Load(SlotPath);
            var frame = slot.GetComponent<UIStyleFrame>();
            Assert.IsNotNull(frame, "Slot has no UIStyleFrame");
            Assert.AreEqual(UIFrameKind.Slot, frame.Kind);
            Assert.AreEqual(settings.SlotFrame, slot.GetComponent<Image>().sprite);
            Assert.AreEqual(Image.Type.Sliced, slot.GetComponent<Image>().type);

            var ui = new SerializedObject(slot.GetComponent<EffectIconUI>());
            var icon = ui.FindProperty("icon").objectReferenceValue as Image;
            Assert.IsNotNull(icon, "icon not set");
            Assert.AreNotSame(slot.GetComponent<Image>(), icon, "The icon must be its own image inside the frame");
            Assert.AreSame(slot.GetComponent<Image>(), ui.FindProperty("frame").objectReferenceValue, "frame not set");

            AssertText(Child(slot, "QtyText").GetComponent<TMP_Text>(), UIFontSize.Small, false, settings);
        }

        [TestCase(1, "")]
        [TestCase(0, "")]
        [TestCase(2, "2")]
        [TestCase(15, "15")]
        public void Slot_ShowsTheCountOnlyForStacks(int quantity, string expected)
        {
            Assert.AreEqual(expected, EffectIconUI.FormatQuantity(quantity));
        }

        [Test]
        public void ItemName_IsColoredByQuality()
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            try
            {
                effect.EffectName = "Glyph of Test";
                effect.Quality = EffectQuality.Legendary;
                string hex = ColorUtility.ToHtmlStringRGB(EffectQualityDisplayHelper.GetQualityColor(EffectQuality.Legendary));
                Assert.AreEqual($"<color=#{hex}>Glyph of Test</color>", EffectQualityDisplayHelper.ColorizeName(effect));
                Assert.AreEqual(string.Empty, EffectQualityDisplayHelper.ColorizeName(null));
            }
            finally
            {
                Object.DestroyImmediate(effect);
            }
        }
    }
}
