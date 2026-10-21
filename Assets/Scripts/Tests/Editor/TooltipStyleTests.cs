using Game.Effects;
using Game.Progression;
using Game.UI;
using Game.UI.Style;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The shared tooltip (UI revamp step 6): a see-through frame that grows with its content and never takes
    /// raycasts, the General / Detailed / list layout, its placement beside what it describes, and the item
    /// text split that feeds the list column.
    /// </summary>
    public class TooltipStyleTests
    {
        private const string PanePath = "Assets/Prefabs/UI/Misc/GeneralTooltipPane.prefab";

        private static UISettings Settings() => AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");

        private static GameObject Load()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanePath);
            Assert.IsNotNull(prefab, $"Missing {PanePath}");
            return prefab;
        }

        private static RectTransform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        // --- Prefab -------------------------------------------------------------------------------------

        [Test]
        public void Frame_IsSeeThroughAndGrowsWithContent()
        {
            var settings = Settings();
            var panel = Child(Load(), "MainPanel");
            var frame = panel.GetComponent<UIStyleFrame>();
            Assert.AreEqual(UIFrameKind.Panel, frame.Kind);
            Assert.Less(frame.Opacity, 1f, "Some transparency");
            Assert.AreEqual(settings.TooltipOpacity, panel.GetComponent<Image>().color.a, 0.01f);
            Assert.AreEqual(Image.Type.Sliced, panel.GetComponent<Image>().type);
            Assert.AreEqual(ContentSizeFitter.FitMode.PreferredSize, panel.GetComponent<ContentSizeFitter>().verticalFit, "Height follows the text");
            Assert.AreEqual(600f, panel.sizeDelta.x, "Fixed width");
        }

        [Test]
        public void Pane_NeverTakesRaycasts()
        {
            var pane = Load();
            Assert.IsFalse(Child(pane, "MainPanel").GetComponent<CanvasGroup>().blocksRaycasts, "It would steal the pointer and flicker");
            foreach (var graphic in pane.GetComponentsInChildren<Graphic>(true))
            {
                Assert.IsFalse(graphic.raycastTarget, $"{graphic.name} is a raycast target");
            }
        }

        [Test]
        public void Texts_FollowTheStandard()
        {
            var settings = Settings();
            var pane = Load();
            var title = Child(pane, "Title").GetComponent<TMP_Text>();
            Assert.AreEqual(settings.BigFontSize, title.fontSize);
            Assert.IsTrue(title.GetComponent<UIStyleText>().Bold);
            foreach (string small in new[] { "GeneralTooltipTxt", "DetailedTooltipTxt", "ListTxt" })
            {
                var text = Child(pane, small).GetComponent<TMP_Text>();
                Assert.AreEqual(settings.SmallFontSize, text.fontSize, small);
                Assert.AreEqual(TextWrappingModes.Normal, text.textWrappingMode, $"{small} wraps");
            }
        }

        [Test]
        public void Layout_HasTheTextAndListColumns()
        {
            var pane = Load();
            Assert.AreEqual("TextColumn", Child(pane, "GeneralTooltipTxt").parent.name);
            Assert.AreEqual("TextColumn", Child(pane, "DetailedTooltipTxt").parent.name);
            Assert.AreEqual("ListColumn", Child(pane, "ListTxt").parent.name);
            Assert.AreEqual("Body", Child(pane, "ListColumn").parent.name);
            Assert.AreEqual(150f, Child(pane, "ListColumn").GetComponent<LayoutElement>().preferredWidth);

            var ui = new SerializedObject(pane.GetComponent<GeneralTooltipPaneUI>());
            Assert.AreEqual(Child(pane, "GeneralTooltipTxt").GetComponent<TMP_Text>(), ui.FindProperty("descriptionText").objectReferenceValue);
            Assert.AreEqual(Child(pane, "DetailedTooltipTxt").GetComponent<TMP_Text>(), ui.FindProperty("detailText").objectReferenceValue);
            Assert.AreEqual(Child(pane, "ListTxt").GetComponent<TMP_Text>(), ui.FindProperty("listText").objectReferenceValue);
            Assert.AreEqual(Child(pane, "ListColumn").gameObject, ui.FindProperty("listSection").objectReferenceValue);
        }

        [Test]
        public void UIMainInstance_UsesThePrefabsLayout()
        {
            SceneInstanceLayout.AssertMatchesPrefab("Assets/Scenes/UIMain.unity", PanePath);
        }

        // --- Placement ----------------------------------------------------------------------------------

        private static readonly Rect Screen1080 = new Rect(-960f, -540f, 1920f, 1080f);
        private static readonly Vector2 Pane = new Vector2(600f, 200f);

        [Test]
        public void Placement_GoesRightOfTheAnchorWhenItFits()
        {
            var anchor = new Rect(-500f, 100f, 50f, 50f); // x -500..-450, y 100..150
            Vector2 topLeft = TooltipPlacement.TopLeft(anchor, Pane, Screen1080, 15f, 12f);
            Assert.AreEqual(-450f + 12f, topLeft.x, 0.01f);
            Assert.AreEqual(150f, topLeft.y, 0.01f, "Top-aligned with the anchor");
        }

        [Test]
        public void Placement_FlipsLeftNearTheRightEdge()
        {
            var anchor = new Rect(600f, 0f, 50f, 50f);
            Vector2 topLeft = TooltipPlacement.TopLeft(anchor, Pane, Screen1080, 15f, 12f);
            Assert.AreEqual(600f - 12f - 600f, topLeft.x, 0.01f);
        }

        [Test]
        public void Placement_StaysAboveTheBottomAndBelowTheTop()
        {
            var nearBottom = new Rect(-500f, -530f, 50f, 20f);
            Assert.AreEqual(-540f + 15f + 200f, TooltipPlacement.TopLeft(nearBottom, Pane, Screen1080, 15f, 12f).y, 0.01f, "Pushed up");

            var aboveTop = new Rect(-500f, 600f, 50f, 20f);
            Assert.AreEqual(540f - 15f, TooltipPlacement.TopLeft(aboveTop, Pane, Screen1080, 15f, 12f).y, 0.01f, "Kept below the top");
        }

        [Test]
        public void Placement_WithNoRoomOnEitherSide_StaysOnScreen()
        {
            var wide = new Rect(-200f, 0f, 400f, 50f);
            var big = new Vector2(1000f, 200f);
            Vector2 topLeft = TooltipPlacement.TopLeft(wide, big, Screen1080, 15f, 12f);
            Assert.GreaterOrEqual(topLeft.x, Screen1080.xMin + 15f - 0.01f);
            Assert.LessOrEqual(topLeft.x + big.x, Screen1080.xMax - 15f + 0.01f);
        }

        [Test]
        public void Placement_PaneWiderThanTheScreen_SitsAtTheMargin()
        {
            Vector2 topLeft = TooltipPlacement.TopLeft(new Rect(0f, 0f, 10f, 10f), new Vector2(3000f, 100f), Screen1080, 15f, 12f);
            Assert.AreEqual(Screen1080.xMin + 15f, topLeft.x, 0.01f);
        }

        [Test]
        public void Placement_BesideThePointer()
        {
            // A 24x24 area below-right of the pointer tip at (100, 200)
            var pointer = new Rect(100f, 176f, 24f, 24f);
            Vector2 topLeft = TooltipPlacement.TopLeft(pointer, Pane, Screen1080, 15f, 12f);
            Assert.AreEqual(124f + 12f, topLeft.x, 0.01f, "Clear of the pointer");
            Assert.AreEqual(200f, topLeft.y, 0.01f);
        }

        // --- Item text split -----------------------------------------------------------------------------

        private static Effect MakeItem(bool withStats)
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            effect.Description = "A blade blessed by Malien.";
            if (withStats)
            {
                effect.StatModifiers = new List<StatModifier>
                {
                    new StatModifier { stat = StatType.Armor, value = 2f },
                    new StatModifier { stat = StatType.Luck, value = -1f },
                };
                effect.StatDependencies = new List<StatDependencyRule>
                {
                    new StatDependencyRule { sourceStat = StatType.Retribution, targetStat = StatType.Armor, perPoints = 1, amount = 1f },
                };
            }
            return effect;
        }

        [Test]
        public void ItemText_SplitsDescriptionFromStatLines()
        {
            var item = MakeItem(withStats: true);
            try
            {
                string description = item.GetDescriptionText();
                string stats = item.GetStatLines();
                Assert.AreEqual(description + "\n" + stats, item.GetFormattedDescription(), "The combined text is unchanged");
                Assert.AreEqual(3, stats.Split('\n').Length, "Two modifiers and one bonus rule");
                StringAssert.Contains("+2", stats);
                StringAssert.Contains("-1", stats);
                StringAssert.Contains("per", stats);
                StringAssert.DoesNotContain("+2", description);
            }
            finally
            {
                Object.DestroyImmediate(item);
            }
        }

        [Test]
        public void ItemText_WithoutStats_HasNoStatLines()
        {
            var item = MakeItem(withStats: false);
            try
            {
                Assert.AreEqual(string.Empty, item.GetStatLines());
                Assert.AreEqual(item.GetDescriptionText(), item.GetFormattedDescription());
            }
            finally
            {
                Object.DestroyImmediate(item);
            }
        }
    }
}
