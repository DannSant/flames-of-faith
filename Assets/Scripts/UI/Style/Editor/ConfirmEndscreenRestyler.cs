using Game.UI.Navigation;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 7: restyles the confirm dialog and the end screen in place.
    /// Confirm dialog: a 400x160 framed window (Small message, OK | Cancel as Small buttons in a row) on a
    /// modal backdrop that dims the screen and blocks clicks behind it. Its instances (MainMenu scene, nested
    /// in PauseMenu and ShopPane) are snapped to the prefab's root layout.
    /// End screen: a center window like the death screen: VICTORY (the Display headline exception, yellow),
    /// a thanks line, and Main Menu in the progress spot, with a UIWindow so the gamepad reaches it.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.ConfirmEndscreenRestyler.Restyle
    /// </summary>
    public static class ConfirmEndscreenRestyler
    {
        public const string ConfirmPath = "Assets/Prefabs/UI/Misc/ConfirmDialogUI.prefab";
        public const string EndscreenPath = "Assets/Prefabs/UI/Misc/Endscreen.prefab";
        public const string ShopPath = "Assets/Prefabs/UI/Shop/ShopPane.prefab";
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        public static readonly Vector2 DialogSize = new Vector2(CenterWindowLayout.Width, 160f);
        // The dialog sits in a different parent in each place (pause window, shop pane, menu canvas): a fixed
        // backdrop centered on it, wider than any canvas, covers the screen from all of them
        public static readonly Vector2 BackdropSize = new Vector2(4000f, 4000f);
        public const int EndscreenPriority = 50;
        private const float SubtitleHeight = 40f;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Confirm Dialog and Endscreen")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[ConfirmEndscreenRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }
            // Writes the new modal backdrop color into the asset
            EditorUtility.SetDirty(settings);

            UpgradeRestyler.EditPrefab(ConfirmPath, root => RestyleDialog(root, settings));
            UpgradeRestyler.EditPrefab(PauseDeathRestyler.PausePath, root => SnapNestedInstances(root, ConfirmPath));
            UpgradeRestyler.EditPrefab(ShopPath, root => SnapNestedInstances(root, ConfirmPath));
            UpgradeRestyler.EditPrefab(EndscreenPath, root => RestyleEndscreen(root, settings));

            UIRevampTools.RunOnScenes(new[] { MainMenuScene }, (scene, path) => UpgradeRestyler.SnapSceneInstances(scene, ConfirmPath));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) =>
            {
                // Revert first: a revert reloads the instance, which would drop the snapped root layout
                bool changed = DropButtonWiringOverrides(scene, EndscreenPath);
                return UpgradeRestyler.SnapSceneInstances(scene, EndscreenPath) || changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[ConfirmEndscreenRestyler] Confirm dialog and end screen restyled.");
        }

        // --- Confirm dialog ---------------------------------------------------------------------------

        private static void RestyleDialog(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float buttonHeight = s.GetButtonHeight(UIButtonSize.Small);

            // Work from the script's references, so the objects keep their identity (and fileIDs) across runs
            var dialog = new SerializedObject(root.GetComponent<ConfirmDialogUI>());
            var panel = (dialog.FindProperty("panel").objectReferenceValue as GameObject).transform as RectTransform;
            var message = dialog.FindProperty("messageText").objectReferenceValue as TextMeshProUGUI;
            var ok = dialog.FindProperty("confirmButton").objectReferenceValue as Button;
            var cancel = dialog.FindProperty("cancelButton").objectReferenceValue as Button;

            Place((RectTransform)root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, DialogSize);

            // The panel the script toggles is the modal backdrop
            Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, BackdropSize);
            var oldFrame = panel.GetComponent<UIStyleFrame>();
            if (oldFrame != null) Object.DestroyImmediate(oldFrame);
            var backdrop = GetOrAdd<Image>(panel.gameObject);
            backdrop.sprite = s.SolidFill;
            backdrop.type = Image.Type.Simple;
            backdrop.color = s.ModalBackdropColor;
            backdrop.raycastTarget = true;

            // The framed window on it
            var window = FindOrCreateImage(panel, "Window");
            window.transform.SetAsFirstSibling();
            Place(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, DialogSize);
            window.raycastTarget = true;
            StyleFrame(window, UIFrameKind.Panel, s);

            // Message: the window's content area above the button row
            message.transform.SetParent(window.transform, false);
            message.name = "Message";
            message.transform.SetSiblingIndex(0);
            Stretch(message.rectTransform, Vector2.zero, Vector2.one, m, m + buttonHeight + m, m, m);
            CenterWindowLayout.PlainText(message, TextAlignmentOptions.Center);
            message.textWrappingMode = TextWrappingModes.Normal;
            message.overflowMode = TextOverflowModes.Overflow;
            message.raycastTarget = false;
            StyleText(message, UIFontSize.Small, false, UIColorRole.Text, s);

            // OK | Cancel, centered in a row (hiding Cancel for an info message centers OK)
            var row = FindOrCreate(window.transform, "Buttons");
            row.SetAsLastSibling();
            row.anchorMin = new Vector2(0f, 0f);
            row.anchorMax = new Vector2(1f, 0f);
            row.pivot = new Vector2(0.5f, 0f);
            row.anchoredPosition = new Vector2(0f, m);
            row.sizeDelta = new Vector2(-2f * m, buttonHeight);
            row.localScale = Vector3.one;
            row.localRotation = Quaternion.identity;
            var layout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = s.Spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            PlaceRowButton((RectTransform)ok.transform, row, s, 0);
            PlaceRowButton((RectTransform)cancel.transform, row, s, 1);

            CenterWindowLayout.UseStandardMarker(panel.GetComponent<UIWindow>());
        }

        private static void PlaceRowButton(RectTransform button, RectTransform row, UISettings s, int index)
        {
            button.SetParent(row, false);
            button.SetSiblingIndex(index);
            button.gameObject.SetActive(true);
            Place(button, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(CenterWindowLayout.ProgressButtonWidth, s.GetButtonHeight(UIButtonSize.Small)));
            var element = button.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = false;
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) StretchFull(label.rectTransform);
            CenterWindowLayout.StyleSmallButton(button, s);
        }

        /// <summary>Gives the nested instances of a prefab inside this prefab their source's root layout.</summary>
        private static void SnapNestedInstances(GameObject root, string sourcePath)
        {
            bool found = false;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (!PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;
                string source = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject));
                if (source != sourcePath) continue;
                MatchPrefabRootLayout(transform.gameObject);
                found = true;
            }
            if (!found) Debug.LogError($"[ConfirmEndscreenRestyler] No {sourcePath} instance in {root.name}");
        }

        // --- End screen -------------------------------------------------------------------------------

        private static void RestyleEndscreen(GameObject root, UISettings s)
        {
            float m = s.Margin;
            CenterWindowLayout.PlaceWindow((RectTransform)root.transform, s);

            var mainPanel = Find(root.transform, "MainPanel");
            CenterWindowLayout.FrameWindow(mainPanel, s);

            var title = Find(mainPanel, "Title").GetComponent<TextMeshProUGUI>();
            var oldColor = title.GetComponent<UIStyleColor>();
            if (oldColor != null) Object.DestroyImmediate(oldColor);
            title.text = "VICTORY";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight, UIFontSize.Display);

            var subtitle = FindOrCreateText(mainPanel, "Subtitle");
            subtitle.transform.SetSiblingIndex(title.transform.GetSiblingIndex() + 1);
            UpgradeRestyler.PlaceTopRow(subtitle.rectTransform, s, m + s.DisplayFontSize + 8f + 4f, SubtitleHeight);
            subtitle.text = "Thanks for playing the demo!";
            CenterWindowLayout.PlainText(subtitle, TextAlignmentOptions.Top);
            subtitle.textWrappingMode = TextWrappingModes.Normal;
            StyleText(subtitle, UIFontSize.Small, false, UIColorRole.Text, s);

            // Main Menu: the progress spot. Wired in code by EndscreenControllerUI, not by persistent calls
            var buttonRect = Find(mainPanel, "MainMenuButton") ?? Find(mainPanel, "ExitButton");
            buttonRect.name = "MainMenuButton";
            var button = buttonRect.GetComponent<Button>();
            while (button.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, 0);
            }
            var label = buttonRect.GetComponentInChildren<TextMeshProUGUI>(true);
            label.text = "Main Menu";
            StretchFull(label.rectTransform);
            CenterWindowLayout.PlaceStackedButton(buttonRect, s, 0);
            buttonRect.SetAsLastSibling();

            // Gamepad: a Primary window that takes focus on open, like the death screen
            GetOrAdd<CanvasGroup>(mainPanel.gameObject);
            var window = GetOrAdd<UIWindow>(mainPanel.gameObject);
            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("role").enumValueIndex = (int)UIWindowRole.Primary;
            windowSo.FindProperty("openMode").enumValueIndex = (int)UIWindowOpenMode.GameObjectActive;
            windowSo.FindProperty("priority").intValue = EndscreenPriority;
            windowSo.FindProperty("focusMode").enumValueIndex = (int)UIWindowFocusMode.AutoFocus;
            windowSo.FindProperty("defaultSelectable").objectReferenceValue = button;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            CenterWindowLayout.UseStandardMarker(window);

            var controller = new SerializedObject(root.GetComponent<EndscreenControllerUI>());
            controller.FindProperty("mainMenuButton").objectReferenceValue = button;
            controller.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// The scene wired the old Exit button to another prefab's PauseMenuController through an onClick
        /// override; the button is now wired in code, so only that override is reverted (never the instance).
        /// </summary>
        private static bool DropButtonWiringOverrides(UnityEngine.SceneManagement.Scene scene, string prefabPath)
        {
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var controller in root.GetComponentsInChildren<EndscreenControllerUI>(true))
                {
                    if (AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(controller)) != prefabPath) continue;
                    foreach (var button in controller.GetComponentsInChildren<Button>(true))
                    {
                        var onClick = new SerializedObject(button).FindProperty("m_OnClick");
                        if (!onClick.prefabOverride) continue;
                        PrefabUtility.RevertPropertyOverride(onClick, InteractionMode.AutomatedAction);
                        changed = true;
                    }
                }
            }
            return changed;
        }
    }
}
