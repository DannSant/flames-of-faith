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
    /// The Stats pane (UI revamp step 4): it mirrors the Inventory on the right, has the "Stats" title and
    /// the tab art, scrolls its rows (masked viewport, content laid out by width), and each StatRow is a
    /// full-width band. Plus the scroll math that keeps the gamepad selection visible.
    /// </summary>
    public class StatsStyleTests
    {
        private const string PanePath = "Assets/Prefabs/UI/Progression/StatsPane.prefab";
        private const string RowPath = "Assets/Prefabs/UI/Progression/StatRow.prefab";
        private const string InventoryPath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";

        private static UISettings Settings() => AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");

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

        [Test]
        public void Pane_MirrorsTheInventory()
        {
            var settings = Settings();
            var pane = (RectTransform)Load(PanePath).transform;
            var inventory = (RectTransform)Load(InventoryPath).transform;

            Assert.AreEqual(inventory.sizeDelta, pane.sizeDelta, "Same size as the Inventory");
            Assert.AreEqual(inventory.anchoredPosition.y, pane.anchoredPosition.y, 0.01f, "Same row as the Inventory");
            Assert.AreEqual(new Vector2(1f, 0.5f), pane.anchorMin, "Anchored to the right edge");
            Assert.AreEqual(new Vector2(1f, 0.5f), pane.pivot);
            Assert.AreEqual(-settings.Margin, pane.anchoredPosition.x, 0.01f, "Margin from the right edge");
        }

        [Test]
        public void Window_IsFramedWithoutFocusTint()
        {
            var window = Child(Load(PanePath), "Scroll");
            Assert.AreEqual(UIFrameKind.Panel, window.GetComponent<UIStyleFrame>()?.Kind);
            Assert.AreEqual(Image.Type.Sliced, window.GetComponent<Image>().type);

            var uiWindow = window.GetComponent<UIWindow>();
            Assert.AreEqual(UIWindowRole.Secondary, uiWindow.Role);
            Assert.IsFalse(uiWindow.OverrideMarkerSettings, "Old arrow-era marker override");
            Assert.IsNull(new SerializedObject(uiWindow).FindProperty("focusFrame").objectReferenceValue, "The focus tint would fade the frame");
        }

        [Test]
        public void Title_IsStats()
        {
            var settings = Settings();
            var title = Child(Load(PanePath), "Title").GetComponent<TMP_Text>();
            Assert.AreEqual("Stats", title.text);
            Assert.AreEqual(settings.BigFontSize, title.fontSize);
            Assert.IsTrue(title.GetComponent<UIStyleText>().Bold);
        }

        [TestCase("MainButton", "Main")]
        [TestCase("SecondaryButton", "Secondary")]
        public void Tabs_UseTheTabArtAndSmallLabels(string tabName, string label)
        {
            var settings = Settings();
            var tab = Child(Load(PanePath), tabName).GetComponent<Button>();
            var image = tab.targetGraphic as Image;
            Assert.IsNotNull(image, "Tab target graphic");
            Assert.IsTrue(image.sprite == settings.TabSprite || image.sprite == settings.TabSelectedSprite, "Tab art");
            Assert.AreEqual(Image.Type.Sliced, image.type);
            Assert.AreEqual(Selectable.Transition.ColorTint, tab.transition, "The selected sprite is set by code, hover only tints");

            var text = tab.GetComponentInChildren<TMP_Text>(true);
            Assert.AreEqual(label, text.text);
            Assert.AreEqual(settings.SmallFontSize, text.fontSize);
        }

        [Test]
        public void Legend_IsSmallWithTinyGlyphs()
        {
            var settings = Settings();
            var prompts = Child(Load(PanePath), "InputPrompts");
            foreach (var text in prompts.GetComponentsInChildren<TMP_Text>(true))
            {
                Assert.AreEqual(settings.SmallFontSize, text.fontSize, text.name);
            }
            foreach (var glyph in prompts.GetComponentsInChildren<Image>(true))
            {
                Assert.AreEqual(new Vector2(settings.TinyIconSize, settings.TinyIconSize), glyph.rectTransform.sizeDelta, glyph.name);
                Assert.IsTrue(glyph.preserveAspect, glyph.name);
            }
        }

        [Test]
        public void List_Scrolls()
        {
            var pane = Load(PanePath);
            var scroll = Child(pane, "Scroll").GetComponent<ScrollRect>();
            Assert.IsNotNull(scroll, "No ScrollRect");
            Assert.IsTrue(scroll.enabled, "The ScrollRect is disabled");
            Assert.IsTrue(scroll.vertical);
            Assert.IsFalse(scroll.horizontal);
            Assert.AreEqual(Child(pane, "View"), scroll.viewport);
            Assert.IsTrue(scroll.viewport.GetComponent<RectMask2D>().enabled, "The viewport must clip the rows");
            Assert.IsTrue(scroll.content == Child(pane, "Primary") || scroll.content == Child(pane, "Secondary"));
            Assert.IsNotNull(scroll.verticalScrollbar, "Scrollbar");
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.None, scroll.verticalScrollbar.navigation.mode,
                "The gamepad must not land on the scrollbar");

            var ui = new SerializedObject(pane.GetComponent<StatsPaneUI>());
            Assert.AreSame(scroll, ui.FindProperty("scrollRect").objectReferenceValue, "StatsPaneUI.scrollRect not set");
        }

        [TestCase("Primary")]
        [TestCase("Secondary")]
        public void Lists_LayOutRowsByWidthAndHeight(string list)
        {
            var rect = Child(Load(PanePath), list);
            var layout = rect.GetComponent<VerticalLayoutGroup>();
            Assert.IsTrue(layout.childControlWidth, "Rows take the list width");
            Assert.IsTrue(layout.childControlHeight, "Rows take their preferred height");
            Assert.AreEqual(ContentSizeFitter.FitMode.PreferredSize, rect.GetComponent<ContentSizeFitter>().verticalFit);
            Assert.AreEqual(new Vector2(0.5f, 1f), rect.pivot, "Scroll content grows downward from the top");
        }

        [Test]
        public void UIMainInstance_UsesThePrefabsLayout()
        {
            SceneInstanceLayout.AssertMatchesPrefab("Assets/Scenes/UIMain.unity", PanePath);
        }

        [Test]
        public void Row_IsAFullWidthBand()
        {
            var settings = Settings();
            var row = Load(RowPath);
            var element = row.GetComponent<LayoutElement>();
            Assert.IsNotNull(element);
            Assert.AreEqual(32f, element.preferredHeight, "Row height");
            Assert.Greater(element.flexibleWidth, 0f, "Rows stretch to the list width");

            var band = row.GetComponent<Image>();
            Assert.AreEqual(settings.SlotFrame, band.sprite);
            Assert.AreSame(band, row.GetComponent<Selectable>().targetGraphic, "Hover/selection must light up the band");

            foreach (var text in row.GetComponentsInChildren<TMP_Text>(true))
            {
                Assert.AreEqual(settings.SmallFontSize, text.fontSize, text.name);
            }
            Assert.AreEqual(UIIconSize.Tiny, Child(row, "RoleStar").GetComponent<UIStyleIcon>()?.Size);
        }

        [Test]
        public void Row_IsClippedByTheListMask()
        {
            // A non-maskable graphic ignores the RectMask2D and draws outside the scrolled list
            foreach (var graphic in Load(RowPath).GetComponentsInChildren<MaskableGraphic>(true))
            {
                Assert.IsTrue(graphic.maskable, $"{graphic.name} is not maskable");
            }
        }

        // --- Scroll math --------------------------------------------------------------------------------

        [Test]
        public void ScrollMath_LeavesAVisibleItemAlone()
        {
            // 600 content, 300 view, scrolled to the top: an item at 100-130 is visible
            Assert.AreEqual(1f, ScrollRectMath.NormalizedPositionToShow(600f, 300f, 100f, 130f, 1f), 1e-4f);
            // scrolled half way (offset 150): an item at 200-230 is visible
            Assert.AreEqual(0.5f, ScrollRectMath.NormalizedPositionToShow(600f, 300f, 200f, 230f, 0.5f), 1e-4f);
        }

        [Test]
        public void ScrollMath_ScrollsDownJustEnoughToShowAnItemBelow()
        {
            // item 400-432 below a view at the top: offset becomes 432 - 300 = 132 of a 300 range
            float position = ScrollRectMath.NormalizedPositionToShow(600f, 300f, 400f, 432f, 1f);
            Assert.AreEqual(1f - 132f / 300f, position, 1e-4f);
        }

        [Test]
        public void ScrollMath_ScrollsUpToShowAnItemAbove()
        {
            // scrolled to the bottom (offset 300), item at 100-132: offset becomes 100
            float position = ScrollRectMath.NormalizedPositionToShow(600f, 300f, 100f, 132f, 0f);
            Assert.AreEqual(1f - 100f / 300f, position, 1e-4f);
        }

        [Test]
        public void ScrollMath_PaddingKeepsAGapAroundTheItem()
        {
            float position = ScrollRectMath.NormalizedPositionToShow(600f, 300f, 400f, 432f, 1f, 8f);
            Assert.AreEqual(1f - 140f / 300f, position, 1e-4f);
        }

        [Test]
        public void ScrollMath_ContentThatFitsStaysAtTheTop()
        {
            Assert.AreEqual(1f, ScrollRectMath.NormalizedPositionToShow(200f, 300f, 150f, 182f, 0.3f));
        }

        [Test]
        public void ScrollMath_StaysWithinRange()
        {
            Assert.AreEqual(0f, ScrollRectMath.NormalizedPositionToShow(600f, 300f, 570f, 650f, 1f), 1e-4f, "Past the end clamps to the bottom");
            Assert.AreEqual(1f, ScrollRectMath.NormalizedPositionToShow(600f, 300f, -20f, 10f, 0.2f), 1e-4f, "Before the start clamps to the top");
        }
    }
}
