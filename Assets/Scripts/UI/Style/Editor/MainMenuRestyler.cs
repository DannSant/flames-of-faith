using System.Linq;
using Game.Metaprogression;
using Game.UI.Navigation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 13: restyles the main menu in place. Every panel rests at its spot on the right, at the
    /// Settings window's height (MainMenuController fades and slides them; the old Animator is removed):
    /// - the menu panel (400 wide): "Flames of Faith" (Big, bold, yellow) over a column of Big buttons, with the
    ///   saved run (Small, muted) under Load Game;
    /// - the class select (640 wide): "Choose your class", three class cards on the left (portrait, name, a red
    ///   "Locked" note, a yellow ring on the chosen one), the class details on the right like the skill tree's
    ///   (name, description, starting weapon, a green / red footer, Skill Tree), Start Run and Back stacked at the bottom.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.MainMenuRestyler.Restyle
    /// </summary>
    public static class MainMenuRestyler
    {
        public const string MenuPanelPath = "Assets/Prefabs/UI/Menus/MainMenuPanel.prefab";
        public const string ClassSelectPath = "Assets/Prefabs/UI/Menus/CharacterSelect.prefab";
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";

        public const float MenuWidth = 400f;
        public const float ClassSelectWidth = 640f;
        public const float BigButtonWidth = 280f;
        public const float CardsWidth = 180f;
        private const float OutlineOutset = 2f;
        private static readonly string[] MenuButtons = { "StartButton", "LoadButton", "SkillTreeButton", "SettingsButton", "ExitButton" };

        public static Vector2 PanelPosition(UISettings s) => new Vector2(-s.Margin, s.Margin);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Main Menu")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[MainMenuRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(MenuPanelPath, root => RestyleMenuPanel(root, settings));
            UpgradeRestyler.EditPrefab(ClassSelectPath, root => RestyleClassSelect(root, settings));

            UIRevampTools.RunOnScenes(new[] { MainMenuScene }, (scene, path) =>
            {
                // Revert first: a revert reloads the instance, which would drop the snapped root layout
                bool changed = RevertChildLayoutOverrides(scene, MenuPanelPath);
                changed |= RevertChildLayoutOverrides(scene, ClassSelectPath);
                changed |= UpgradeRestyler.SnapSceneInstances(scene, MenuPanelPath, ClassSelectPath);
                changed |= WireScene(scene, settings);
                return changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[MainMenuRestyler] Main menu restyled.");
        }

        private static float ContentTop(UISettings s) => s.Margin + InventoryRestyler.TitleRowHeight + s.Spacing;

        private static void PlacePanel(GameObject root, UISettings s, float width)
        {
            Place((RectTransform)root.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), PanelPosition(s),
                CenterWindowLayout.WindowSize(s, width));
            var frame = GetOrAdd<Image>(root);
            frame.enabled = true;
            frame.raycastTarget = true;
            StyleFrame(frame, UIFrameKind.Panel, s);
            CenterWindowLayout.UseStandardMarker(root.GetComponent<UIWindow>());
        }

        private static TextMeshProUGUI PlaceWindowTitle(GameObject root, UISettings s, string text)
        {
            var title = FindOrCreateText(root.transform, "Title");
            title.text = text;
            title.transform.SetSiblingIndex(0);
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);
            return title;
        }

        // --- Menu panel ------------------------------------------------------------------------------------

        private static void RestyleMenuPanel(GameObject root, UISettings s)
        {
            float m = s.Margin;
            PlacePanel(root, s, MenuWidth);
            PlaceWindowTitle(root, s, "Flames of Faith");

            var container = Find(root.transform, "Container");
            container.SetSiblingIndex(1);
            var oldImage = container.GetComponent<Image>();
            if (oldImage != null) Object.DestroyImmediate(oldImage);
            Stretch(container, Vector2.zero, Vector2.one, m, m, m, ContentTop(s) + m);
            var column = GetOrAdd<VerticalLayoutGroup>(container.gameObject);
            column.padding = new RectOffset(0, 0, 0, 0);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = false;
            column.childControlHeight = false;
            column.childForceExpandWidth = false;
            column.childForceExpandHeight = false;

            // The saved run under Load Game (filled by MainMenuController)
            var saveInfo = FindOrCreateText(container, "SaveInfo");

            float bigHeight = s.GetButtonHeight(UIButtonSize.Big);
            var order = new[] { MenuButtons[0], MenuButtons[1], "SaveInfo", MenuButtons[2], MenuButtons[3], MenuButtons[4] };
            for (int i = 0; i < order.Length; i++) Find(container, order[i]).SetSiblingIndex(i);
            foreach (string name in MenuButtons)
            {
                var button = Find(container, name);
                button.sizeDelta = new Vector2(BigButtonWidth, bigHeight);
                StyleBigButton(button, s);
            }

            saveInfo.rectTransform.sizeDelta = new Vector2(MenuWidth - 2f * m, s.SmallFontSize + 6f);
            CenterWindowLayout.PlainText(saveInfo, TextAlignmentOptions.Center);
            saveInfo.textWrappingMode = TextWrappingModes.NoWrap;
            saveInfo.text = "Warrior - Act I - Level 7";
            StyleText(saveInfo, UIFontSize.Small, false, UIColorRole.Muted, s);
        }

        private static void StyleBigButton(RectTransform button, UISettings s)
        {
            var style = GetOrAdd<UIStyleButton>(button.gameObject);
            style.Configure(UIButtonSize.Big);
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                StretchFull(label.rectTransform);
                CenterWindowLayout.PlainText(label, TextAlignmentOptions.Center);
                StyleText(label, UIFontSize.Big, false, UIColorRole.Text, s);
            }
            style.Apply(s);
        }

        // --- Class select ----------------------------------------------------------------------------------

        private static void RestyleClassSelect(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float contentTop = ContentTop(s);
            float contentBottom = CenterWindowLayout.ButtonsTop(s, 2) + m;
            PlacePanel(root, s, ClassSelectWidth);
            PlaceWindowTitle(root, s, "Choose your class");

            // Left: the class cards, in GameSession.SelectedPlayerIndex order (Warrior, Archer, Augur)
            var container = Find(root.transform, "Container");
            container.SetSiblingIndex(1);
            var oldImage = container.GetComponent<Image>();
            if (oldImage != null) Object.DestroyImmediate(oldImage);
            container.anchorMin = new Vector2(0f, 0f);
            container.anchorMax = new Vector2(0f, 1f);
            container.pivot = new Vector2(0f, 0.5f);
            container.offsetMin = new Vector2(m, contentBottom);
            container.offsetMax = new Vector2(m + CardsWidth, -contentTop);
            var column = GetOrAdd<VerticalLayoutGroup>(container.gameObject);
            column.padding = new RectOffset(0, 0, 0, 0);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = true;

            foreach (var card in container.GetComponentsInChildren<ClassSelectHoverButton>(true))
            {
                RestyleCard(card, s);
            }

            // Bottom: Start Run in the progress spot, Back above it
            var back = Find(container, "Back") ?? Find(root.transform, "Back");
            back.SetParent(root.transform, false);
            var backLabel = back.GetComponentInChildren<TextMeshProUGUI>(true);
            backLabel.text = "Back";
            StretchFull(backLabel.rectTransform);
            CenterWindowLayout.PlaceStackedButton(back, s, 1);

            var start = FindOrCreate(root.transform, "StartRunButton");
            GetOrAdd<CanvasRenderer>(start.gameObject);
            var startImage = GetOrAdd<Image>(start.gameObject);
            var startButton = GetOrAdd<Button>(start.gameObject);
            startButton.targetGraphic = startImage;
            var startLabel = FindOrCreateText(start, "Label");
            StretchFull(startLabel.rectTransform);
            startLabel.text = "Start Run";
            CenterWindowLayout.PlaceStackedButton(start, s, 0);

            // Right: the class details, like the skill tree's details panel
            var details = FindOrCreateImage(root.transform, "Details");
            details.rectTransform.anchorMin = Vector2.zero;
            details.rectTransform.anchorMax = Vector2.one;
            details.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            details.rectTransform.offsetMin = new Vector2(m + CardsWidth + s.Spacing, contentBottom);
            details.rectTransform.offsetMax = new Vector2(-m, -contentTop);
            details.raycastTarget = false;
            StyleFrame(details, UIFrameKind.Slot, s);
            var layout = GetOrAdd<VerticalLayoutGroup>(details.gameObject);
            layout.padding = new RectOffset((int)m, (int)m, (int)m, (int)m);
            layout.spacing = s.Spacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var tooltip = Find(root.transform, "TooltipPane");
            TextMeshProUGUI Reuse(string newName, string oldName)
            {
                var existing = Find(details.transform, newName) ?? (tooltip != null ? Find(tooltip, oldName) : null);
                var text = existing != null ? existing.GetComponent<TextMeshProUGUI>() : FindOrCreateText(details.transform, newName);
                text.transform.SetParent(details.transform, false);
                text.name = newName;
                return text;
            }

            var className = Reuse("ClassName", "TooltipTitle");
            var classDesc = Reuse("ClassDesc", "TooltipDesc");
            var weaponHeader = Reuse("WeaponHeader", "Title (1)");
            var weaponName = Reuse("WeaponName", "TooltipItemTitle");
            var weaponDesc = Reuse("WeaponDesc", "TooltipItemDesc");
            var footer = Reuse("Footer", "Footer");
            DestroyChild(root.transform, "TooltipPane");

            DetailText(className, 0, UIFontSize.Big, UIColorRole.Text, true, TextAlignmentOptions.TopLeft, s, "Warrior");
            DetailText(classDesc, 1, UIFontSize.Small, UIColorRole.Text, false, TextAlignmentOptions.TopLeft, s, "A sturdy fighter...");
            GetOrAdd<LayoutElement>(classDesc.gameObject).flexibleHeight = 1f;
            DetailText(weaponHeader, 2, UIFontSize.Small, UIColorRole.Highlight, true, TextAlignmentOptions.TopLeft, s, "Starting weapon");
            DetailText(weaponName, 3, UIFontSize.Small, UIColorRole.None, true, TextAlignmentOptions.TopLeft, s, "Sword");
            DetailText(weaponDesc, 4, UIFontSize.Small, UIColorRole.Text, false, TextAlignmentOptions.TopLeft, s, "Swings in an arc.");
            DetailText(footer, 5, UIFontSize.Small, UIColorRole.None, false, TextAlignmentOptions.Center, s, "Unlocked");

            // Skill Tree: opens the tree on the chosen class (e.g. to unlock it)
            var treeButton = FindOrCreate(details.transform, "SkillTreeButton");
            treeButton.SetSiblingIndex(6);
            GetOrAdd<CanvasRenderer>(treeButton.gameObject);
            var treeImage = GetOrAdd<Image>(treeButton.gameObject);
            GetOrAdd<Button>(treeButton.gameObject).targetGraphic = treeImage;
            var treeLabel = FindOrCreateText(treeButton, "Label");
            StretchFull(treeLabel.rectTransform);
            treeLabel.text = "Skill Tree";
            var treeLayout = GetOrAdd<LayoutElement>(treeButton.gameObject);
            treeLayout.minHeight = treeLayout.preferredHeight = s.GetButtonHeight(UIButtonSize.Small);
            treeButton.sizeDelta = new Vector2(CenterWindowLayout.ProgressButtonWidth, s.GetButtonHeight(UIButtonSize.Small));
            CenterWindowLayout.StyleSmallButton(treeButton, s);

            start.SetAsLastSibling();
        }

        private static void DetailText(TextMeshProUGUI text, int index, UIFontSize size, UIColorRole color, bool bold,
            TextAlignmentOptions alignment, UISettings s, string sample)
        {
            text.transform.SetSiblingIndex(index);
            var element = text.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = false;
            CenterWindowLayout.PlainText(text, alignment);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            text.raycastTarget = false;
            if (string.IsNullOrEmpty(text.text) || text.text.StartsWith("'") || text.text.Contains("Starting item")) text.text = sample;
            if (color == UIColorRole.None) text.color = s.TextColor;
            StyleText(text, size, false, color, s, bold);
        }

        private static void RestyleCard(ClassSelectHoverButton card, UISettings s)
        {
            var rect = (RectTransform)card.transform;
            var element = card.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = false;

            // Background: the slot frame, the button's tint target (ClassSelectHoverButton greys it while locked)
            var button = card.GetComponent<Button>();
            var background = (Find(rect, "Image") ?? Find(rect, "Background")).GetComponent<Image>();
            background.name = "Background";
            background.transform.SetSiblingIndex(0);
            StretchFull(background.rectTransform);
            background.raycastTarget = true;
            StyleFrame(background, UIFrameKind.Slot, s);
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            button.colors = colors;

            // Portrait: the class sprite already on the card
            var portrait = rect.Cast<Transform>()
                .Select(t => t.GetComponent<Image>())
                .FirstOrDefault(i => i != null && i != background && i.name != "SelectedMark");
            if (portrait != null)
            {
                portrait.name = "Portrait";
                portrait.transform.SetSiblingIndex(1);
                Stretch(portrait.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), 6f, 44f, 6f, 6f);
                portrait.preserveAspect = true;
                portrait.raycastTarget = false;
            }

            var name = (Find(rect, "Name") ?? Find(rect, "Text (TMP)")).GetComponent<TextMeshProUGUI>();
            name.name = "Name";
            name.transform.SetSiblingIndex(2);
            Place(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(CardsWidth - 12f, 18f));
            CenterWindowLayout.PlainText(name, TextAlignmentOptions.Center);
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.text = card.ClassData != null ? card.ClassData.characterName : name.text;
            name.raycastTarget = false;
            StyleText(name, UIFontSize.Small, false, UIColorRole.Text, s, bold: true);

            var locked = FindOrCreateText(rect, "Locked");
            locked.transform.SetSiblingIndex(3);
            Place(locked.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(CardsWidth - 12f, 16f));
            CenterWindowLayout.PlainText(locked, TextAlignmentOptions.Center);
            locked.text = "Locked";
            StyleText(locked, UIFontSize.Small, false, UIColorRole.Negative, s);
            locked.gameObject.SetActive(false);

            // The chosen class: the item bag's yellow ring
            var mark = FindOrCreateImage(rect, "SelectedMark");
            StretchFull(mark.rectTransform, -OutlineOutset);
            mark.sprite = s.SelectedOutline;
            mark.type = Image.Type.Sliced;
            mark.raycastTarget = false;
            var markColor = GetOrAdd<UIStyleColor>(mark.gameObject);
            markColor.Configure(UIColorRole.Highlight);
            markColor.Apply(s);
            mark.transform.SetAsLastSibling();
            mark.gameObject.SetActive(false);

            var so = new SerializedObject(card);
            so.FindProperty("lockedLabel").objectReferenceValue = locked.gameObject;
            so.FindProperty("selectedMark").objectReferenceValue = mark.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- MainMenu scene --------------------------------------------------------------------------------

        private static bool WireScene(UnityEngine.SceneManagement.Scene scene, UISettings s)
        {
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
            var controller = all.Select(t => t.GetComponent<MainMenuController>()).FirstOrDefault(c => c != null);
            if (controller == null)
            {
                Debug.LogError("[MainMenuRestyler] No MainMenuController in MainMenu");
                return false;
            }

            // The panels fade and slide in code now
            var animator = controller.GetComponent<Animator>();
            if (animator != null) Object.DestroyImmediate(animator);

            GameObject Instance(string prefabPath) => all.Select(t => t.gameObject).FirstOrDefault(go =>
                PrefabUtility.IsAnyPrefabInstanceRoot(go) && AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go)) == prefabPath);
            var menu = Instance(MenuPanelPath);
            var classSelect = Instance(ClassSelectPath);
            var settingsPanel = Instance(SettingsRestyler.SettingsPath);

            // Settings rests at its shown spot like the other panels
            if (settingsPanel != null)
            {
                var rect = (RectTransform)settingsPanel.transform;
                rect.anchoredPosition = new Vector2(SettingsRestyler.MainMenuShownX(s), rect.anchoredPosition.y);
                PrefabUtility.RecordPrefabInstancePropertyModifications(rect);
            }

            // B on the class select goes back to the menu panel
            var classWindow = classSelect.GetComponent<UIWindow>();
            var windowSo = new SerializedObject(classWindow);
            var calls = windowSo.FindProperty("onCancel.m_PersistentCalls.m_Calls");
            calls.arraySize = 1;
            var call = calls.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = controller;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = "Game.UI.MainMenuController, Assembly-CSharp";
            call.FindPropertyRelative("m_MethodName").stringValue = nameof(MainMenuController.SwitchFromCharacterSelectToMainPanel);
            call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // Void
            call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // RuntimeOnly
            windowSo.ApplyModifiedPropertiesWithoutUndo();

            var cards = classSelect.GetComponentsInChildren<ClassSelectHoverButton>(true);
            TextMeshProUGUI Text(GameObject owner, string name) =>
                owner.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.name == name);

            var so = new SerializedObject(controller);
            so.FindProperty("mainPanelWindow").objectReferenceValue = menu.GetComponent<UIWindow>();
            so.FindProperty("characterSelectWindow").objectReferenceValue = classWindow;
            if (settingsPanel != null) so.FindProperty("settingsWindow").objectReferenceValue = settingsPanel.GetComponent<UIWindow>();

            var classes = so.FindProperty("classesByPlayerIndex");
            classes.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++) classes.GetArrayElementAtIndex(i).objectReferenceValue = cards[i].ClassData;

            so.FindProperty("startRunButton").objectReferenceValue = classSelect.GetComponentsInChildren<Button>(true).First(b => b.name == "StartRunButton");
            so.FindProperty("skillTree").objectReferenceValue = all.Select(t => t.GetComponent<SkillTreeController>()).FirstOrDefault(c => c != null);
            so.FindProperty("classNameText").objectReferenceValue = Text(classSelect, "ClassName");
            so.FindProperty("classDescriptionText").objectReferenceValue = Text(classSelect, "ClassDesc");
            so.FindProperty("itemNameText").objectReferenceValue = Text(classSelect, "WeaponName");
            so.FindProperty("itemDescriptionText").objectReferenceValue = Text(classSelect, "WeaponDesc");
            so.FindProperty("classFooterText").objectReferenceValue = Text(classSelect, "Footer");
            so.FindProperty("saveInfoText").objectReferenceValue = Text(menu, "SaveInfo");
            so.FindProperty("loadGameButton").objectReferenceValue = menu.GetComponentsInChildren<Button>(true).First(b => b.name == "LoadButton");
            // The footer replaced the grey "Locked..." line appended to the description
            so.FindProperty("classLockedInfo").stringValue = "Locked. Unlock it in the skill tree for {0} Favors.";
            so.ApplyModifiedPropertiesWithoutUndo();

            // The class select's Skill Tree button opens the tree on the chosen class
            var treeButton = classSelect.GetComponentsInChildren<Button>(true).First(b => b.name == "SkillTreeButton");
            var treeSo = new SerializedObject(treeButton);
            var treeCalls = treeSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            treeCalls.arraySize = 1;
            var treeCall = treeCalls.GetArrayElementAtIndex(0);
            treeCall.FindPropertyRelative("m_Target").objectReferenceValue = controller;
            treeCall.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = "Game.UI.MainMenuController, Assembly-CSharp";
            treeCall.FindPropertyRelative("m_MethodName").stringValue = nameof(MainMenuController.OpenSkillTreeForSelected);
            treeCall.FindPropertyRelative("m_Mode").enumValueIndex = 1;
            treeCall.FindPropertyRelative("m_CallState").enumValueIndex = 2;
            treeSo.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
