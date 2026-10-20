using Game.UI.Navigation;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 3: restyles the Settings panel in place as a two-column center window and puts it
    /// in the pause menu.
    /// - Left column: Gameplay (auto-attack, lock-on mode, tutorial hints) and Audio. Right column:
    ///   Graphics and Language. Section headers Small, bold, yellow; rows 32 high, label left, control right.
    /// - Back (progress spot) and Cancel call SettingsPanelController.RequestClose; the main menu and the
    ///   pause menu each close it themselves, so the prefab has no scene-specific wiring.
    /// - UIMain gets an instance opened by the pause menu's new Settings button; the main menu's slide-in
    ///   animation is adjusted to the new width.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.SettingsRestyler.Restyle
    /// </summary>
    public static class SettingsRestyler
    {
        public const string SettingsPath = "Assets/Prefabs/UI/Menus/SettingsPanel.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
        private static readonly string[] SlideClips = { "Assets/Animation/UI/SettingsShow.anim", "Assets/Animation/UI/HideSettings.anim" };
        // The old on-screen x of the slide (anchor right edge, 535 wide panel)
        private const float OldShownX = -276.1f;

        public const float Width = 640f;
        public const float RowHeight = 32f;
        private const float HeaderHeight = 18f;
        private const float DropdownWidth = 150f;
        private const float SliderWidth = 135f;
        private const float SliderHeight = 18f;
        private const float ControlGap = 8f;
        // Above the pause menu (100) and its confirm dialog (101)
        private const int PauseSettingsPriority = 102;

        public static readonly string[] LeftSections = { "GameplaySection", "AudioSection" };
        public static readonly string[] RightSections = { "GraphicsSection", "LanguageSection" };

        private static readonly Dictionary<string, (string header, string[] rows)> Sections = new()
        {
            ["GameplaySection"] = ("Gameplay", new[] { "AutoAttackToggleRow", "LockonToggleRow", "TutorialHintsRow" }),
            ["AudioSection"] = ("Audio", new[] { "MasterVolume", "MusicVolume", "SFXVolume" }),
            ["GraphicsSection"] = ("Graphics", new[] { "ResolutionRow", "QualityRow", "FullscreenRow", "vsyncRow" }),
            ["LanguageSection"] = ("Language", new[] { "LanguageTextRow" }),
        };

        /// <summary>The shown x of the slide-in in the main menu (anchored to the right edge).</summary>
        public static float MainMenuShownX(UISettings s) => -(s.Margin + Width * 0.5f);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Settings")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[SettingsRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(SettingsPath, root => RestylePanel(root, settings));
            UpgradeRestyler.EditPrefab(PauseDeathRestyler.PausePath, root => AddPauseSettingsButton(root, settings));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) => PlaceInUIMain(scene, settings));
            UIRevampTools.RunOnScenes(new[] { MainMenuScene }, (scene, path) => FixMainMenuInstance(scene, settings));
            AdjustSlideClips(settings);

            AssetDatabase.SaveAssets();
            Debug.Log("[SettingsRestyler] Settings restyled and added to the pause menu.");
        }

        // --- Panel --------------------------------------------------------------------------------------

        private static void RestylePanel(GameObject root, UISettings s)
        {
            float m = s.Margin;
            var rootRect = (RectTransform)root.transform;
            CenterWindowLayout.PlaceWindow(rootRect, s, Width);
            var frame = root.GetComponent<Image>();
            frame.raycastTarget = true;
            StyleFrame(frame, UIFrameKind.Panel, s);

            var controller = root.GetComponentInChildren<SettingsPanelController>(true);
            var window = root.GetComponent<UIWindow>();
            CenterWindowLayout.UseStandardMarker(window);
            ReplacePersistentCalls(window.OnCancel, controller.RequestClose);

            var title = (Find(root.transform, "Tittle") ?? Find(root.transform, "Title")).GetComponent<TextMeshProUGUI>();
            title.name = "Title";
            title.text = "Settings";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Text);

            // Two columns between the title row and the Back button
            float top = m + InventoryRestyler.TitleRowHeight + m;
            float bottom = CenterWindowLayout.ButtonsTop(s, 1) + m;
            var left = Column(root.transform, "LeftColumn", new Vector2(0f, 0f), new Vector2(0.5f, 1f), m, bottom, m * 0.5f, top, s);
            var right = Column(root.transform, "RightColumn", new Vector2(0.5f, 0f), new Vector2(1f, 1f), m * 0.5f, bottom, m, top, s);

            EnsureTutorialHintsRow(root, controller, s);

            for (int i = 0; i < LeftSections.Length; i++) RestyleSection(root, LeftSections[i], left, i, s);
            for (int i = 0; i < RightSections.Length; i++) RestyleSection(root, RightSections[i], right, i, s);

            var back = (Find(root.transform, "Back (1)") ?? Find(root.transform, "BackButton"));
            back.name = "BackButton";
            back.GetComponentInChildren<TextMeshProUGUI>(true).text = "Back";
            CenterWindowLayout.PlaceStackedButton(back, s, 0);
            back.SetAsLastSibling();
            ReplacePersistentCalls(back.GetComponent<Button>().onClick, controller.RequestClose);

            // Nothing in the panel keeps the old fonts (dropdown lists used LiberationSans)
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.GetComponent<UIStyleText>() == null)
                {
                    StyleText(text, UIFontSize.Small, false, UIColorRole.Text, s);
                }
            }
        }

        private static RectTransform Column(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            float left, float bottom, float right, float top, UISettings s)
        {
            var column = FindOrCreate(parent, name);
            Stretch(column, anchorMin, anchorMax, left, bottom, right, top);
            var layout = GetOrAdd<VerticalLayoutGroup>(column.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = s.Margin;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return column;
        }

        private static void RestyleSection(GameObject root, string sectionName, RectTransform column, int index, UISettings s)
        {
            var section = root.GetComponentsInChildren<RectTransform>(true).First(t => t.name == sectionName);
            section.SetParent(column, false);
            section.SetSiblingIndex(index);
            var background = section.GetComponent<Image>();
            if (background != null) background.enabled = false;

            var layout = GetOrAdd<VerticalLayoutGroup>(section.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = s.Spacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var (headerText, rows) = Sections[sectionName];
            var header = section.GetComponentsInChildren<TextMeshProUGUI>(true).First(t => t.transform.parent == section);
            header.text = headerText;
            header.transform.SetSiblingIndex(0);
            SetRowHeight(header.gameObject, HeaderHeight);
            CenterWindowLayout.PlainText(header, TextAlignmentOptions.MidlineLeft);
            StyleText(header, UIFontSize.Small, false, UIColorRole.Highlight, s, bold: true);

            for (int i = 0; i < rows.Length; i++)
            {
                var row = Find(section, rows[i]);
                row.SetSiblingIndex(i + 1);
                RestyleRow(row, s);
            }
        }

        private static void RestyleRow(RectTransform row, UISettings s)
        {
            SetRowHeight(row.gameObject, RowHeight);
            var oldLayout = row.GetComponent<HorizontalLayoutGroup>();
            if (oldLayout != null) Object.DestroyImmediate(oldLayout);

            var label = row.GetComponentsInChildren<TextMeshProUGUI>(true).First(t => t.GetComponentInParent<TMP_Dropdown>(true) == null);
            var dropdown = row.GetComponentInChildren<TMP_Dropdown>(true);
            var slider = row.GetComponentInChildren<Slider>(true);
            var toggle = row.GetComponentInChildren<Toggle>(true);

            float controlWidth;
            if (dropdown != null)
            {
                controlWidth = DropdownWidth;
                var rect = (RectTransform)dropdown.transform;
                Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(DropdownWidth, s.TinyIconSize));
                StyleDropdown(dropdown, s);
            }
            else if (slider != null)
            {
                controlWidth = SliderWidth;
                Place((RectTransform)slider.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(SliderWidth, SliderHeight));
            }
            else
            {
                controlWidth = s.TinyIconSize;
                var rect = (RectTransform)toggle.transform;
                Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, Vector2.one * s.TinyIconSize);
                StyleIcon(toggle.gameObject, UIIconSize.Tiny, s);
            }

            Stretch(label.rectTransform, Vector2.zero, Vector2.one, 0f, 0f, controlWidth + ControlGap, 0f);
            CenterWindowLayout.PlainText(label, TextAlignmentOptions.MidlineLeft);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.fontStyle = FontStyles.Normal;
            StyleText(label, UIFontSize.Small, false, UIColorRole.Text, s);
        }

        private static void StyleDropdown(TMP_Dropdown dropdown, UISettings s)
        {
            var box = dropdown.GetComponent<Image>();
            box.sprite = s.SlotFrame;
            box.type = Image.Type.Sliced;
            box.color = Color.white;

            if (dropdown.captionText != null)
            {
                Stretch(dropdown.captionText.rectTransform, Vector2.zero, Vector2.one, 8f, 0f, 24f, 0f);
                CenterWindowLayout.PlainText(dropdown.captionText, TextAlignmentOptions.MidlineLeft);
                StyleText(dropdown.captionText, UIFontSize.Small, false, UIColorRole.Text, s);
            }
            if (dropdown.itemText != null)
            {
                CenterWindowLayout.PlainText(dropdown.itemText, TextAlignmentOptions.MidlineLeft);
                StyleText(dropdown.itemText, UIFontSize.Small, false, UIColorRole.Text, s);
            }

            var arrow = Find(dropdown.transform, "Arrow");
            if (arrow != null)
            {
                Place(arrow, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(14f, 14f));
                arrow.GetComponent<Image>().color = s.TextColor;
            }

            if (dropdown.template != null)
            {
                var list = dropdown.template.GetComponent<Image>();
                if (list != null)
                {
                    list.sprite = s.SlotFrame;
                    list.type = Image.Type.Sliced;
                    list.color = Color.white;
                }
                foreach (var item in dropdown.template.GetComponentsInChildren<Toggle>(true))
                {
                    // Item rows: transparent, a soft highlight on hover/selection
                    var background = item.targetGraphic as Image;
                    if (background != null) background.color = new Color(1f, 1f, 1f, 0f);
                    var colors = item.colors;
                    colors.normalColor = new Color(1f, 1f, 1f, 0f);
                    colors.highlightedColor = new Color(1f, 1f, 1f, 0.15f);
                    colors.selectedColor = new Color(1f, 1f, 1f, 0.15f);
                    colors.pressedColor = new Color(1f, 1f, 1f, 0.25f);
                    item.colors = colors;
                }
            }
        }

        // A third Gameplay row: "Tutorial hints", the same diamond toggle as auto-attack
        private static void EnsureTutorialHintsRow(GameObject root, SettingsPanelController controller, UISettings s)
        {
            var gameplay = root.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "GameplaySection");
            var row = Find(gameplay, "TutorialHintsRow");
            if (row == null)
            {
                row = FindOrCreate(gameplay, "TutorialHintsRow");
                var label = FindOrCreateText(row, "TutorialHintsTxt");
                label.text = "Tutorial hints";

                var autoToggle = Find(gameplay, "AutoAttackToggleRow").GetComponentInChildren<Toggle>(true);
                var source = PrefabUtility.GetCorrespondingObjectFromSource(autoToggle.gameObject);
                var instance = source != null
                    ? (GameObject)PrefabUtility.InstantiatePrefab(source, row)
                    : Object.Instantiate(autoToggle.gameObject, row);
                instance.name = "TutorialHintsToggle";
            }

            var so = new SerializedObject(controller);
            so.FindProperty("tutorialHintsToggle").objectReferenceValue = row.GetComponentInChildren<Toggle>(true);
            so.ApplyModifiedPropertiesWithoutUndo();

            var autoLabel = Find(gameplay, "AutoAttackToggleRow").GetComponentInChildren<TextMeshProUGUI>(true);
            autoLabel.text = "Auto-attack";
            var lockLabel = Find(gameplay, "LockonToggleRow").GetComponentInChildren<TextMeshProUGUI>(true);
            lockLabel.text = "Lock-on: toggle (off: hold)";
        }

        private static void SetRowHeight(GameObject go, float height)
        {
            var element = GetOrAdd<LayoutElement>(go);
            element.ignoreLayout = false;
            element.minHeight = height;
            element.preferredHeight = height;
            element.flexibleHeight = 0f;
        }

        private static void ReplacePersistentCalls(UnityEvent unityEvent, UnityAction call)
        {
            for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(unityEvent, i);
            }
            UnityEventTools.AddPersistentListener(unityEvent, call);
        }

        // --- Pause menu: a Settings button above Save & Exit -------------------------------------------

        private static void AddPauseSettingsButton(GameObject root, UISettings s)
        {
            var mainPanel = Find(root.transform, "MainPanel");
            var button = Find(mainPanel, "SettingsButton");
            if (button == null)
            {
                var resume = Find(mainPanel, "ResumeButton");
                button = (RectTransform)Object.Instantiate(resume.gameObject, mainPanel).transform;
                button.name = "SettingsButton";
            }
            button.GetComponentInChildren<TextMeshProUGUI>(true).text = "Settings";
            CenterWindowLayout.PlaceStackedButton(button, s, 2);
            ReplacePersistentCalls(button.GetComponent<Button>().onClick, root.GetComponent<PauseMenuController>().OpenSettings);
        }

        // --- Scenes -------------------------------------------------------------------------------------

        private static bool PlaceInUIMain(UnityEngine.SceneManagement.Scene scene, UISettings s)
        {
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
            var pause = all.FirstOrDefault(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject) &&
                AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)) == PauseDeathRestyler.PausePath);
            if (pause == null)
            {
                Debug.LogError("[SettingsRestyler] No PauseMenu in UIMain");
                return false;
            }

            var settingsInstance = all.FirstOrDefault(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject) &&
                AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)) == SettingsPath)?.gameObject;
            if (settingsInstance == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPath);
                settingsInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pause.parent);
                settingsInstance.transform.SetSiblingIndex(pause.GetSiblingIndex() + 1);
            }
            MatchPrefabRootLayout(settingsInstance);
            settingsInstance.SetActive(false);

            var window = settingsInstance.GetComponent<UIWindow>();
            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("priority").intValue = PauseSettingsPriority;
            windowSo.ApplyModifiedPropertiesWithoutUndo();

            var pauseSo = new SerializedObject(pause.GetComponent<PauseMenuController>());
            pauseSo.FindProperty("settingsWindow").objectReferenceValue = window;
            pauseSo.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool FixMainMenuInstance(UnityEngine.SceneManagement.Scene scene, UISettings s)
        {
            var instance = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject) &&
                    AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)) == SettingsPath)?.gameObject;
            if (instance == null)
            {
                Debug.LogError("[SettingsRestyler] No SettingsPanel in MainMenu");
                return false;
            }

            // The scene retargeted Back and Cancel to MainMenuController; the prefab's RequestClose now does it
            var window = instance.GetComponent<UIWindow>();
            RevertProperty(window, "onCancel");
            var back = instance.GetComponentsInChildren<Button>(true).First(b => b.name == "BackButton");
            RevertProperty(back, "m_OnClick");

            // Same size as the prefab, still anchored to the right edge for the slide-in (the animation sets x)
            var rect = (RectTransform)instance.transform;
            var source = (RectTransform)PrefabUtility.GetCorrespondingObjectFromSource(instance).transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = source.sizeDelta;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, source.anchoredPosition.y);
            return true;
        }

        private static void RevertProperty(Object target, string property)
        {
            var prop = new SerializedObject(target).FindProperty(property);
            if (prop != null && prop.prefabOverride) PrefabUtility.RevertPropertyOverride(prop, InteractionMode.AutomatedAction);
        }

        // The main menu slides the panel in from the right: keep it on screen at the new width
        private static void AdjustSlideClips(UISettings s)
        {
            float shownX = MainMenuShownX(s);
            foreach (string path in SlideClips)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if (binding.propertyName != "m_AnchoredPosition.x" || !binding.path.EndsWith("SettingsPanel")) continue;
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    var keys = curve.keys;
                    bool changed = false;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (keys[i].value < 0f && !Mathf.Approximately(keys[i].value, shownX))
                        {
                            keys[i].value = shownX;
                            changed = true;
                        }
                    }
                    if (!changed) continue;
                    curve.keys = keys;
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                    EditorUtility.SetDirty(clip);
                    Debug.Log($"[SettingsRestyler] {path}: shown x {OldShownX} -> {shownX}");
                }
            }
        }
    }
}
