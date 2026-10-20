using Game.UI;
using Game.UI.Navigation;
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
    /// Guards the center windows (level-up, pause, death screen, settings): they share the Inventory's
    /// height and vertical position, are framed, have a bold title, use Small buttons, and keep their
    /// progress button (Continue / Resume / Retry / Back) in the same spot.
    /// </summary>
    public class CenterWindowStyleTests
    {
        private const string InventoryPath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";
        private const string UpgradePath = "Assets/Prefabs/UI/Progression/SelectUpgradePane.prefab";

        public class WindowCase
        {
            public string prefab;
            public string frame;          // object holding the window frame
            public string title;
            public UIFontSize titleSize;
            public UIColorRole titleColor;
            public string progressButton;
            public string[] otherButtons;
            public bool inUIMain;

            public override string ToString() => System.IO.Path.GetFileNameWithoutExtension(prefab);
        }

        public static IEnumerable<WindowCase> Windows()
        {
            yield return new WindowCase
            {
                prefab = UpgradePath, frame = "MainPanel", title = "SelectUpgradeText",
                titleSize = UIFontSize.Big, titleColor = UIColorRole.Text,
                progressButton = "ContinueButton", otherButtons = new string[0], inUIMain = true,
            };
            yield return new WindowCase
            {
                prefab = "Assets/Prefabs/UI/Systems/PauseMenu.prefab", frame = "MainPanel", title = "Title",
                titleSize = UIFontSize.Big, titleColor = UIColorRole.Highlight,
                progressButton = "ResumeButton", otherButtons = new[] { "ExitButton" }, inUIMain = true,
            };
            yield return new WindowCase
            {
                // YOU DIED is the agreed headline exception: Display size, red
                prefab = "Assets/Prefabs/UI/Systems/DeathscreenPane.prefab", frame = "MainPanel", title = "Title",
                titleSize = UIFontSize.Display, titleColor = UIColorRole.Negative,
                progressButton = "RetryButton", otherButtons = new[] { "ExitButton" }, inUIMain = true,
            };
        }

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

        [TestCaseSource(nameof(Windows))]
        public void Window_HasTheInventorysHeightAndRow(WindowCase window)
        {
            var settings = Settings();
            var rect = (RectTransform)Load(window.prefab).transform;
            var inventory = (RectTransform)Load(InventoryPath).transform;

            Assert.AreEqual(inventory.sizeDelta.y, rect.sizeDelta.y, 0.01f, "Same height as the Inventory");
            Assert.AreEqual(inventory.anchoredPosition.y, rect.anchoredPosition.y, 0.01f, "Same vertical position as the Inventory");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), rect.anchorMin, "Centered");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), rect.anchorMax, "Centered");
            Assert.AreEqual(0f, rect.anchoredPosition.x, 0.01f, "Centered");
            Assert.AreEqual(settings.Margin, rect.anchoredPosition.y, 0.01f);
        }

        [TestCaseSource(nameof(Windows))]
        public void Window_IsFramed(WindowCase window)
        {
            var frame = Child(Load(window.prefab), window.frame);
            Assert.AreEqual(UIFrameKind.Panel, frame.GetComponent<UIStyleFrame>()?.Kind, "Panel frame");
            Assert.AreEqual(Image.Type.Sliced, frame.GetComponent<Image>().type);
        }

        [TestCaseSource(nameof(Windows))]
        public void Title_IsBoldInItsSizeAndColor(WindowCase window)
        {
            var settings = Settings();
            var title = Child(Load(window.prefab), window.title).GetComponent<TMP_Text>();
            var style = title.GetComponent<UIStyleText>();
            Assert.IsNotNull(style, "Title has no UIStyleText");
            Assert.AreEqual(window.titleSize, style.Size);
            Assert.IsTrue(style.Bold, "Titles are bold");
            Assert.AreEqual(window.titleColor, style.ColorRole);
            Assert.AreEqual(settings.GetFontSize(window.titleSize), title.fontSize);
            Assert.AreEqual((Vector3)(Vector4)settings.GetColor(window.titleColor), (Vector3)(Vector4)title.color, "Baked title color");
        }

        [TestCaseSource(nameof(Windows))]
        public void Buttons_AreSmall(WindowCase window)
        {
            var settings = Settings();
            var root = Load(window.prefab);
            foreach (string name in window.otherButtons.Append(window.progressButton))
            {
                var button = Child(root, name);
                Assert.AreEqual(UIButtonSize.Small, button.GetComponent<UIStyleButton>()?.Size, $"{name} size role");
                Assert.AreEqual(settings.GetButtonHeight(UIButtonSize.Small), button.sizeDelta.y, $"{name} height");
                var label = button.GetComponentInChildren<TMP_Text>(true);
                Assert.AreEqual(settings.SmallFontSize, label.fontSize, $"{name} label size");
            }
        }

        [Test]
        public void ProgressButtons_ShareOneSpot()
        {
            var cases = Windows().ToList();
            var first = Child(Load(cases[0].prefab), cases[0].progressButton);
            foreach (var window in cases.Skip(1))
            {
                var button = Child(Load(window.prefab), window.progressButton);
                string label = $"{window} {window.progressButton} vs {cases[0]} {cases[0].progressButton}";
                Assert.AreEqual(first.anchorMin, button.anchorMin, label);
                Assert.AreEqual(first.anchorMax, button.anchorMax, label);
                Assert.AreEqual(first.pivot, button.pivot, label);
                Assert.AreEqual(first.anchoredPosition, button.anchoredPosition, label);
                Assert.AreEqual(first.sizeDelta, button.sizeDelta, label);
            }
        }

        [TestCaseSource(nameof(Windows))]
        public void Window_UsesTheStandardSelectionPadding(WindowCase window)
        {
            foreach (var uiWindow in Load(window.prefab).GetComponentsInChildren<UIWindow>(true))
            {
                if (PrefabUtility.IsPartOfPrefabInstance(uiWindow)) continue; // nested windows (confirm dialog) have their own step
                Assert.IsFalse(uiWindow.OverrideMarkerSettings, $"{uiWindow.name}: old arrow-era marker override");
                var focusFrame = new SerializedObject(uiWindow).FindProperty("focusFrame").objectReferenceValue;
                Assert.IsNull(focusFrame, $"{uiWindow.name}: the focus tint would fade the window frame");
            }
        }

        [TestCaseSource(nameof(Windows))]
        public void UIMainInstance_UsesThePrefabsLayout(WindowCase window)
        {
            if (!window.inUIMain) Assert.Ignore("Not placed in UIMain");
            SceneInstanceLayout.AssertMatchesPrefab("Assets/Scenes/UIMain.unity", window.prefab);
        }

        // --- Level-up window ----------------------------------------------------------------------------

        [Test]
        public void Upgrade_HeaderHasALevelLineAndCoins()
        {
            var settings = Settings();
            var pane = Load(UpgradePath);
            var ui = new SerializedObject(pane.GetComponent<UpgradeSelectionUI>());

            var level = ui.FindProperty("levelText").objectReferenceValue as TMP_Text;
            Assert.IsNotNull(level, "levelText not set");
            Assert.AreEqual(UIFontSize.Small, level.GetComponent<UIStyleText>()?.Size);
            Assert.AreEqual(UIColorRole.Highlight, level.GetComponent<UIStyleText>()?.ColorRole, "Level line is yellow");

            var coins = Child(pane, "CoinsText").GetComponent<TMP_Text>();
            Assert.AreEqual(UIFontSize.Small, coins.GetComponent<UIStyleText>()?.Size);
            Assert.AreEqual(UIColorRole.Positive, coins.GetComponent<UIStyleText>()?.ColorRole, "Coins gained is green");
            Assert.AreEqual(UIIconSize.Tiny, Child(pane, "CoinsIcon").GetComponent<UIStyleIcon>()?.Size);
            Assert.AreEqual(settings.TinyIconSize, Child(pane, "CoinsIcon").sizeDelta.x);
        }

        [Test]
        public void Upgrade_ChoicesSpawnInAColumnAndThePrefabHasNoPlaceholders()
        {
            var pane = Load(UpgradePath);
            var ui = new SerializedObject(pane.GetComponent<UpgradeSelectionUI>());
            var options = ui.FindProperty("optionsContainer").objectReferenceValue as Transform;
            Assert.IsNotNull(options, "optionsContainer not set");
            Assert.IsNotNull(options.GetComponent<VerticalLayoutGroup>(), "Choices are laid out in a column");
            Assert.IsEmpty(pane.GetComponentsInChildren<UpgradeButtonUI>(true), "Placeholder or stray UpgradeButtonUI left in the pane");
        }

        [Test]
        public void UpgradeButton_IsASmallFullWidthButtonWithATinyStar()
        {
            var settings = Settings();
            var button = Load("Assets/Prefabs/UI/Progression/UpgradeButton.prefab");
            Assert.AreEqual(UIButtonSize.Small, button.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(settings.GetButtonHeight(UIButtonSize.Small), button.GetComponent<LayoutElement>().preferredHeight);
            Assert.AreEqual(settings.SmallFontSize, button.GetComponentInChildren<TMP_Text>(true).fontSize);
            Assert.AreEqual(UIIconSize.Tiny, Child(button, "RoleStar").GetComponent<UIStyleIcon>()?.Size);
        }

        [TestCase(3, "You are now Level 3")]
        [TestCase(12, "You are now Level 12")]
        public void Upgrade_LevelLine(int level, string expected)
        {
            Assert.AreEqual(expected, UpgradeSelectionUI.FormatLevelLine("You are now Level {0}", level));
        }
    }
}
