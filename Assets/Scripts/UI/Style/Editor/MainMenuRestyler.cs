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
    /// It also places the Pilgrim's Chronicle (ChronicleRestyler) and adds its button.
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
        public const string ChronicleButton = "ChronicleButton";
        private static readonly string[] MenuButtons = { "StartButton", "LoadButton", "SkillTreeButton", ChronicleButton, "SettingsButton", "ExitButton" };

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
            ChronicleRestyler.RestylePrefab(settings);

            UIRevampTools.RunOnScenes(new[] { MainMenuScene }, (scene, path) =>
            {
                // Revert first: a revert reloads the instance, which would drop the snapped root layout
                bool changed = EnsureChronicleInstance(scene);
                changed |= RevertChildLayoutOverrides(scene, MenuPanelPath);
                changed |= RevertChildLayoutOverrides(scene, ClassSelectPath);
                changed |= UpgradeRestyler.SnapSceneInstances(scene, MenuPanelPath, ClassSelectPath, ChronicleRestyler.PanelPath);
                changed |= WireScene(scene, settings);
                return changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[MainMenuRestyler] Main menu restyled.");
        }

        // The Chronicle rests with the other panels, right after Settings
        private static bool EnsureChronicleInstance(UnityEngine.SceneManagement.Scene scene)
        {
            var transforms = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
            bool IsInstanceOf(Transform t, string path) => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)
                && AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject)) == path;
            if (transforms.Any(t => IsInstanceOf(t, ChronicleRestyler.PanelPath))) return false;

            var settingsPanel = transforms.First(t => IsInstanceOf(t, SettingsRestyler.SettingsPath));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ChronicleRestyler.PanelPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, settingsPanel.parent);
            instance.transform.SetSiblingIndex(settingsPanel.GetSiblingIndex() + 1);
            return true;
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

            // A copy of Settings: same art and label, opens the Pilgrim's Chronicle (wired in the scene)
            if (Find(container, ChronicleButton) == null)
            {
                var chronicle = Object.Instantiate(Find(container, "SettingsButton"), container);
                chronicle.name = ChronicleButton;
            }
            Find(container, ChronicleButton).GetComponentInChildren<TextMeshProUGUI>(true).text = "Pilgrim's Chronicle";

            float bigHeight = s.GetButtonHeight(UIButtonSize.Big);
            var order = new[] { MenuButtons[0], MenuButtons[1], "SaveInfo", MenuButtons[2], MenuButtons[3], MenuButtons[4], MenuButtons[5] };
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
            saveInfo.text = "Crusader - Act I - Level 7";
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

            // Left: the class cards, in GameSession.SelectedPlayerIndex order (Crusader, Sentinel, Augur)
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
            var footer = Reuse("Footer", "Footer");
            DestroyChild(root.transform, "TooltipPane");
            // The Abilities tab replaced the "Starting weapon" block (Nov 2026)
            foreach (string old in new[] { "WeaponHeader", "WeaponName", "WeaponDesc" }) DestroyChild(details.transform, old);

            DetailText(className, 0, UIFontSize.Big, UIColorRole.Text, true, TextAlignmentOptions.TopLeft, s, "Crusader");
            DetailText(classDesc, 1, UIFontSize.Small, UIColorRole.Text, false, TextAlignmentOptions.TopLeft, s, "A sturdy fighter...");
            GetOrAdd<LayoutElement>(classDesc.gameObject).flexibleHeight = 0f;
            var tabsUi = BuildDetailsTabs(details.rectTransform, s);
            DetailText(footer, 4, UIFontSize.Small, UIColorRole.None, false, TextAlignmentOptions.Center, s, "Unlocked");

            // Skill Tree: opens the tree on the chosen class (e.g. to unlock it)
            var treeButton = FindOrCreate(details.transform, "SkillTreeButton");
            treeButton.SetSiblingIndex(5);
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

            // The tabs link the whole class select for the gamepad
            var tabsSo = new SerializedObject(tabsUi);
            var cardList = tabsSo.FindProperty("cards");
            var cardButtons = container.GetComponentsInChildren<ClassSelectHoverButton>(true).Select(c => c.GetComponent<Selectable>()).ToArray();
            cardList.arraySize = cardButtons.Length;
            for (int i = 0; i < cardButtons.Length; i++) cardList.GetArrayElementAtIndex(i).objectReferenceValue = cardButtons[i];
            tabsSo.FindProperty("skillTreeButton").objectReferenceValue = treeButton.GetComponent<Selectable>();
            tabsSo.FindProperty("backButton").objectReferenceValue = back.GetComponent<Selectable>();
            tabsSo.FindProperty("startRunButton").objectReferenceValue = startButton;
            tabsSo.ApplyModifiedPropertiesWithoutUndo();

            // The cards' buttons choose their class by name (persistent listeners): keep them on the renamed methods
            foreach (var card in container.GetComponentsInChildren<ClassSelectHoverButton>(true))
            {
                if (card.ClassData == null) continue;
                var cardSo = new SerializedObject(card.GetComponent<Button>());
                var cardCalls = cardSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                if (cardCalls.arraySize == 0) continue;
                cardCalls.GetArrayElementAtIndex(0).FindPropertyRelative("m_MethodName").stringValue = SelectMethodFor(card.ClassData.characterClass);
                cardSo.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        /// <summary>The MainMenuController method a class card calls when pressed.</summary>
        public static string SelectMethodFor(Game.Control.CharacterClass characterClass) => characterClass switch
        {
            Game.Control.CharacterClass.Crusader => nameof(MainMenuController.SelectCrusader),
            Game.Control.CharacterClass.Sentinel => nameof(MainMenuController.SelectSentinel),
            Game.Control.CharacterClass.Augur => nameof(MainMenuController.SelectAugur),
            _ => string.Empty,
        };

        public const float DetailTabsHeight = 32f;
        public const float DetailRowSpacing = 4f;
        private static readonly string[] DetailTabNames = { "Abilities", "Lore", "Playstyle" };
        private static readonly Color RowBandNormal = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color RowBandHighlighted = new Color(1f, 1f, 1f, 0.85f);
        private static readonly Color TextBandNormal = new Color(1f, 1f, 1f, 0f);
        private static readonly Color TextBandHighlighted = new Color(1f, 1f, 1f, 0.2f);

        // The Abilities / Lore / Playstyle tabs and their scrolling list, between the description and the footer
        private static ClassDetailsTabsUI BuildDetailsTabs(RectTransform details, UISettings s)
        {
            var ui = GetOrAdd<ClassDetailsTabsUI>(details.gameObject);

            var tabsRow = FindOrCreate(details, "Tabs");
            tabsRow.SetSiblingIndex(2);
            var tabsLayout = GetOrAdd<HorizontalLayoutGroup>(tabsRow.gameObject);
            tabsLayout.spacing = s.Spacing;
            tabsLayout.childControlWidth = true;
            tabsLayout.childControlHeight = true;
            tabsLayout.childForceExpandWidth = true;
            tabsLayout.childForceExpandHeight = true;
            var tabsElement = GetOrAdd<LayoutElement>(tabsRow.gameObject);
            tabsElement.minHeight = tabsElement.preferredHeight = DetailTabsHeight;

            var uiSo = new SerializedObject(ui);
            var tabButtons = uiSo.FindProperty("tabButtons");
            var tabLabels = uiSo.FindProperty("tabLabels");
            tabButtons.arraySize = DetailTabNames.Length;
            tabLabels.arraySize = DetailTabNames.Length;
            for (int i = 0; i < DetailTabNames.Length; i++)
            {
                var tab = FindOrCreate(tabsRow, "Tab_" + DetailTabNames[i]);
                tab.SetSiblingIndex(i);
                GetOrAdd<CanvasRenderer>(tab.gameObject);
                var image = GetOrAdd<Image>(tab.gameObject);
                var button = GetOrAdd<Button>(tab.gameObject);
                button.targetGraphic = image;
                var label = FindOrCreateText(tab, "Label");
                StatsRestyler.RestyleTab(tab, DetailTabNames[i], new Vector2(0.5f, 0.5f), Vector2.zero, 100f, s);
                label.color = i == 0 ? s.HighlightColor : s.TextColor;
                if (i == 0) image.sprite = s.GetTabSprite(true);
                tabButtons.GetArrayElementAtIndex(i).objectReferenceValue = button;
                tabLabels.GetArrayElementAtIndex(i).objectReferenceValue = label;
            }

            // The list: a masked view whose content follows its rows; it only takes the room left over
            var scrollRect = FindOrCreate(details, "TabScroll");
            scrollRect.SetSiblingIndex(3);
            var scrollElement = GetOrAdd<LayoutElement>(scrollRect.gameObject);
            scrollElement.minHeight = 0f;
            scrollElement.preferredHeight = 0f;
            scrollElement.flexibleHeight = 1f;
            var scroll = GetOrAdd<ScrollRect>(scrollRect.gameObject);
            var view = FindOrCreate(scrollRect, "View");
            StretchFull(view);
            GetOrAdd<RectMask2D>(view.gameObject);
            var content = FindOrCreate(view, "Content");
            StatsRestyler.RestyleList(content, s);
            GetOrAdd<VerticalLayoutGroup>(content.gameObject).spacing = DetailRowSpacing;
            scroll.viewport = view;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20f;
            scroll.verticalScrollbar = null;

            // Row templates, kept inactive next to the view
            var templates = FindOrCreate(scrollRect, "Templates");
            templates.gameObject.SetActive(false);
            var kitRow = BuildRow(templates, "KitRowTemplate", s, RowBandNormal, RowBandHighlighted, out var kitText);
            float slot = s.GetIconSize(UIIconSize.Small);
            var kitLayout = GetOrAdd<HorizontalLayoutGroup>(kitRow.gameObject);
            kitLayout.padding = new RectOffset(4, 4, 4, 4);
            kitLayout.spacing = s.Spacing;
            kitLayout.childAlignment = TextAnchor.MiddleLeft;
            kitLayout.childControlWidth = true;
            kitLayout.childControlHeight = true;
            kitLayout.childForceExpandWidth = false;
            kitLayout.childForceExpandHeight = false;
            var frame = FindOrCreateImage(kitRow.transform, "Slot");
            frame.transform.SetSiblingIndex(0);
            StyleFrame(frame, UIFrameKind.Slot, s);
            frame.raycastTarget = false;
            var frameElement = GetOrAdd<LayoutElement>(frame.gameObject);
            frameElement.minWidth = frameElement.preferredWidth = slot;
            frameElement.minHeight = frameElement.preferredHeight = slot;
            var icon = FindOrCreateImage(frame.transform, "Icon");
            StretchFull(icon.rectTransform, 6f);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            kitText.transform.SetSiblingIndex(1);
            GetOrAdd<LayoutElement>(kitText.gameObject).flexibleWidth = 1f;
            kitText.alignment = TextAlignmentOptions.MidlineLeft;
            var kitSo = new SerializedObject(kitRow);
            kitSo.FindProperty("frame").objectReferenceValue = frame;
            kitSo.FindProperty("icon").objectReferenceValue = icon;
            kitSo.ApplyModifiedPropertiesWithoutUndo();

            var textRow = BuildRow(templates, "TextRowTemplate", s, TextBandNormal, TextBandHighlighted, out var paragraph);
            var textLayout = GetOrAdd<VerticalLayoutGroup>(textRow.gameObject);
            textLayout.padding = new RectOffset(4, 4, 2, 2);
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;
            paragraph.alignment = TextAlignmentOptions.TopLeft;

            var statRow = BuildRow(templates, "StatRowTemplate", s, TextBandNormal, TextBandHighlighted, out var statText);
            var statLayout = GetOrAdd<HorizontalLayoutGroup>(statRow.gameObject);
            statLayout.padding = new RectOffset(4, 4, 2, 2);
            statLayout.spacing = s.Spacing;
            statLayout.childAlignment = TextAnchor.UpperLeft;
            statLayout.childControlWidth = true;
            statLayout.childControlHeight = true;
            statLayout.childForceExpandWidth = false;
            statLayout.childForceExpandHeight = false;
            var star = FindOrCreateImage(statRow.transform, "Star");
            star.transform.SetSiblingIndex(0);
            star.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CurrencyRowBuilder.FavorIconPath);
            star.preserveAspect = true;
            star.raycastTarget = false;
            float starSize = s.GetFontSize(UIFontSize.Small) + 4f;
            var starElement = GetOrAdd<LayoutElement>(star.gameObject);
            starElement.minWidth = starElement.preferredWidth = starSize;
            starElement.minHeight = starElement.preferredHeight = starSize;
            statText.transform.SetSiblingIndex(1);
            GetOrAdd<LayoutElement>(statText.gameObject).flexibleWidth = 1f;
            statText.alignment = TextAlignmentOptions.TopLeft;
            var statSo = new SerializedObject(statRow);
            statSo.FindProperty("star").objectReferenceValue = star;
            statSo.ApplyModifiedPropertiesWithoutUndo();

            uiSo.FindProperty("scrollRect").objectReferenceValue = scroll;
            uiSo.FindProperty("content").objectReferenceValue = content;
            uiSo.FindProperty("kitRowTemplate").objectReferenceValue = kitRow;
            uiSo.FindProperty("textRowTemplate").objectReferenceValue = textRow;
            uiSo.FindProperty("statRowTemplate").objectReferenceValue = statRow;
            uiSo.ApplyModifiedPropertiesWithoutUndo();
            return ui;
        }

        // A selectable row: a band (its own image, so the gamepad frame and hover cover it) and a Small text
        private static ClassDetailsRowUI BuildRow(RectTransform parent, string name, UISettings s, Color normal, Color highlighted, out TextMeshProUGUI text)
        {
            var rect = FindOrCreate(parent, name);
            GetOrAdd<CanvasRenderer>(rect.gameObject);
            var band = GetOrAdd<Image>(rect.gameObject);
            band.sprite = s.SlotFrame;
            band.type = Image.Type.Sliced;
            band.color = Color.white;
            band.raycastTarget = true;
            var button = GetOrAdd<Button>(rect.gameObject);
            button.targetGraphic = band;
            button.transition = Selectable.Transition.ColorTint;
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = normal;
            colors.highlightedColor = highlighted;
            colors.selectedColor = highlighted;
            colors.pressedColor = highlighted;
            colors.disabledColor = normal;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit };

            text = FindOrCreateText(rect, "Text");
            CenterWindowLayout.PlainText(text, TextAlignmentOptions.TopLeft);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            text.raycastTarget = false;
            text.text = name;
            StyleText(text, UIFontSize.Small, false, UIColorRole.None, s);
            text.color = s.TextColor;

            var row = GetOrAdd<ClassDetailsRowUI>(rect.gameObject);
            var so = new SerializedObject(row);
            so.FindProperty("text").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            return row;
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
            var chroniclePanel = Instance(ChronicleRestyler.PanelPath);

            // The shared tooltip, for the class select's Abilities tab: drawn over every panel, never takes raycasts
            var tooltip = Instance(TooltipRestyler.PanePath);
            if (tooltip == null)
            {
                var canvas = classSelect.GetComponentInParent<Canvas>(true).rootCanvas.transform;
                tooltip = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TooltipRestyler.PanePath), canvas);
                MatchPrefabRootLayout(tooltip);
            }
            tooltip.transform.SetAsLastSibling();
            var tabsUi = classSelect.GetComponentInChildren<ClassDetailsTabsUI>(true);
            var tabsSo = new SerializedObject(tabsUi);
            tabsSo.FindProperty("tooltipPane").objectReferenceValue = tooltip.GetComponent<GeneralTooltipPaneUI>();
            tabsSo.ApplyModifiedPropertiesWithoutUndo();

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
            so.FindProperty("chronicleWindow").objectReferenceValue = chroniclePanel.GetComponent<UIWindow>();

            var classes = so.FindProperty("classesByPlayerIndex");
            classes.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++) classes.GetArrayElementAtIndex(i).objectReferenceValue = cards[i].ClassData;

            so.FindProperty("startRunButton").objectReferenceValue = classSelect.GetComponentsInChildren<Button>(true).First(b => b.name == "StartRunButton");
            so.FindProperty("skillTree").objectReferenceValue = all.Select(t => t.GetComponent<SkillTreeController>()).FirstOrDefault(c => c != null);
            so.FindProperty("classNameText").objectReferenceValue = Text(classSelect, "ClassName");
            so.FindProperty("classDescriptionText").objectReferenceValue = Text(classSelect, "ClassDesc");
            so.FindProperty("detailsTabs").objectReferenceValue = classSelect.GetComponentInChildren<ClassDetailsTabsUI>(true);
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

            var chronicleButton = menu.GetComponentsInChildren<Button>(true).First(b => b.name == ChronicleButton);
            var chronicleSo = new SerializedObject(chronicleButton);
            var chronicleCalls = chronicleSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            chronicleCalls.arraySize = 1;
            var chronicleCall = chronicleCalls.GetArrayElementAtIndex(0);
            chronicleCall.FindPropertyRelative("m_Target").objectReferenceValue = controller;
            chronicleCall.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = "Game.UI.MainMenuController, Assembly-CSharp";
            chronicleCall.FindPropertyRelative("m_MethodName").stringValue = nameof(MainMenuController.OpenChronicle);
            chronicleCall.FindPropertyRelative("m_Mode").enumValueIndex = 1;
            chronicleCall.FindPropertyRelative("m_CallState").enumValueIndex = 2;
            chronicleSo.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
