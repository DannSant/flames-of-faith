using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 4: restyles the Stats pane (StatsPane + StatRow) in place.
    /// - The window mirrors the Inventory on the right: same size, height and row, 15 from the edge.
    /// - "Stats" title (Big, bold), Main/Secondary tabs with the tab art, a scrolling list of rows
    ///   (RectMask2D viewport, auto-hiding scrollbar; StatsPaneUI switches the content per tab and keeps the
    ///   gamepad selection in view), a one-line legend with a Tiny glyph.
    /// - Each StatRow is a full-width 32-high dark band: star, name, value (green/red).
    /// Batchmode: -executeMethod Game.UI.Style.Editor.StatsRestyler.Restyle
    /// </summary>
    public static class StatsRestyler
    {
        public const string PanePath = "Assets/Prefabs/UI/Progression/StatsPane.prefab";
        public const string RowPath = "Assets/Prefabs/UI/Progression/StatRow.prefab";
        private static readonly string[] Scenes = { "Assets/Scenes/UIMain.unity", "Assets/Scenes/LevelSelector.unity" };

        public const float RowHeight = 32f;
        public const float RowSpacing = 4f;
        public const float TabsHeight = 32f;
        public const float LegendHeight = 28f;
        public const float ScrollbarWidth = 6f;
        private const float RowInset = 8f;
        private const float ValueWidth = 60f;

        // The band behind each row: a soft slot frame, brighter while hovered or selected
        private static readonly Color BandColor = Color.white;
        private static readonly Color BandNormal = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color BandHighlighted = new Color(1f, 1f, 1f, 0.85f);
        private static readonly Color ScrollTrackColor = new Color(0f, 0f, 0f, 0.35f);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Stats Pane")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[StatsRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(RowPath, root => RestyleRow(root, settings));
            UpgradeRestyler.EditPrefab(PanePath, root => RestylePane(root, settings));
            foreach (string scene in Scenes)
            {
                UIRevampTools.RunOnScenes(new[] { scene }, (s, path) => UpgradeRestyler.SnapSceneInstances(s, PanePath));
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[StatsRestyler] Stats pane restyled.");
        }

        /// <summary>The pane: the Inventory's size, mirrored on the right edge.</summary>
        public static void PlacePane(RectTransform rect, UISettings s)
        {
            Place(rect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-s.Margin, s.Margin), InventoryRestyler.PaneSize(s));
        }

        // --- Pane ---------------------------------------------------------------------------------------

        private static void RestylePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            PlacePane((RectTransform)root.transform, s);

            // "Scroll" is the window: the frame, the UIWindow and the ScrollRect
            var window = Find(root.transform, "Scroll");
            CenterWindowLayout.FrameWindow(window, s);
            CenterWindowLayout.UseStandardMarker(window.GetComponent<Navigation.UIWindow>());

            var title = Find(window, "Title").GetComponent<TextMeshProUGUI>();
            title.text = "Stats";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Text);

            // Tabs under the title, sharing the content width
            float tabsTop = m + InventoryRestyler.TitleRowHeight + s.Spacing;
            float tabWidth = (InventoryRestyler.PaneSize(s).x - 2f * m - s.Spacing) * 0.5f;
            RestyleTab(Find(window, "MainButton"), "Main", new Vector2(0f, 1f), new Vector2(m, -tabsTop), tabWidth, s);
            RestyleTab(Find(window, "SecondaryButton"), "Secondary", new Vector2(1f, 1f), new Vector2(-m, -tabsTop), tabWidth, s);

            // The list between the tabs and the legend; the scrollbar sits in the margin on the right
            float listTop = tabsTop + TabsHeight + s.Spacing;
            float listBottom = m + LegendHeight + s.Spacing;
            var view = Find(window, "View");
            Stretch(view, Vector2.zero, Vector2.one, m, listBottom, m + ScrollbarWidth + 4f, listTop);
            var viewImage = view.GetComponent<Image>();
            if (viewImage != null) viewImage.enabled = false;
            GetOrAdd<RectMask2D>(view.gameObject).enabled = true;

            RectTransform primary = Find(view, "Primary");
            RectTransform secondary = Find(view, "Secondary");
            RestyleList(primary, s);
            RestyleList(secondary, s);

            var scrollbar = BuildScrollbar(window, s, listTop, listBottom);

            var scroll = GetOrAdd<ScrollRect>(window.gameObject);
            scroll.enabled = true;
            scroll.viewport = view;
            scroll.content = primary;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.scrollSensitivity = RowHeight;
            scroll.horizontalScrollbar = null;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scroll.verticalScrollbarSpacing = 0f;

            RestyleLegend(Find(window, "InputPrompts"), s);

            var ui = new SerializedObject(root.GetComponent<StatsPaneUI>());
            ui.FindProperty("scrollRect").objectReferenceValue = scroll;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RestyleTab(RectTransform tab, string label, Vector2 anchor, Vector2 position, float width, UISettings s)
        {
            Place(tab, anchor, anchor, position, new Vector2(width, TabsHeight));
            var button = tab.GetComponent<Button>();
            var image = button.targetGraphic as Image;
            if (image != null)
            {
                StretchFull(image.rectTransform);
                image.sprite = s.GetTabSprite(false);
                image.type = Image.Type.Sliced;
                image.preserveAspect = false;
                image.color = Color.white;
            }
            // StatsPaneUI swaps the sprite for the selected tab; hover only tints
            button.transition = Selectable.Transition.ColorTint;
            var colors = ColorBlock.defaultColorBlock;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.disabledColor = Color.white;
            button.colors = colors;

            var text = tab.GetComponentInChildren<TextMeshProUGUI>(true);
            text.text = label;
            StretchFull(text.rectTransform);
            CenterWindowLayout.PlainText(text, TextAlignmentOptions.Center);
            StyleText(text, UIFontSize.Small, false, UIColorRole.None, s);
        }

        // A tab's rows: top-anchored scroll content whose layout sets each row's width and height
        private static void RestyleList(RectTransform list, UISettings s)
        {
            list.anchorMin = new Vector2(0f, 1f);
            list.anchorMax = new Vector2(1f, 1f);
            list.pivot = new Vector2(0.5f, 1f);
            list.anchoredPosition = Vector2.zero;
            list.sizeDelta = Vector2.zero;

            var layout = GetOrAdd<VerticalLayoutGroup>(list.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = RowSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = GetOrAdd<ContentSizeFitter>(list.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static Scrollbar BuildScrollbar(RectTransform window, UISettings s, float top, float bottom)
        {
            var track = FindOrCreateImage(window, "Scrollbar");
            var rect = track.rectTransform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(-s.Margin - ScrollbarWidth, bottom);
            rect.offsetMax = new Vector2(-s.Margin, -top);
            track.sprite = s.SolidFill;
            track.type = Image.Type.Simple;
            track.color = ScrollTrackColor;

            var area = FindOrCreate(rect, "Sliding Area");
            StretchFull(area);
            var handle = FindOrCreateImage(area, "Handle");
            StretchFull(handle.rectTransform);
            handle.sprite = s.SolidFill;
            handle.color = s.MutedTextColor;

            var scrollbar = GetOrAdd<Scrollbar>(track.gameObject);
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.targetGraphic = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            // The gamepad moves through the rows, never onto the scrollbar
            scrollbar.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            return scrollbar;
        }

        // One line at the bottom: [glyph] Hold to see the items granting you a stat
        private static void RestyleLegend(RectTransform prompts, UISettings s)
        {
            prompts.anchorMin = new Vector2(0f, 0f);
            prompts.anchorMax = new Vector2(1f, 0f);
            prompts.pivot = new Vector2(0.5f, 0f);
            prompts.offsetMin = new Vector2(s.Margin, s.Margin);
            prompts.offsetMax = new Vector2(-s.Margin, s.Margin + LegendHeight);

            foreach (Transform group in prompts)
            {
                var groupRect = (RectTransform)group;
                StretchFull(groupRect);
                foreach (var glyph in group.GetComponentsInChildren<Image>(true))
                {
                    Place(glyph.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.one * s.TinyIconSize);
                    glyph.rectTransform.pivot = new Vector2(0f, 0.5f);
                    glyph.rectTransform.anchoredPosition = Vector2.zero;
                    glyph.preserveAspect = true;
                    glyph.raycastTarget = false;
                    StyleIcon(glyph.gameObject, UIIconSize.Tiny, s);
                }
                foreach (var text in group.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    Stretch(text.rectTransform, Vector2.zero, Vector2.one, s.TinyIconSize + 6f, 0f, 0f, 0f);
                    CenterWindowLayout.PlainText(text, TextAlignmentOptions.MidlineLeft);
                    text.textWrappingMode = TextWrappingModes.NoWrap;
                    StyleText(text, UIFontSize.Small, false, UIColorRole.Muted, s);
                }
            }
        }

        // --- StatRow: one full-width band --------------------------------------------------------------

        private static void RestyleRow(GameObject root, UISettings s)
        {
            var rect = (RectTransform)root.transform;
            Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(InventoryRestyler.PaneSize(s).x - 2f * s.Margin, RowHeight));

            var layout = GetOrAdd<LayoutElement>(root);
            layout.ignoreLayout = false;
            layout.minHeight = RowHeight;
            layout.preferredHeight = RowHeight;
            layout.flexibleWidth = 1f;

            // The band is the row's own image, so the gamepad frame and hover cover the whole row
            var band = root.GetComponent<Image>();
            band.sprite = s.SlotFrame;
            band.type = Image.Type.Sliced;
            band.color = BandColor;
            band.raycastTarget = true;

            var selectable = root.GetComponent<Selectable>();
            selectable.targetGraphic = band;
            selectable.transition = Selectable.Transition.ColorTint;
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = BandNormal;
            colors.highlightedColor = BandHighlighted;
            colors.selectedColor = BandHighlighted;
            colors.pressedColor = Color.white;
            colors.disabledColor = BandNormal;
            colors.fadeDuration = 0.08f;
            selectable.colors = colors;

            // The old content holder becomes a plain full-row container
            var content = Find(root.transform, "Image");
            StretchFull(content);
            var contentImage = content.GetComponent<Image>();
            if (contentImage != null) contentImage.raycastTarget = false;

            float star = s.TinyIconSize;
            var roleStar = Find(content, "RoleStar");
            Place(roleStar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(RowInset * 0.5f, 0f), Vector2.one * star);
            roleStar.pivot = new Vector2(0f, 0.5f);
            roleStar.anchoredPosition = new Vector2(RowInset * 0.5f, 0f);
            StyleIcon(roleStar.gameObject, UIIconSize.Tiny, s);

            var label = Find(content, "Label").GetComponent<TextMeshProUGUI>();
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, RowInset * 0.5f + star + 4f, 0f, ValueWidth + RowInset, 0f);
            CenterWindowLayout.PlainText(label, TextAlignmentOptions.MidlineLeft);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            StyleText(label, UIFontSize.Small, false, UIColorRole.None, s);

            var value = Find(content, "Value").GetComponent<TextMeshProUGUI>();
            value.text = "20";
            Place(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-RowInset, 0f), new Vector2(ValueWidth, RowHeight));
            value.rectTransform.pivot = new Vector2(1f, 0.5f);
            value.rectTransform.anchoredPosition = new Vector2(-RowInset, 0f);
            CenterWindowLayout.PlainText(value, TextAlignmentOptions.MidlineRight);
            value.textWrappingMode = TextWrappingModes.NoWrap;
            value.raycastTarget = false;
            StyleText(value, UIFontSize.Small, false, UIColorRole.None, s);

            // Everything in a row must be clipped by the list's mask; the texts were not maskable, so rows
            // scrolled out of view kept drawing over the legend and the frame
            foreach (var graphic in root.GetComponentsInChildren<MaskableGraphic>(true))
            {
                graphic.maskable = true;
            }
        }
    }
}
