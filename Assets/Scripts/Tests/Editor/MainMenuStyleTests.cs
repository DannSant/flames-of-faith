using Game.Control;
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
    /// Guards the restyled main menu: the menu panel (Big buttons, the saved-run line) and the class select (cards,
    /// details like the skill tree's, Start Run / Back) resting on the right at the Settings window's height, the
    /// scene wiring (no Animator: MainMenuController fades and slides the panels; B goes back from class select),
    /// and the small pure rules.
    /// </summary>
    public class MainMenuStyleTests
    {
        private const string MenuPanelPath = "Assets/Prefabs/UI/Menus/MainMenuPanel.prefab";
        private const string ClassSelectPath = "Assets/Prefabs/UI/Menus/CharacterSelect.prefab";
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

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

        private static Transform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        private static void AssertText(Transform owner, UIFontSize size, UIColorRole? color, UISettings settings)
        {
            var text = owner.GetComponent<TMP_Text>();
            Assert.IsNotNull(text, $"{owner.name} is not a text");
            Assert.AreEqual(size, text.GetComponent<UIStyleText>()?.Size, $"{owner.name} size role");
            Assert.AreEqual(settings.GetFontSize(size), text.fontSize, owner.name);
            if (color.HasValue) Assert.AreEqual(color.Value, text.GetComponent<UIStyleText>().ColorRole, $"{owner.name} color role");
        }

        private static void AssertPanel(GameObject root, float width, UISettings settings)
        {
            var rect = (RectTransform)root.transform;
            Assert.AreEqual(new Vector2(1f, 0.5f), rect.anchorMin, "Anchored to the right edge");
            Assert.AreEqual(new Vector2(1f, 0.5f), rect.pivot);
            Assert.AreEqual(new Vector2(-settings.Margin, settings.Margin), rect.anchoredPosition, "At the margin, the Inventory's row");
            Assert.AreEqual(width, rect.sizeDelta.x);
            Assert.AreEqual(((RectTransform)Load(InventoryPath).transform).sizeDelta.y, rect.sizeDelta.y, "The standard window height");
            Assert.AreEqual(UIFrameKind.Panel, root.GetComponent<UIStyleFrame>()?.Kind, "Framed");

            var title = Child(root, "Title");
            AssertText(title, UIFontSize.Big, UIColorRole.Highlight, settings);
            Assert.IsTrue(title.GetComponent<UIStyleText>().Bold);

            var window = root.GetComponent<UIWindow>();
            Assert.IsNotNull(window);
            var so = new SerializedObject(window);
            Assert.AreEqual((int)UIWindowOpenMode.Manual, so.FindProperty("openMode").enumValueIndex, "The panels stay active and fade in place");
            Assert.IsFalse(window.OverrideMarkerSettings);
        }

        private const string InventoryPath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";

        // --- Menu panel ------------------------------------------------------------------------------------

        [Test]
        public void MenuPanel_IsAFramedColumnOfBigButtons()
        {
            var settings = Settings();
            var root = Load(MenuPanelPath);
            AssertPanel(root, 400f, settings);

            foreach (string name in new[] { "StartButton", "LoadButton", "SkillTreeButton", "SettingsButton", "ExitButton" })
            {
                var button = (RectTransform)Child(root, name);
                Assert.AreEqual(UIButtonSize.Big, button.GetComponent<UIStyleButton>()?.Size, $"{name} is a Big button");
                Assert.AreEqual(settings.GetButtonHeight(UIButtonSize.Big), button.sizeDelta.y, name);
                Assert.AreEqual(settings.BigFontSize, button.GetComponentInChildren<TMP_Text>(true).fontSize, $"{name} label");
            }

            var saveInfo = Child(root, "SaveInfo");
            AssertText(saveInfo, UIFontSize.Small, UIColorRole.Muted, settings);
            Assert.AreEqual(Child(root, "LoadButton").GetSiblingIndex() + 1, saveInfo.GetSiblingIndex(), "The saved run sits under Load Game");
        }

        // --- Class select ----------------------------------------------------------------------------------

        [Test]
        public void ClassSelect_HasCardsDetailsAndStartRun()
        {
            var settings = Settings();
            var root = Load(ClassSelectPath);
            AssertPanel(root, 640f, settings);
            Assert.IsFalse(root.GetComponentsInChildren<Transform>(true).Any(t => t.name == "TooltipPane"), "Leftover tooltip pane");

            var cards = root.GetComponentsInChildren<ClassSelectHoverButton>(true);
            Assert.AreEqual(3, cards.Length);
            foreach (var card in cards)
            {
                Assert.IsNotNull(card.ClassData, $"{card.name} has no class");
                var so = new SerializedObject(card);
                var locked = so.FindProperty("lockedLabel").objectReferenceValue as GameObject;
                var mark = so.FindProperty("selectedMark").objectReferenceValue as GameObject;
                Assert.IsNotNull(locked, $"{card.name}: no Locked note");
                Assert.IsNotNull(mark, $"{card.name}: no selected ring");
                Assert.IsFalse(locked.activeSelf);
                Assert.IsFalse(mark.activeSelf);
                AssertText(locked.transform, UIFontSize.Small, UIColorRole.Negative, settings);
                AssertText(card.transform.Find("Name"), UIFontSize.Small, UIColorRole.Text, settings);
                Assert.AreEqual(card.ClassData.characterName, card.transform.Find("Name").GetComponent<TMP_Text>().text, "The card names its class");
                Assert.IsNotNull(card.transform.Find("Portrait")?.GetComponent<Image>()?.sprite, $"{card.name} has no portrait");
            }

            AssertText(Child(root, "ClassName"), UIFontSize.Big, null, settings);
            AssertText(Child(root, "ClassDesc"), UIFontSize.Small, null, settings);
            AssertText(Child(root, "WeaponHeader"), UIFontSize.Small, UIColorRole.Highlight, settings);
            AssertText(Child(root, "WeaponName"), UIFontSize.Small, UIColorRole.None, settings);
            AssertText(Child(root, "WeaponDesc"), UIFontSize.Small, null, settings);
            AssertText(Child(root, "Footer"), UIFontSize.Small, UIColorRole.None, settings);

            var start = (RectTransform)Child(root, "StartRunButton");
            var back = (RectTransform)Child(root, "Back");
            Assert.AreEqual(UIButtonSize.Small, start.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(UIButtonSize.Small, back.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(new Vector2(0.5f, 0f), start.anchorMin, "Start Run in the progress spot");
            Assert.AreEqual(settings.Margin, start.anchoredPosition.y, 0.01f);
            Assert.Greater(back.anchoredPosition.y, start.anchoredPosition.y, "Back above Start Run");
            Assert.AreEqual(UIButtonSize.Small, Child(root, "SkillTreeButton").GetComponent<UIStyleButton>()?.Size);
        }

        // --- Scene -------------------------------------------------------------------------------------------

        [Test]
        public void Scene_HasNoAnimator_AndBGoesBackFromClassSelect()
        {
            string yaml = System.IO.File.ReadAllText(ScenePath);
            string controllerGuid = AssetDatabase.AssetPathToGUID("Assets/Animation/UI/MainMenuAnimator.controller");
            Assert.IsTrue(string.IsNullOrEmpty(controllerGuid) || !yaml.Contains(controllerGuid), "The menu still uses the old Animator");
            Assert.IsFalse(yaml.Contains("--- !u!95 "), "An Animator is left in the main menu");
            // Both are overrides on the class select's instance (the controller is a scene object)
            StringAssert.IsMatch(@"propertyPath: onCancel\.m_PersistentCalls\.m_Calls\.Array\.data\[0\]\.m_MethodName\s+value: SwitchFromCharacterSelectToMainPanel",
                yaml, "B on the class select is not wired");
            StringAssert.IsMatch(@"m_OnClick\.m_PersistentCalls\.m_Calls\.Array\.data\[0\]\.m_MethodName\s+value: OpenSkillTreeForSelected",
                yaml, "The class select's Skill Tree button is not wired");
        }

        [Test]
        public void Scene_WiresTheController()
        {
            string yaml = System.IO.File.ReadAllText(ScenePath);
            foreach (string field in new[] { "startRunButton", "skillTree", "classFooterText", "saveInfoText", "classesByPlayerIndex" })
            {
                StringAssert.Contains($"{field}:", yaml, $"MainMenuController.{field} is not serialized in the scene");
            }
            // The classes in GameSession.SelectedPlayerIndex order (the player prefabs: Warrior, Archer, Augur)
            string[] expected = { "Warrior", "Archer", "Augur" };
            var guids = expected.Select(n => AssetDatabase.AssetPathToGUID($"Assets/Resources/CharacterData/{n}.asset")).ToArray();
            int start = yaml.IndexOf("classesByPlayerIndex:", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(start, 0);
            int last = -1;
            foreach (string guid in guids)
            {
                int at = yaml.IndexOf(guid, start, System.StringComparison.Ordinal);
                Assert.Greater(at, last, "classesByPlayerIndex is not Warrior, Archer, Augur in that order");
                last = at;
            }
        }

        [TestCase(MenuPanelPath)]
        [TestCase(ClassSelectPath)]
        public void SceneInstances_UseThePrefabsLayout(string prefab)
        {
            SceneInstanceLayout.AssertMatchesPrefab(ScenePath, prefab);
            SceneInstanceLayout.AssertNoChildLayoutOverrides(ScenePath, prefab);
        }

        // --- Pure ----------------------------------------------------------------------------------------

        [TestCase("Warrior", 1, 7, "Warrior - Act I - Level 7")]
        [TestCase("Augur", 2, 12, "Augur - Act II - Level 12")]
        [TestCase("Archer", 0, 0, "Archer - Act I - Level 1")]
        public void SaveInfo(string className, int act, int level, string expected)
        {
            Assert.AreEqual(expected, MainMenuController.FormatSaveInfo("{0} - Act {1} - Level {2}", className, act, level));
        }

        [Test]
        public void Details_ShowTheHoveredClass_ElseTheChosenOne()
        {
            var warrior = ScriptableObject.CreateInstance<CharacterClassData>();
            var archer = ScriptableObject.CreateInstance<CharacterClassData>();
            try
            {
                Assert.AreSame(archer, MainMenuController.ClassToShow(archer, warrior));
                Assert.AreSame(warrior, MainMenuController.ClassToShow(null, warrior));
                Assert.IsNull(MainMenuController.ClassToShow(null, null));
            }
            finally
            {
                Object.DestroyImmediate(warrior);
                Object.DestroyImmediate(archer);
            }
        }
    }
}
