using Game.Scene;
using Game.UI.InputPrompts;
using Game.UI.Navigation;
using Game.UI.Overworld;
using Game.UI.Style;
using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Guards the overworld map's HUD (LevelSelector scene): the map window (act title, level, coins, Save &amp; Exit)
    /// top right above the Stats pane, the framed gamepad prompts bottom left that switch rows with focus, the scene
    /// wiring that shows the level name and opens the Stats pane, and readable level names.
    /// </summary>
    public class LevelSelectorStyleTests
    {
        private const string WindowPath = "Assets/Prefabs/UI/LevelSelector/LevelSelectorUI.prefab";
        private const string PromptsPath = "Assets/Prefabs/UI/LevelSelector/InputPrompts.prefab";
        private const string StatsPanePath = "Assets/Prefabs/UI/Progression/StatsPane.prefab";
        private const string ScenePath = "Assets/Scenes/LevelSelector.unity";

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
            var value = new SerializedObject(owner).FindProperty(field)?.objectReferenceValue as T;
            Assert.IsNotNull(value, $"{owner.GetType().Name}.{field} is not set");
            return value;
        }

        private static void AssertText(TMP_Text text, UIFontSize size, UISettings settings)
        {
            Assert.AreEqual(size, text.GetComponent<UIStyleText>()?.Size, $"{text.name} size role");
            Assert.AreEqual(settings.GetFontSize(size), text.fontSize, text.name);
        }

        // --- Map window --------------------------------------------------------------------------------------

        [Test]
        public void MapWindow_SitsTopRightAboveTheStatsPane()
        {
            var settings = Settings();
            var window = (RectTransform)Load(WindowPath).transform;
            var stats = (RectTransform)Load(StatsPanePath).transform;

            Assert.AreEqual(new Vector2(1f, 1f), window.anchorMin);
            Assert.AreEqual(new Vector2(1f, 1f), window.pivot);
            Assert.AreEqual(new Vector2(-settings.Margin, -settings.Margin), window.anchoredPosition);
            Assert.AreEqual(stats.sizeDelta.x, window.sizeDelta.x, "As wide as the Stats pane under it");
            Assert.AreEqual(stats.anchoredPosition.x, window.anchoredPosition.x, "Same right edge as the Stats pane");

            // The Stats pane is centered on the right edge: its top is half its height above the screen's middle
            float statsTopFromScreenTop = settings.ReferenceResolution.y * 0.5f - stats.anchoredPosition.y - stats.sizeDelta.y * 0.5f;
            Assert.LessOrEqual(settings.Margin + window.sizeDelta.y, statsTopFromScreenTop, "The map window ends above the Stats pane");

            Assert.AreEqual(UIFrameKind.Panel, window.GetComponent<UIStyleFrame>()?.Kind, "Framed");
        }

        [Test]
        public void MapWindow_ShowsTitleLevelCoinsAndExitInTheStandardSizes()
        {
            var settings = Settings();
            var root = Load(WindowPath);
            var info = root.GetComponent<OverworldMapInfoUI>();
            Assert.IsNotNull(info, "The map window has no OverworldMapInfoUI");

            var title = Ref<TextMeshProUGUI>(info, "actTitleText");
            AssertText(title, UIFontSize.Big, settings);
            Assert.AreEqual(UIColorRole.Highlight, title.GetComponent<UIStyleText>()?.ColorRole, "Act title in yellow");
            Assert.IsTrue(title.GetComponent<UIStyleText>().Bold);

            AssertText(Ref<TextMeshProUGUI>(info, "levelNameText"), UIFontSize.Small, settings);
            var coins = Ref<TextMeshProUGUI>(info, "coinsText");
            AssertText(coins, UIFontSize.Small, settings);
            var coin = coins.transform.parent.Find("CoinsIcon");
            Assert.IsNotNull(coin, "Coin icon next to the coins");
            Assert.AreEqual(UIIconSize.Tiny, coin.GetComponent<UIStyleIcon>()?.Size);

            var exit = root.transform.Find("ExitButton");
            Assert.AreEqual(UIButtonSize.Small, exit.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(settings.SmallFontSize, exit.GetComponentInChildren<TMP_Text>(true).fontSize);
            Assert.IsFalse(root.GetComponentsInChildren<Transform>(true).Any(t => t.name == "LevelNamePane"), "Leftover LevelNamePane");

            var window = root.GetComponent<UIWindow>();
            var so = new SerializedObject(window);
            Assert.AreEqual((int)UIWindowRole.Primary, so.FindProperty("role").enumValueIndex);
            Assert.AreEqual((int)UIWindowFocusMode.OnDemand, so.FindProperty("focusMode").enumValueIndex, "Y enters the UI; the map keeps the stick");
            Assert.IsNull(so.FindProperty("focusFrame").objectReferenceValue);
        }

        // --- Gamepad prompts ---------------------------------------------------------------------------------

        [Test]
        public void Prompts_AreAFramedWindowBottomLeft_GamepadOnly()
        {
            var settings = Settings();
            var root = Load(PromptsPath);
            var rect = (RectTransform)root.transform;
            Assert.AreEqual(Vector2.zero, rect.anchorMin);
            Assert.AreEqual(Vector2.zero, rect.pivot);
            Assert.AreEqual(new Vector2(settings.Margin, settings.Margin), rect.anchoredPosition);
            Assert.AreEqual(UIFrameKind.Panel, root.GetComponent<UIStyleFrame>()?.Kind, "Framed");
            Assert.IsNotNull(root.GetComponent<ContentSizeFitter>(), "The frame follows the rows shown");

            var visibility = root.GetComponents<InputPromptVisibilityUI>();
            Assert.AreEqual(3, visibility.Length, "The window, the map rows and the menu rows");
            foreach (var v in visibility)
            {
                var so = new SerializedObject(v);
                Assert.IsFalse(so.FindProperty("showOnKeyboardMouse").boolValue, "Gamepad only");
                Assert.IsTrue(so.FindProperty("showOnGamepad").boolValue);
            }

            GlobalFocusRequirement RequirementFor(string group) => visibility
                .Select(v => new SerializedObject(v))
                .Where(so => so.FindProperty("targets").arraySize == 1
                    && (so.FindProperty("targets").GetArrayElementAtIndex(0).objectReferenceValue as GameObject)?.name == group)
                .Select(so => (GlobalFocusRequirement)so.FindProperty("globalRequirement").enumValueIndex)
                .Single();
            Assert.AreEqual(GlobalFocusRequirement.NoWindowFocused, RequirementFor("MapRows"), "Move / Enter level / Menu while on the map");
            Assert.AreEqual(GlobalFocusRequirement.AnyWindowFocused, RequirementFor("MenuRows"), "Swap windows / Back while in the UI");

            foreach (var image in root.GetComponentsInChildren<Image>(true).Where(i => i.gameObject != root))
            {
                Assert.AreEqual(UIIconSize.Tiny, image.GetComponent<UIStyleIcon>()?.Size, $"{image.name} is a Tiny icon");
                Assert.IsNotNull(image.sprite, $"{image.transform.parent.name}/{image.name} has no sprite");
            }
            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                AssertText(label, UIFontSize.Small, settings);
            }
        }

        [TestCase(GlobalFocusRequirement.NoWindowFocused, false, 0, true)]
        [TestCase(GlobalFocusRequirement.NoWindowFocused, true, 2, false)]
        [TestCase(GlobalFocusRequirement.AnyWindowFocused, true, 1, true)]
        [TestCase(GlobalFocusRequirement.AnyWindowFocused, false, 0, false)]
        [TestCase(GlobalFocusRequirement.MultipleWindowsOpen, true, 1, false)]
        [TestCase(GlobalFocusRequirement.MultipleWindowsOpen, true, 2, true)]
        [TestCase(GlobalFocusRequirement.Ignore, false, 0, true)]
        public void PromptFocusRules(GlobalFocusRequirement requirement, bool anyFocused, int cyclable, bool visible)
        {
            Assert.AreEqual(visible, InputPromptVisibilityUI.MeetsGlobalRequirement(requirement, anyFocused, cyclable));
        }

        // --- Scene -------------------------------------------------------------------------------------------

        [Test]
        public void Scene_HasNoSeparateCurrencyPane_AndThePresenterFeedsTheMapWindow()
        {
            string yaml = System.IO.File.ReadAllText(ScenePath);
            Assert.IsFalse(yaml.Contains("CurrencyPane"), "The coins are part of the map window now");
            Assert.IsTrue(yaml.Contains("propertyPath: mapInfo"), "OverworldPlayerStatePresenter.mapInfo is not set in the scene");
            Assert.IsFalse(yaml.Contains("propertyPath: playerStatePanel"), "Leftover override of the old currency field");
        }

        [Test]
        public void Scene_KeepsTheCanvasChildrenOnTheCanvasPlane()
        {
            // The Stats pane sat ~8000 units toward the camera and was clipped on wide screens
            string yaml = System.IO.File.ReadAllText(ScenePath);
            foreach (Match match in Regex.Matches(yaml, @"propertyPath: m_LocalPosition\.z\s+value: (\S+)"))
            {
                Assert.AreEqual(0f, float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), 0.01f,
                    "A LevelSelector UI instance is offset toward the camera");
            }
        }

        [TestCase(WindowPath)]
        [TestCase(PromptsPath)]
        public void SceneInstances_UseThePrefabsLayout(string prefab)
        {
            SceneInstanceLayout.AssertMatchesPrefab(ScenePath, prefab);
            SceneInstanceLayout.AssertNoChildLayoutOverrides(ScenePath, prefab);
        }

        // --- Names -------------------------------------------------------------------------------------------

        [TestCase(1, "Jesterwoods", "Act I - Jesterwoods")]
        [TestCase(2, "Ashen Steppes", "Act II - Ashen Steppes")]
        [TestCase(4, "", "Act IV")]
        public void ActTitle(int act, string name, string expected)
        {
            Assert.AreEqual(expected, OverworldPlayerStatePresenter.FormatActTitle("Act {0} - {1}", act, name));
        }

        [TestCase(1, "I")]
        [TestCase(3, "III")]
        [TestCase(4, "IV")]
        [TestCase(9, "IX")]
        [TestCase(14, "XIV")]
        public void RomanNumerals(int number, string expected)
        {
            Assert.AreEqual(expected, OverworldPlayerStatePresenter.ToRoman(number));
        }

        [Test]
        public void EveryLevel_HasAReadableName()
        {
            // Shown on the map window: placeholder asset-style names ("HolyAltarEncounter", "Camp1") read badly
            var levels = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/Resources" })
                .Select(g => AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(l => l != null)
                .ToList();
            Assert.IsNotEmpty(levels);
            foreach (var level in levels)
            {
                string name = level.DisplayName;
                Assert.IsFalse(string.IsNullOrWhiteSpace(name), $"{level.name} has no DisplayName");
                Assert.IsFalse(char.IsDigit(name[name.Length - 1]), $"{level.name}: \"{name}\" ends with a number");
                Assert.IsFalse(Regex.IsMatch(name, @"^\S*[a-z][A-Z]\S*$"), $"{level.name}: \"{name}\" looks like a CamelCase asset name");
            }
        }
    }
}
