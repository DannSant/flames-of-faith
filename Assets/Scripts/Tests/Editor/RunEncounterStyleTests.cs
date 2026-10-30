using Game.RunEncounters;
using Game.UI.RunEncounters;
using Game.UI.Style;
using Game.UI.Style.Editor;
using NUnit.Framework;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Guards the restyled run encounter windows (their shared center-window layout is in CenterWindowStyleTests):
    /// the Campfire's single colored line, the Results' colored lines, the Event's bottom column of growing option
    /// buttons and its art panel, and the Treasure, which no longer has a window (its item goes to the item bag).
    /// </summary>
    public class RunEncounterStyleTests
    {
        private const string CampfirePath = "Assets/Prefabs/UI/RunEncounters/CampfirePane.prefab";
        private const string ResultsPath = "Assets/Prefabs/UI/RunEncounters/ResultsPane.prefab";
        private const string EventPath = "Assets/Prefabs/UI/RunEncounters/EventEncounterPane.prefab";
        private const string OptionPath = "Assets/Prefabs/UI/RunEncounters/EventOptionButton.prefab";
        private const string TreasurePath = "Assets/Prefabs/UI/RunEncounters/TreasurePane.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

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

        private static bool Has(GameObject root, string name) =>
            root.GetComponentsInChildren<Transform>(true).Any(t => t.name == name);

        private static T Ref<T>(Component owner, string property) where T : Object
        {
            var value = new SerializedObject(owner).FindProperty(property).objectReferenceValue as T;
            Assert.IsNotNull(value, $"{owner.GetType().Name}.{property} is not set");
            return value;
        }

        private static void AssertSmallText(TMP_Text text, UISettings settings)
        {
            Assert.AreEqual(UIFontSize.Small, text.GetComponent<UIStyleText>()?.Size, $"{text.name} size role");
            Assert.AreEqual(settings.SmallFontSize, text.fontSize, $"{text.name} font size");
            Assert.AreEqual(TextWrappingModes.Normal, text.textWrappingMode, $"{text.name} wraps");
        }

        private static void AssertWiredInCode(Button button, Button wired)
        {
            Assert.AreSame(button, wired, "The Continue field points at another button");
            Assert.AreEqual(0, button.onClick.GetPersistentEventCount(), "A persistent call would run besides the code wiring");
        }

        // --- Campfire --------------------------------------------------------------------------------------

        [Test]
        public void Campfire_HasOneSmallLineAndNoGem()
        {
            var pane = Load(CampfirePath);
            foreach (string leftover in new[] { "gemIcon", "restedTxt", "DescriptionPanel" })
            {
                Assert.IsFalse(Has(pane, leftover), $"Leftover {leftover}");
            }
            var info = Ref<TextMeshProUGUI>(pane.GetComponent<CampfireUI>(), "infoTxt");
            Assert.AreEqual("InfoText", info.name);
            AssertSmallText(info, Settings());
        }

        [Test]
        public void Campfire_ContinueIsWiredInCode()
        {
            var pane = Load(CampfirePath);
            var button = Child(pane, "ContinueButton").GetComponent<Button>();
            AssertWiredInCode(button, Ref<Button>(pane.GetComponent<CampfireUI>(), "continueButton"));
        }

        [Test]
        public void Campfire_RestedLineColorsEachGain()
        {
            Assert.AreEqual("You feel rested. You gained <color=#00FFFF>5 Grace</color> and <color=#00FF00>10 health</color>.",
                CampfireUI.FormatRestedText("You feel rested. You gained {0} and {1}.", 5, 10, "00FFFF", "00FF00"));
        }

        // --- Results ---------------------------------------------------------------------------------------

        [Test]
        public void Results_IsTitleTextAndContinue()
        {
            var pane = Load(ResultsPath);
            Assert.IsFalse(Has(pane, "DescriptionPanel"), "Leftover DescriptionPanel");
            Assert.IsNull(Child(pane, "MainPanel").GetComponent<GridLayoutGroup>(), "Leftover grid on the window");
            AssertSmallText(Ref<TextMeshProUGUI>(pane.GetComponent<RunEncounterResultUI>(), "resultsText"), Settings());

            var button = Child(pane, "ContinueButton").GetComponent<Button>();
            Assert.AreEqual("Continue", button.GetComponentInChildren<TMP_Text>(true).text);
            AssertWiredInCode(button, Ref<Button>(pane.GetComponent<RunEncounterResultUI>(), "okButton"));
        }

        [Test]
        public void Results_NarrationIsLeftAligned_AndTheSummarySitsCenteredAboveContinue()
        {
            var pane = Load(ResultsPath);
            var ui = pane.GetComponent<RunEncounterResultUI>();
            var text = Ref<TextMeshProUGUI>(ui, "resultsText");
            var summary = Ref<TextMeshProUGUI>(ui, "summaryText");
            var settings = Settings();

            Assert.AreEqual("Summary", summary.name);
            AssertSmallText(summary, settings);
            Assert.AreEqual(TextAlignmentOptions.TopLeft, text.alignment, "The narration reads like the event text");
            Assert.AreEqual(TextAlignmentOptions.Bottom, summary.alignment, "The summary is centered on its last line");
            Assert.IsTrue(summary.richText);

            // Top-anchored rows: the narration ends before the summary starts, and the summary ends above Continue
            float textBottom = -text.rectTransform.anchoredPosition.y + text.rectTransform.sizeDelta.y;
            float summaryTop = -summary.rectTransform.anchoredPosition.y;
            float summaryBottom = summaryTop + summary.rectTransform.sizeDelta.y;
            float continueTop = CenterWindowLayout.WindowSize(settings).y - CenterWindowLayout.ButtonsTop(settings, 1);
            Assert.LessOrEqual(textBottom, summaryTop + 0.01f, "The narration overlaps the summary");
            Assert.LessOrEqual(summaryBottom, continueTop - settings.Margin + 0.01f, "The summary overlaps Continue");
            Assert.AreEqual(RunEncounterRestyler.SummaryHeight(settings), summary.rectTransform.sizeDelta.y, 0.01f);
        }

        [TestCase("Gained 10 health\n", "<color=#0F0>Gained 10 health</color>")]
        [TestCase("Lost 5 Grace\r\n", "<color=#F00>Lost 5 Grace</color>")]
        [TestCase("Gained 3 Favors\nLost 10 health\nFound item: Ring\n",
            "<color=#0F0>Gained 3 Favors</color>\n<color=#F00>Lost 10 health</color>\nFound item: Ring")]
        [TestCase("", "")]
        public void Results_ColorGainsAndLosses(string results, string expected)
        {
            Assert.AreEqual(expected, RunEncounterResultUI.ColorizeResultLines(results, "0F0", "F00"));
        }

        // --- Event -----------------------------------------------------------------------------------------

        [Test]
        public void Event_ArtPanelSitsLeftOfTheWindowAndStartsHidden()
        {
            var settings = Settings();
            var pane = Load(EventPath);
            var ui = pane.GetComponent<EventEncounterUI>();
            var panel = (RectTransform)Ref<GameObject>(ui, "artPanel").transform;
            var art = Ref<Image>(ui, "artImage");

            Assert.IsFalse(panel.gameObject.activeSelf, "Only events with art show the panel");
            Assert.AreEqual(new Vector2(0f, 0.5f), panel.anchorMin, "Anchored to the window's left edge");
            Assert.AreEqual(new Vector2(0f, 0.5f), panel.anchorMax);
            Assert.AreEqual(new Vector2(1f, 0.5f), panel.pivot);
            Assert.AreEqual(new Vector2(-settings.Margin, 0f), panel.anchoredPosition, "One margin from the window");
            Assert.AreEqual(((RectTransform)pane.transform).sizeDelta, panel.sizeDelta, "The window's size");
            Assert.AreEqual(UIFrameKind.Panel, panel.GetComponent<UIStyleFrame>()?.Kind, "Framed like a window");

            Assert.AreSame(panel, art.transform.parent);
            Assert.IsTrue(art.preserveAspect, "Art keeps its proportions");
            Assert.IsFalse(art.raycastTarget);
        }

        [Test]
        public void Event_TextFillsTheRoomTheOptionsLeave()
        {
            var settings = Settings();
            var ui = Load(EventPath).GetComponent<EventEncounterUI>();
            var description = Ref<TextMeshProUGUI>(ui, "descriptionText");
            AssertSmallText(description, settings);
            Assert.AreEqual(1f, description.GetComponent<LayoutElement>()?.flexibleHeight, "The text takes the free room");

            var options = Ref<Transform>(ui, "optionsContainer");
            Assert.AreSame(description.transform.parent, options.parent, "Text and options share the body column");
            Assert.Greater(options.GetSiblingIndex(), description.transform.GetSiblingIndex(), "Options below the text");
            Assert.IsNotNull(options.parent.GetComponent<VerticalLayoutGroup>(), "The body is a column");

            var column = options.GetComponent<VerticalLayoutGroup>();
            Assert.IsNotNull(column, "Options are a column");
            Assert.IsNull(options.GetComponent<HorizontalLayoutGroup>(), "Leftover horizontal row");
            Assert.AreEqual(TextAnchor.LowerCenter, column.childAlignment, "Options sit at the bottom");
            Assert.IsTrue(column.childControlHeight, "Each option takes its own height");
            Assert.IsTrue(column.childForceExpandWidth, "Options are full width");
            Assert.AreEqual(settings.Spacing, column.spacing);
        }

        [Test]
        public void EventOption_IsASmallButtonThatGrowsWithItsLabel()
        {
            var settings = Settings();
            var option = Load(OptionPath);
            Assert.AreEqual(UIButtonSize.Small, option.GetComponent<UIStyleButton>()?.Size);
            Assert.IsNull(option.GetComponent<LayoutElement>(), "A root LayoutElement would pin the height to the Small height");
            var layout = option.GetComponent<VerticalLayoutGroup>();
            Assert.IsNotNull(layout, "The label sets the button's height through the root layout");
            Assert.IsTrue(layout.childControlHeight);

            var label = option.GetComponentInChildren<TextMeshProUGUI>(true);
            AssertSmallText(label, settings);
            float minHeight = label.GetComponent<LayoutElement>().minHeight + layout.padding.vertical;
            Assert.AreEqual(settings.GetButtonHeight(UIButtonSize.Small), minHeight, 0.01f, "Never shorter than a Small button");

            var graphic = option.GetComponent<Button>().targetGraphic;
            Assert.IsTrue(graphic.GetComponent<LayoutElement>()?.ignoreLayout ?? false, "The button art stays stretched");
        }

        // --- Treasure --------------------------------------------------------------------------------------

        [Test]
        public void Treasure_HasNoWindowOfItsOwn()
        {
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<GameObject>(TreasurePath), "TreasurePane is gone: treasure items open the item bag");
            Assert.IsFalse(System.IO.File.Exists(TreasurePath + ".meta"));
            StringAssert.DoesNotContain("TreasurePane", System.IO.File.ReadAllText(UIMainScene), "UIMain still holds a TreasurePane");
        }

        [TestCase(TreasureRewardType.Currency, 75, "Gained 75 Golden Chronos")]
        [TestCase(TreasureRewardType.Experience, 30, "Gained 30 Experience")]
        public void Treasure_ResultLines(TreasureRewardType type, int amount, string expected)
        {
            Assert.AreEqual(expected, TreasureEncounterController.DescribeReward(type, amount));
        }
    }
}
