using Game.UI.Chronicle;
using Game.UI.Navigation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// Builds the Pilgrim's Chronicle panel (Prefabs/UI/Menus/PilgrimsChronicle): a main menu panel with a title, a row
    /// of tabs, one page per tab and Back in the progress spot. Today it has one tab, Scenes: a scrolling list of the
    /// seen scenes. A new section is a new tab and page here, plus an entry in ChronicleUI.tabs.
    /// MainMenuRestyler runs it and places the instance in the scene.
    /// </summary>
    public static class ChronicleRestyler
    {
        public const string PanelPath = "Assets/Prefabs/UI/Menus/PilgrimsChronicle.prefab";
        public const float Width = 640f;
        private const float TabWidth = 160f;

        public static void RestylePrefab(UISettings settings)
        {
            CutsceneRestyler.EnsurePrefab(PanelPath);
            UpgradeRestyler.EditPrefab(PanelPath, root => RestylePanel(root, settings));
        }

        private static void RestylePanel(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float rowHeight = s.GetButtonHeight(UIButtonSize.Small);
            var ui = GetOrAdd<ChronicleUI>(root);
            GetOrAdd<CanvasGroup>(root);
            var window = GetOrAdd<UIWindow>(root);

            var rootRect = (RectTransform)root.transform;
            Place(rootRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), MainMenuRestyler.PanelPosition(s), CenterWindowLayout.WindowSize(s, Width));
            var frame = GetOrAdd<Image>(root);
            frame.raycastTarget = true;
            StyleFrame(frame, UIFrameKind.Panel, s);

            var title = FindOrCreateText(root.transform, "Title");
            title.text = "Pilgrim's Chronicle";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            // Tabs under the title
            float tabsTop = m + InventoryRestyler.TitleRowHeight + s.Spacing;
            var tabs = FindOrCreate(root.transform, "Tabs");
            Stretch(tabs, new Vector2(0f, 1f), new Vector2(1f, 1f));
            tabs.pivot = new Vector2(0.5f, 1f);
            tabs.anchoredPosition = new Vector2(0f, -tabsTop);
            tabs.sizeDelta = new Vector2(-2f * m, StatsRestyler.TabsHeight);

            var tab = FindOrCreateImage(tabs, "ScenesTab");
            var tabButton = GetOrAdd<Button>(tab.gameObject);
            tabButton.targetGraphic = tab;
            // Nowhere to go with one tab; give the tabs Automatic navigation when a second one joins
            tabButton.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            var tabLabel = FindOrCreateText(tab.rectTransform, "Label");
            StatsRestyler.RestyleTab(tab.rectTransform, "Scenes", new Vector2(0f, 1f), Vector2.zero, TabWidth, s);
            tab.rectTransform.pivot = new Vector2(0f, 1f);

            // The tab's page, between the tabs and Back
            float pageTop = tabsTop + StatsRestyler.TabsHeight + s.Spacing;
            float pageBottom = CenterWindowLayout.ButtonsTop(s, 1) + s.Spacing;
            var page = FindOrCreate(root.transform, "ScenesPage");
            StretchFull(page);

            var view = FindOrCreate(page, "View");
            Stretch(view, Vector2.zero, Vector2.one, m, pageBottom, m + StatsRestyler.ScrollbarWidth + 4f, pageTop);
            GetOrAdd<RectMask2D>(view.gameObject).enabled = true;

            var rows = FindOrCreate(view, "Rows");
            StatsRestyler.RestyleList(rows, s);
            rows.GetComponent<VerticalLayoutGroup>().spacing = s.Spacing;

            var template = FindOrCreate(rows, "RowTemplate");
            var templateImage = GetOrAdd<Image>(template.gameObject);
            var templateButton = GetOrAdd<Button>(template.gameObject);
            templateButton.targetGraphic = templateImage;
            var templateElement = GetOrAdd<LayoutElement>(template.gameObject);
            templateElement.minHeight = templateElement.preferredHeight = rowHeight;
            var templateLabel = FindOrCreateText(template, "Label");
            StretchFull(templateLabel.rectTransform);
            templateLabel.text = "The Pilgrimage of the Flame";
            CenterWindowLayout.StyleSmallButton(template, s);
            var row = GetOrAdd<ChronicleRowUI>(template.gameObject);
            var rowSo = new SerializedObject(row);
            rowSo.FindProperty("label").objectReferenceValue = templateLabel;
            rowSo.ApplyModifiedPropertiesWithoutUndo();
            template.gameObject.SetActive(false);

            var scrollbar = StatsRestyler.BuildScrollbar((RectTransform)page, s, pageTop, pageBottom);

            var scroll = GetOrAdd<ScrollRect>(page.gameObject);
            scroll.viewport = view;
            scroll.content = rows;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.scrollSensitivity = rowHeight;
            scroll.horizontalScrollbar = null;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scroll.verticalScrollbarSpacing = 0f;

            var empty = FindOrCreateText(page, "EmptyText");
            Stretch(empty.rectTransform, Vector2.zero, Vector2.one, m, pageBottom, m, pageTop);
            CenterWindowLayout.PlainText(empty, TextAlignmentOptions.Center);
            empty.textWrappingMode = TextWrappingModes.Normal;
            empty.text = "Nothing is written here yet.";
            StyleText(empty, UIFontSize.Small, false, UIColorRole.Muted, s);

            var back = FindOrCreate(root.transform, "Back");
            var backImage = GetOrAdd<Image>(back.gameObject);
            var backButton = GetOrAdd<Button>(back.gameObject);
            backButton.targetGraphic = backImage;
            var backLabel = FindOrCreateText(back, "Label");
            StretchFull(backLabel.rectTransform);
            backLabel.text = "Back";
            CenterWindowLayout.PlaceStackedButton(back, s, 0);

            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("role").enumValueIndex = (int)UIWindowRole.Primary;
            windowSo.FindProperty("openMode").enumValueIndex = (int)UIWindowOpenMode.Manual;
            windowSo.FindProperty("focusMode").enumValueIndex = (int)UIWindowFocusMode.AutoFocus;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            CenterWindowLayout.UseStandardMarker(window);
            SettingsRestyler.ReplacePersistentCalls(window.OnCancel, ui.RequestClose);

            var so = new SerializedObject(ui);
            var tabList = so.FindProperty("tabs");
            tabList.arraySize = 1;
            var entry = tabList.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("button").objectReferenceValue = tabButton;
            entry.FindPropertyRelative("label").objectReferenceValue = tabLabel;
            entry.FindPropertyRelative("page").objectReferenceValue = page.gameObject;
            so.FindProperty("scrollRect").objectReferenceValue = scroll;
            so.FindProperty("rowTemplate").objectReferenceValue = row;
            so.FindProperty("emptyText").objectReferenceValue = empty.gameObject;
            so.FindProperty("backButton").objectReferenceValue = backButton;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
