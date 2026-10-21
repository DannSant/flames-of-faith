using Game.UI;
using Game.UI.Navigation;
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
    /// The Settings panel (UI revamp step 3): two columns of Small rows, a Tutorial hints toggle, a close
    /// flow that needs no scene wiring (Back and Cancel ask SettingsPanelController to close), and its place
    /// in the pause menu.
    /// </summary>
    public class SettingsStyleTests
    {
        private const string SettingsPath = "Assets/Prefabs/UI/Menus/SettingsPanel.prefab";
        private const string PausePath = "Assets/Prefabs/UI/Systems/PauseMenu.prefab";

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

        [TestCase("LeftColumn", "GameplaySection", "AudioSection")]
        [TestCase("RightColumn", "GraphicsSection", "LanguageSection")]
        public void Sections_SitInTwoColumns(string column, string first, string second)
        {
            var root = Load(SettingsPath);
            Assert.AreEqual(column, Child(root, first).parent.name);
            Assert.AreEqual(column, Child(root, second).parent.name);
            Assert.Less(Child(root, first).GetSiblingIndex(), Child(root, second).GetSiblingIndex(), "Section order");
        }

        [Test]
        public void EveryText_IsSmallOrTheTitle()
        {
            var settings = Settings();
            foreach (var text in Load(SettingsPath).GetComponentsInChildren<TMP_Text>(true))
            {
                float expected = text.name == "Title" ? settings.BigFontSize : settings.SmallFontSize;
                Assert.AreEqual(expected, text.fontSize, $"{text.name} font size");
                Assert.AreEqual(settings.Font, text.font, $"{text.name} font (no LiberationSans left)");
            }
        }

        [Test]
        public void SectionHeaders_AreBoldYellow()
        {
            foreach (string header in new[] { "GameplayLabel", "AudioLabel", "GraphicsLabel", "LanguageLabel" })
            {
                var style = Child(Load(SettingsPath), header).GetComponent<UIStyleText>();
                Assert.IsNotNull(style, header);
                Assert.IsTrue(style.Bold, $"{header} bold");
                Assert.AreEqual(UIColorRole.Highlight, style.ColorRole, $"{header} color");
            }
        }

        [Test]
        public void Toggles_AreTiny()
        {
            var settings = Settings();
            foreach (var toggle in Load(SettingsPath).GetComponentsInChildren<Toggle>(true))
            {
                if (toggle.GetComponentInParent<TMP_Dropdown>(true) != null) continue; // dropdown list items
                var rect = (RectTransform)toggle.transform;
                Assert.AreEqual(new Vector2(settings.TinyIconSize, settings.TinyIconSize), rect.sizeDelta, $"{toggle.name} size");
            }
        }

        [Test]
        public void TutorialHintsToggle_IsWired()
        {
            var root = Load(SettingsPath);
            var controller = new SerializedObject(root.GetComponentInChildren<SettingsPanelController>(true));
            var toggle = controller.FindProperty("tutorialHintsToggle").objectReferenceValue as Toggle;
            Assert.IsNotNull(toggle, "tutorialHintsToggle not set");
            Assert.AreEqual("TutorialHintsRow", toggle.transform.parent.name);
            Assert.AreEqual("GameplaySection", toggle.transform.parent.parent.name);
        }

        [Test]
        public void BackAndCancel_AskThePanelToClose()
        {
            var root = Load(SettingsPath);
            var controller = root.GetComponentInChildren<SettingsPanelController>(true);

            var back = Child(root, "BackButton").GetComponent<Button>();
            Assert.AreEqual(1, back.onClick.GetPersistentEventCount(), "Back: one call");
            Assert.AreSame(controller, back.onClick.GetPersistentTarget(0));
            Assert.AreEqual(nameof(SettingsPanelController.RequestClose), back.onClick.GetPersistentMethodName(0));

            var window = root.GetComponent<UIWindow>();
            Assert.AreEqual(1, window.OnCancel.GetPersistentEventCount(), "Cancel: one call");
            Assert.AreSame(controller, window.OnCancel.GetPersistentTarget(0));
            Assert.AreEqual(nameof(SettingsPanelController.RequestClose), window.OnCancel.GetPersistentMethodName(0));
        }

        [Test]
        public void MainMenu_NoLongerRetargetsTheCloseCalls()
        {
            // The scene used to point Back and Cancel at MainMenuController.CloseSettings; it now listens to
            // SettingsPanelController.OnCloseRequested in code
            string yaml = File.ReadAllText("Assets/Scenes/MainMenu.unity");
            Assert.IsFalse(SettingsOverride(yaml, "m_OnClick.m_PersistentCalls"), "MainMenu still retargets the Back button");
            Assert.IsFalse(SettingsOverride(yaml, "onCancel.m_PersistentCalls"), "MainMenu still retargets Cancel");
        }

        // Whether the scene overrides this property on an object of the SettingsPanel prefab
        private static bool SettingsOverride(string yaml, string propertyPrefix)
        {
            string guid = AssetDatabase.AssetPathToGUID(SettingsPath);
            return System.Text.RegularExpressions.Regex.IsMatch(yaml,
                $@"target: \{{fileID: \d+, guid: {guid}, type: 3\}}\s+propertyPath: {System.Text.RegularExpressions.Regex.Escape(propertyPrefix)}");
        }

        [Test]
        public void PauseMenu_HasASettingsButtonAndUIMainHasThePanel()
        {
            var pause = Load(PausePath);
            var button = Child(pause, "SettingsButton").GetComponent<Button>();
            Assert.AreEqual(nameof(PauseMenuController.OpenSettings), button.onClick.GetPersistentMethodName(0));
            Assert.AreSame(pause.GetComponent<PauseMenuController>(), button.onClick.GetPersistentTarget(0));

            string uiMain = File.ReadAllText("Assets/Scenes/UIMain.unity");
            string settingsGuid = AssetDatabase.AssetPathToGUID(SettingsPath);
            Assert.IsTrue(uiMain.Contains(settingsGuid), "UIMain has no SettingsPanel instance");
            Assert.IsTrue(uiMain.Contains("propertyPath: settingsWindow"), "PauseMenuController.settingsWindow is not set in UIMain");
        }

        [Test]
        public void MainMenu_SettingsRestsOnScreen()
        {
            // The main menu fades and slides its panels in place (MainMenuController): the instance's own position
            // is where it shows, so it must be on screen at its width
            var settings = Settings();
            var prefab = Load(SettingsPath);
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab.transform, out string guid, out long rootId));
            string yaml = File.ReadAllText("Assets/Scenes/MainMenu.unity");
            var match = System.Text.RegularExpressions.Regex.Match(yaml,
                $@"target: \{{fileID: {rootId}, guid: {guid}, type: 3\}}\s+propertyPath: m_AnchoredPosition\.x\s+value: (\S+)");
            Assert.IsTrue(match.Success, "The MainMenu SettingsPanel instance doesn't set its x");
            float shown = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

            float width = ((RectTransform)prefab.transform).sizeDelta.x;
            // Anchored to the right edge, pivot centered: the panel's right edge sits at shown + width/2
            Assert.LessOrEqual(shown + width * 0.5f, -settings.Margin + 0.01f, "The panel would stick out of the screen");
            Assert.Greater(shown - width * 0.5f, -settings.ReferenceResolution.x, "The panel would stick out on the left");
        }
    }
}
