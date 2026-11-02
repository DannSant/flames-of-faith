using Game.Cutscenes;
using Game.UI;
using Game.UI.Chronicle;
using Game.UI.Cutscenes;
using Game.UI.InputPrompts;
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
    /// Guards the cutscene window (text band, Next, hold to skip, drawn above the loading screen, in the Bootstrapper)
    /// and the Pilgrim's Chronicle (a standard main menu panel with tabs, wired in the scene).
    /// </summary>
    public class CutsceneStyleTests
    {
        private const string PlayerPath = "Assets/Prefabs/UI/Cutscenes/CutscenePlayer.prefab";
        private const string ChroniclePath = "Assets/Prefabs/UI/Menus/PilgrimsChronicle.prefab";
        private const string MenuPanelPath = "Assets/Prefabs/UI/Menus/MainMenuPanel.prefab";
        private const string InventoryPath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
        private const string BootstrapperScene = "Assets/Scenes/Bootstrapper.unity";

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

        private static void AssertText(Transform owner, UIFontSize size, UIColorRole color, UISettings settings)
        {
            var text = owner.GetComponent<TMP_Text>();
            Assert.IsNotNull(text, $"{owner.name} is not a text");
            Assert.AreEqual(size, text.GetComponent<UIStyleText>()?.Size, $"{owner.name} size role");
            Assert.AreEqual(settings.GetFontSize(size), text.fontSize, owner.name);
            Assert.AreEqual(color, text.GetComponent<UIStyleText>().ColorRole, $"{owner.name} color role");
        }

        // --- Cutscene window -------------------------------------------------------------------------------

        [Test]
        public void Player_IsARootCanvasAboveTheLoadingScreen()
        {
            var root = Load(PlayerPath);
            Assert.IsNotNull(root.GetComponent<CutscenePlayer>());
            Assert.IsNotNull(root.GetComponent<CutsceneDatabaseProvider>());
            Assert.IsNotNull(root.GetComponent<GraphicRaycaster>(), "Clicks need a raycaster");

            var canvas = root.GetComponent<Canvas>();
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);
            Assert.Greater(canvas.sortingOrder, 99, "Under the loading screen, the scene would play behind it");

            var settings = Settings();
            var scaler = root.GetComponent<CanvasScaler>();
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(settings.ReferenceResolution, scaler.referenceResolution);
            Assert.AreEqual(settings.MatchWidthOrHeight, scaler.matchWidthOrHeight);

            Assert.IsFalse(Child(root, "Window").gameObject.activeSelf, "The window starts closed");
        }

        [Test]
        public void Window_TakesFocusFromEverything()
        {
            var window = Child(Load(PlayerPath), "Window").GetComponent<UIWindow>();
            Assert.IsNotNull(window);
            Assert.AreEqual(UIWindowRole.Primary, window.Role);
            Assert.AreEqual(UIWindowFocusMode.AutoFocus, window.FocusMode);
            Assert.Greater(window.Priority, 102, "Above the highest window (the pause menu's Settings)");
            Assert.IsTrue(window.BlocksGameplayInput);
            Assert.IsNotNull(window.GetComponent<CutsceneWindowUI>());

            var so = new SerializedObject(window);
            Assert.AreEqual("NextButton", (so.FindProperty("defaultSelectable").objectReferenceValue as Selectable)?.name);
            Assert.AreEqual(0, window.OnCancel.GetPersistentEventCount(), "A tap of B must not leave a half-played scene");
        }

        [Test]
        public void Page_HasTheTextInABandOverTheBottomQuarter()
        {
            var settings = Settings();
            var root = Load(PlayerPath);

            var band = (RectTransform)Child(root, "Band");
            Assert.AreEqual(Vector2.zero, band.anchorMin);
            Assert.AreEqual(new Vector2(1f, 0.25f), band.anchorMax);
            Assert.AreEqual(settings.ModalBackdropColor, band.GetComponent<Image>().color);

            var text = Child(root, "Text");
            Assert.IsTrue(text.IsChildOf(band));
            AssertText(text, UIFontSize.Big, UIColorRole.Text, settings);
            var tmp = text.GetComponent<TMP_Text>();
            Assert.AreEqual(TextWrappingModes.Normal, tmp.textWrappingMode);

            var image = Child(root, "Image").GetComponent<Image>();
            Assert.IsTrue(image.preserveAspect, "Other aspect ratios letterbox instead of stretching the art");
            Assert.IsFalse(image.raycastTarget);
        }

        [Test]
        public void Controls_FollowTheStandard()
        {
            var settings = Settings();
            var root = Load(PlayerPath);
            var window = Child(root, "Window");

            var next = Child(root, "NextButton");
            Assert.AreEqual(UIButtonSize.Small, next.GetComponent<UIStyleButton>()?.Size);
            Assert.IsNotNull(next.GetComponent<Button>());
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Automatic, next.GetComponent<Button>().navigation.mode, "The focus system needs one navigable Selectable");
            Assert.IsFalse(next.IsChildOf(Child(root, "Page")), "Controls stay put while the pages fade");

            var clickArea = Child(root, "ClickArea");
            Assert.AreEqual(0, clickArea.GetSiblingIndex(), "Behind the page and the controls");
            Assert.IsTrue(clickArea.GetComponent<Image>().raycastTarget, "Blocks the windows behind, and a click advances");
            Assert.AreEqual(UnityEngine.UI.Navigation.Mode.None, clickArea.GetComponent<Button>().navigation.mode);

            var skip = Child(root, "SkipBar");
            Assert.IsNotNull(skip.GetComponent<HoldButtonUI>());
            Assert.AreEqual(UIFrameKind.Bar, skip.GetComponent<UIStyleFrame>()?.Kind);
            AssertText(skip.Find("Label"), UIFontSize.Small, UIColorRole.Text, settings);
            Assert.AreEqual("Hold to skip", skip.Find("Label").GetComponent<TMP_Text>().text);
            Assert.AreEqual(Image.Type.Filled, skip.Find("Fill").GetComponent<Image>().type);
            Assert.AreEqual(UIColorRole.Highlight, skip.Find("Fill").GetComponent<UIStyleColor>()?.ColorRole);
            Assert.AreEqual(UIIconSize.Tiny, skip.Find("Glyph").GetComponent<UIStyleIcon>()?.Size);
            var prompt = new SerializedObject(skip.Find("Glyph").GetComponent<InputPromptIconUI>());
            Assert.IsNotNull(prompt.FindProperty("action").objectReferenceValue, "The glyph needs the Cancel action");

            var ui = new SerializedObject(window.GetComponent<CutsceneWindowUI>());
            foreach (string field in new[] { "windowGroup", "pageGroup", "pageImage", "pageText", "clickArea", "nextButton", "nextLabel", "skipBar", "skipFill" })
            {
                Assert.IsNotNull(ui.FindProperty(field).objectReferenceValue, $"CutsceneWindowUI.{field} is not wired");
            }
            Assert.GreaterOrEqual(ui.FindProperty("skipHoldTime").floatValue, 0.5f, "Too short a hold can fire by accident");
            Assert.IsNotNull(new SerializedObject(root.GetComponent<CutscenePlayer>()).FindProperty("window").objectReferenceValue);
        }

        [Test]
        public void Bootstrapper_HasThePlayer_AtTheRoot()
        {
            string guid = AssetDatabase.AssetPathToGUID(PlayerPath);
            string yaml = System.IO.File.ReadAllText(BootstrapperScene);
            StringAssert.Contains($"m_SourcePrefab: {{fileID: 100100000, guid: {guid}", yaml, "No CutscenePlayer in the Bootstrapper scene");
        }

        // --- Pilgrim's Chronicle ---------------------------------------------------------------------------

        [Test]
        public void Chronicle_IsAStandardMenuPanel()
        {
            var settings = Settings();
            var root = Load(ChroniclePath);
            var rect = (RectTransform)root.transform;
            Assert.AreEqual(new Vector2(1f, 0.5f), rect.anchorMin);
            Assert.AreEqual(new Vector2(-settings.Margin, settings.Margin), rect.anchoredPosition);
            Assert.AreEqual(640f, rect.sizeDelta.x);
            Assert.AreEqual(((RectTransform)Load(InventoryPath).transform).sizeDelta.y, rect.sizeDelta.y, "The standard window height");
            Assert.AreEqual(UIFrameKind.Panel, root.GetComponent<UIStyleFrame>()?.Kind);

            var title = Child(root, "Title");
            AssertText(title, UIFontSize.Big, UIColorRole.Highlight, settings);
            Assert.IsTrue(title.GetComponent<UIStyleText>().Bold);
            Assert.AreEqual("Pilgrim's Chronicle", title.GetComponent<TMP_Text>().text);

            var window = root.GetComponent<UIWindow>();
            var so = new SerializedObject(window);
            Assert.AreEqual((int)UIWindowOpenMode.Manual, so.FindProperty("openMode").enumValueIndex, "The menu's panels stay active and fade in place");
            Assert.IsFalse(window.OverrideMarkerSettings);
            Assert.AreEqual(1, window.OnCancel.GetPersistentEventCount(), "B closes it");
            Assert.AreEqual(nameof(ChronicleUI.RequestClose), window.OnCancel.GetPersistentMethodName(0));

            var back = (RectTransform)Child(root, "Back");
            Assert.AreEqual(UIButtonSize.Small, back.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(new Vector2(0.5f, 0f), back.anchorMin, "Back in the progress spot");
            Assert.AreEqual(settings.Margin, back.anchoredPosition.y, 0.01f);
        }

        [Test]
        public void Chronicle_HasRoomForMoreTabs_AndAHiddenRowTemplate()
        {
            var root = Load(ChroniclePath);
            var ui = new SerializedObject(root.GetComponent<ChronicleUI>());
            var tabs = ui.FindProperty("tabs");
            Assert.AreEqual(1, tabs.arraySize);
            var tab = tabs.GetArrayElementAtIndex(0);
            Assert.IsNotNull(tab.FindPropertyRelative("button").objectReferenceValue);
            Assert.IsNotNull(tab.FindPropertyRelative("label").objectReferenceValue);
            Assert.IsNotNull(tab.FindPropertyRelative("page").objectReferenceValue);
            Assert.AreEqual("Tabs", Child(root, "ScenesTab").parent.name, "Tabs sit in their own row");

            foreach (string field in new[] { "scrollRect", "rowTemplate", "emptyText", "backButton" })
            {
                Assert.IsNotNull(ui.FindProperty(field).objectReferenceValue, $"ChronicleUI.{field} is not wired");
            }

            var template = (ui.FindProperty("rowTemplate").objectReferenceValue as ChronicleRowUI).gameObject;
            Assert.IsFalse(template.activeSelf, "The template is never a row");
            Assert.AreEqual(UIButtonSize.Small, template.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(UIColorRole.Muted, Child(root, "EmptyText").GetComponent<UIStyleText>().ColorRole);

            foreach (var graphic in Child(root, "Rows").GetComponentsInChildren<MaskableGraphic>(true))
            {
                Assert.IsTrue(graphic.maskable, $"{graphic.name} would draw outside the list");
            }
        }

        [Test]
        public void MenuPanel_HasTheChronicleButton()
        {
            var settings = Settings();
            var button = (RectTransform)Child(Load(MenuPanelPath), "ChronicleButton");
            Assert.AreEqual(UIButtonSize.Big, button.GetComponent<UIStyleButton>()?.Size);
            Assert.AreEqual(settings.GetButtonHeight(UIButtonSize.Big), button.sizeDelta.y);
            Assert.AreEqual("Pilgrim's Chronicle", button.GetComponentInChildren<TMP_Text>(true).text);
            var onClick = button.GetComponent<Button>().onClick;
            Assert.AreEqual(1, onClick.GetPersistentEventCount());
            Assert.IsNull(onClick.GetPersistentTarget(0), "The target is the scene's MainMenuController (Scene_WiresTheChronicle)");
        }

        [Test]
        public void Scene_WiresTheChronicle()
        {
            string yaml = System.IO.File.ReadAllText(MainMenuScene);
            StringAssert.IsMatch(@"chronicleWindow: \{fileID: [1-9-]", yaml, "MainMenuController.chronicleWindow is not wired");
            StringAssert.IsMatch(@"m_OnClick\.m_PersistentCalls\.m_Calls\.Array\.data\[0\]\.m_MethodName\s+value: OpenChronicle", yaml, "The Chronicle button is not wired");
            StringAssert.Contains(nameof(MainMenuController.OpenChronicle), yaml);
        }

        [Test]
        public void SceneInstance_UsesThePrefabsLayout()
        {
            SceneInstanceLayout.AssertMatchesPrefab(MainMenuScene, ChroniclePath);
            SceneInstanceLayout.AssertNoChildLayoutOverrides(MainMenuScene, ChroniclePath);
        }
    }
}
