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
    /// Guards the restyled confirm dialog: a 400x160 framed window with a Small message and OK | Cancel as
    /// Small buttons in a row, on a modal backdrop that blocks clicks behind it, and its gamepad window.
    /// Its three instances (MainMenu, PauseMenu, ShopPane) must keep the prefab's layout and draw on top.
    /// </summary>
    public class ConfirmDialogStyleTests
    {
        private const string DialogPath = "Assets/Prefabs/UI/Misc/ConfirmDialogUI.prefab";
        private const string PausePath = "Assets/Prefabs/UI/Systems/PauseMenu.prefab";
        private const string ShopPath = "Assets/Prefabs/UI/Shop/ShopPane.prefab";

        private static UISettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/Resources/UI/UISettings.asset");
            Assert.IsNotNull(settings, "Missing UISettings");
            return settings;
        }

        private static GameObject LoadDialog()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DialogPath);
            Assert.IsNotNull(prefab, $"Missing {DialogPath}");
            return prefab;
        }

        private static T Ref<T>(string property) where T : Object
        {
            var value = new SerializedObject(LoadDialog().GetComponent<ConfirmDialogUI>()).FindProperty(property).objectReferenceValue as T;
            Assert.IsNotNull(value, $"ConfirmDialogUI.{property} is not set");
            return value;
        }

        private static RectTransform Child(GameObject root, string name)
        {
            var child = root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);
            Assert.IsNotNull(child, $"{root.name} has no {name}");
            return child;
        }

        [Test]
        public void Root_IsA400x160CenteredWindow()
        {
            var rect = (RectTransform)LoadDialog().transform;
            Assert.AreEqual(new Vector2(0.5f, 0.5f), rect.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), rect.anchorMax);
            Assert.AreEqual(Vector2.zero, rect.anchoredPosition);
            Assert.AreEqual(new Vector2(400f, 160f), rect.sizeDelta);
        }

        [Test]
        public void Window_IsFramedAndTheSizeOfTheDialog()
        {
            var window = Child(LoadDialog(), "Window");
            Assert.AreEqual(UIFrameKind.Panel, window.GetComponent<UIStyleFrame>()?.Kind, "Panel frame");
            Assert.AreEqual(Image.Type.Sliced, window.GetComponent<Image>().type);
            Assert.AreEqual(((RectTransform)LoadDialog().transform).sizeDelta, window.sizeDelta);
        }

        [Test]
        public void Panel_IsAModalBackdropCoveringTheScreen()
        {
            var settings = Settings();
            var panel = (RectTransform)Ref<GameObject>("panel").transform;
            var backdrop = panel.GetComponent<Image>();
            Assert.IsNotNull(backdrop, "The panel has no backdrop image");
            Assert.IsTrue(backdrop.raycastTarget, "The backdrop must block clicks on the windows behind the dialog");
            Assert.AreEqual(settings.ModalBackdropColor, backdrop.color, "Backdrop color comes from UISettings");
            Assert.IsNull(panel.GetComponent<UIStyleFrame>(), "The backdrop is not a frame");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), panel.anchorMin, "Centered on the dialog");
            Assert.GreaterOrEqual(panel.sizeDelta.x, 3840f, "Covers the screen from any parent");
            Assert.GreaterOrEqual(panel.sizeDelta.y, 2160f, "Covers the screen from any parent");
            Assert.AreEqual(0, panel.Find("Window").GetSiblingIndex(), "The window draws over the backdrop");
        }

        [Test]
        public void Message_IsSmallCenteredWrappingText()
        {
            var settings = Settings();
            var message = Ref<TextMeshProUGUI>("messageText");
            Assert.AreEqual("Window", message.transform.parent.name, "The message is inside the framed window");
            var style = message.GetComponent<UIStyleText>();
            Assert.AreEqual(UIFontSize.Small, style?.Size);
            Assert.AreEqual(UIColorRole.Text, style?.ColorRole);
            Assert.AreEqual(settings.SmallFontSize, message.fontSize);
            Assert.AreEqual(TextAlignmentOptions.Center, message.alignment);
            Assert.AreEqual(TextWrappingModes.Normal, message.textWrappingMode);
            Assert.IsFalse(message.enableAutoSizing);
            Assert.IsFalse(message.raycastTarget);
        }

        [Test]
        public void Buttons_AreSmallAndSideBySideOkFirst()
        {
            var settings = Settings();
            var ok = Ref<Button>("confirmButton");
            var cancel = Ref<Button>("cancelButton");
            var row = ok.transform.parent;
            Assert.AreEqual("Buttons", row.name);
            Assert.AreSame(row, cancel.transform.parent, "OK and Cancel share the row");
            Assert.Less(ok.transform.GetSiblingIndex(), cancel.transform.GetSiblingIndex(), "OK on the left");

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            Assert.IsNotNull(layout, "The row lays the buttons out, so hiding Cancel centers OK");
            Assert.AreEqual(settings.Spacing, layout.spacing);
            Assert.AreEqual(TextAnchor.MiddleCenter, layout.childAlignment);

            foreach (var button in new[] { ok, cancel })
            {
                var rect = (RectTransform)button.transform;
                Assert.AreEqual(UIButtonSize.Small, button.GetComponent<UIStyleButton>()?.Size, $"{button.name} size role");
                Assert.AreEqual(new Vector2(160f, settings.GetButtonHeight(UIButtonSize.Small)), rect.sizeDelta, $"{button.name} size");
                Assert.AreEqual(settings.SmallFontSize, button.GetComponentInChildren<TMP_Text>(true).fontSize, $"{button.name} label");
                Assert.IsTrue(button.gameObject.activeSelf, $"{button.name} is active in the prefab (info mode hides Cancel at runtime)");
            }
        }

        [Test]
        public void Window_TakesGamepadFocusOnCancel()
        {
            var panel = Ref<GameObject>("panel");
            var window = panel.GetComponent<UIWindow>();
            Assert.IsNotNull(window, "The dialog panel has no UIWindow");
            var so = new SerializedObject(window);
            Assert.AreEqual((int)UIWindowRole.Primary, so.FindProperty("role").enumValueIndex);
            Assert.AreEqual((int)UIWindowFocusMode.AutoFocus, so.FindProperty("focusMode").enumValueIndex);
            Assert.Greater(so.FindProperty("priority").intValue, 100, "Above the pause menu it opens from");
            Assert.AreSame(Ref<Button>("cancelButton"), so.FindProperty("defaultSelectable").objectReferenceValue, "Gamepad starts on Cancel");
            Assert.IsFalse(window.OverrideMarkerSettings);
            Assert.IsNull(so.FindProperty("focusFrame").objectReferenceValue);

            var calls = so.FindProperty("onCancel.m_PersistentCalls.m_Calls");
            Assert.AreEqual(1, calls.arraySize, "B cancels the dialog");
            Assert.AreEqual("Cancel", calls.GetArrayElementAtIndex(0).FindPropertyRelative("m_MethodName").stringValue);
        }

        [TestCase("Assets/Scenes/MainMenu.unity")]
        [TestCase(PausePath)]
        [TestCase(ShopPath)]
        public void Instance_UsesThePrefabsLayout(string containerPath)
        {
            SceneInstanceLayout.AssertMatchesPrefab(containerPath, DialogPath);
        }

        [TestCase(PausePath)]
        [TestCase(ShopPath)]
        public void NestedInstance_DrawsOnTop(string containerPath)
        {
            var container = AssetDatabase.LoadAssetAtPath<GameObject>(containerPath);
            var dialog = container.GetComponentInChildren<ConfirmDialogUI>(true);
            Assert.IsNotNull(dialog, $"{containerPath} has no ConfirmDialogUI");
            var transform = dialog.transform;
            Assert.AreEqual(transform.parent.childCount - 1, transform.GetSiblingIndex(), "The dialog is the last sibling, so it draws over its window");
        }
    }
}
