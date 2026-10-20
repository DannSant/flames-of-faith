using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 1: restyles the gameplay HUD (HealthPane, IconsPane, ExperiencePane,
    /// FlameProgressionBar, WaveHintUI) to the UI standard in place, and lays them out in UIMain:
    /// everything 15 units (UISettings.Margin) from the screen edges at the 1920x1080 reference,
    /// HealthPane and the wave hint stacked top-left. Run it again after changing the layout here.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.HudRestyler.Restyle
    /// </summary>
    public static class HudRestyler
    {
        private const string MainUI = "Assets/Prefabs/UI/MainUI/";
        public const string HealthPanePath = MainUI + "HealthPane.prefab";
        public const string IconsPanePath = MainUI + "IconsPane.prefab";
        public const string ExperiencePanePath = MainUI + "ExperiencePane.prefab";
        public const string FlameBarPath = MainUI + "FlameProgressionBar.prefab";
        public const string WaveHintPath = MainUI + "WaveHintUI.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";
        private const string RedFill = "Assets/AssetPackages/ClassicPixelRPGUI/Artwok/UI/Hp_orange.png";

        public static readonly string[] HudPrefabs = { HealthPanePath, IconsPanePath, ExperiencePanePath, FlameBarPath, WaveHintPath };

        // Layout, in canvas units at the reference resolution
        public const float TopLeftWidth = 430f;
        private const float BarFrameInset = 3f;
        private const float HealthBarHeight = 22f;
        private const float GraceBarHeight = 14f;
        private const float LabelHeight = 16f;
        private const float WaveBlockWidth = 80f;
        private const float SlotSpacing = 15f;
        private const float ToggleColumnWidth = 160f;
        private const float ExperienceWidth = 300f;
        private const float FlameBarWidth = 600f;
        private const float FlameBarHeight = 22f;
        public const string TopLeftStackName = "TopLeftHud";

        private static readonly Color BarBackgroundColor = new Color(0.08f, 0.06f, 0.06f, 0.9f);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle HUD")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[HudRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            EditPrefab(HealthPanePath, root => RestyleHealthPane(root, settings));
            EditPrefab(IconsPanePath, root => RestyleIconsPane(root, settings));
            EditPrefab(ExperiencePanePath, root => RestyleExperiencePane(root, settings));
            EditPrefab(FlameBarPath, root => RestyleFlameBar(root, settings));
            EditPrefab(WaveHintPath, root => RestyleWaveHint(root, settings));

            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) => LayoutUIMain(scene, settings));

            AssetDatabase.SaveAssets();
            Debug.Log("[HudRestyler] HUD restyled.");
        }

        private static void EditPrefab(string path, System.Action<GameObject> edit)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                edit(root);
                SetLayerRecursively(root, UILayer);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // --- HealthPane ---------------------------------------------------------------------------------

        public static float HealthPaneHeight(UISettings s) =>
            s.Margin + HealthBarHeight + s.Spacing + GraceBarHeight + 4f + LabelHeight + s.Margin;

        private static void RestyleHealthPane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float height = HealthPaneHeight(s);
            float barsWidth = TopLeftWidth - 2f * m - WaveBlockWidth - s.Spacing;

            var rootRect = (RectTransform)root.transform;
            Place(rootRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m, -m), new Vector2(TopLeftWidth, height));
            SetLayoutSize(root, TopLeftWidth, height);
            StyleFrame(root.GetComponent<Image>(), UIFrameKind.Panel, s);

            // Health bar
            var healthBar = Find(root.transform, "Healthbar");
            Place(healthBar, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m, -m), new Vector2(barsWidth, HealthBarHeight));
            RestyleSliderBar(healthBar, s, null);
            var healthText = Find(healthBar, "Text (TMP)").GetComponent<TextMeshProUGUI>();
            StretchFull(healthText.rectTransform);
            healthText.alignment = TextAlignmentOptions.Center;
            StyleText(healthText, UIFontSize.Small, true, UIColorRole.Text, s);
            healthText.transform.SetAsLastSibling();

            // Grace bar: one bar centered on 0, and its two labels under it
            var graceGroup = Find(root.transform, "GraceCorruptionBars");
            var deadSlider = graceGroup.GetComponent<Slider>();
            if (deadSlider != null) Object.DestroyImmediate(deadSlider);

            float graceTop = m + HealthBarHeight + s.Spacing;
            float graceGroupHeight = GraceBarHeight + 4f + LabelHeight;
            Place(graceGroup, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m, -graceTop), new Vector2(barsWidth, graceGroupHeight));

            var bar = FindOrCreate(graceGroup, "Bar");
            bar.SetSiblingIndex(0);
            Stretch(bar, new Vector2(0f, 1f), new Vector2(1f, 1f));
            bar.pivot = new Vector2(0.5f, 1f);
            bar.anchoredPosition = Vector2.zero;
            bar.sizeDelta = new Vector2(0f, GraceBarHeight);

            var background = Find(graceGroup, "Background") ?? Find(bar, "Background");
            background.SetParent(bar, false);
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = null;
            backgroundImage.color = BarBackgroundColor;
            backgroundImage.raycastTarget = false;
            StretchFull(background, BarFrameInset);
            background.SetSiblingIndex(0);

            // Split at the middle by anchors, so it stays centered at any size
            var negative = Find(graceGroup, "FillCorruption") ?? Find(bar, "FillCorruption");
            negative.SetParent(bar, false);
            Stretch(negative, new Vector2(0f, 0f), new Vector2(0.5f, 1f), BarFrameInset, BarFrameInset, 0f, BarFrameInset);
            SetupTintedFill(negative.GetComponent<Image>(), s.BarFill, s.CorruptionColor, (int)Image.OriginHorizontal.Right);
            negative.SetSiblingIndex(1);

            var positive = Find(graceGroup, "FillGrace") ?? Find(bar, "FillGrace");
            positive.SetParent(bar, false);
            Stretch(positive, new Vector2(0.5f, 0f), new Vector2(1f, 1f), 0f, BarFrameInset, BarFrameInset, BarFrameInset);
            SetupTintedFill(positive.GetComponent<Image>(), s.BarFill, s.GraceColor, (int)Image.OriginHorizontal.Left);
            positive.SetSiblingIndex(2);

            var graceBorder = FindOrCreateImage(bar, "Border");
            StretchFull(graceBorder.rectTransform);
            graceBorder.raycastTarget = false;
            StyleFrame(graceBorder, UIFrameKind.Bar, s);
            graceBorder.transform.SetSiblingIndex(3);

            // Labels: Grace left (cyan / purple, colored by GraceBar), Corruption right (purple)
            var graceText = Find(graceGroup, "GraceTxt").GetComponent<TextMeshProUGUI>();
            Place(graceText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(barsWidth * 0.5f, LabelHeight));
            graceText.alignment = TextAlignmentOptions.MidlineLeft;
            graceText.fontStyle = FontStyles.Normal;
            graceText.color = s.GraceColor;
            graceText.raycastTarget = true; // tooltip
            StyleText(graceText, UIFontSize.Small, true, UIColorRole.None, s);

            var corruptionText = Find(graceGroup, "CorruptionTxt").GetComponent<TextMeshProUGUI>();
            Place(corruptionText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(barsWidth * 0.5f, LabelHeight));
            corruptionText.alignment = TextAlignmentOptions.MidlineRight;
            corruptionText.fontStyle = FontStyles.Normal;
            corruptionText.raycastTarget = true; // tooltip
            StyleText(corruptionText, UIFontSize.Small, true, UIColorRole.Corruption, s);

            // Wave block on the right: "Wave 3" small, timer big (purple during the Corruptor phase, set by WaveUI)
            var wavePane = Find(root.transform, "WavePane");
            float contentHeight = height - 2f * m;
            Place(wavePane, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-m, -m), new Vector2(WaveBlockWidth, contentHeight));

            float waveTextsHeight = LabelHeight + 2f + 26f;
            float waveTop = (contentHeight - waveTextsHeight) * 0.5f;
            var waveNumber = Find(wavePane, "WaveNumberText").GetComponent<TextMeshProUGUI>();
            Place(waveNumber.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -waveTop), new Vector2(WaveBlockWidth, LabelHeight));
            SetupPlainText(waveNumber, TextAlignmentOptions.Center);
            StyleText(waveNumber, UIFontSize.Small, true, UIColorRole.Text, s);

            var waveTimer = Find(wavePane, "WaveTimerText").GetComponent<TextMeshProUGUI>();
            Place(waveTimer.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -(waveTop + LabelHeight + 2f)), new Vector2(WaveBlockWidth, 26f));
            SetupPlainText(waveTimer, TextAlignmentOptions.Center);
            waveTimer.color = s.TextColor;
            StyleText(waveTimer, UIFontSize.Big, true, UIColorRole.None, s);
        }

        // --- IconsPane ----------------------------------------------------------------------------------

        public static Vector2 IconsPaneSize(UISettings s)
        {
            float icon = s.MediumIconSize;
            float width = s.Margin + 3f * icon + 2f * SlotSpacing + SlotSpacing + ToggleColumnWidth + s.Margin;
            // The input glyphs straddle the icons' bottom edge, so leave them half a glyph of room
            float height = s.Margin + icon + s.PromptIconSize * 0.5f + s.Margin;
            return new Vector2(width, height);
        }

        private static readonly (string slot, string glyph, string overlay)[] AbilitySlots =
        {
            ("MainAttackButton", "AttackInputIcon", "AttackBG"),
            ("SecondaryAttackButton", "SpecialAttackInputIcon", "SecondaryAttackCooldownBG"),
            ("DashButton", "DashInputIcon", "DashCooldownBg"),
        };

        private static void RestyleIconsPane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float icon = s.MediumIconSize;
            float prompt = s.PromptIconSize;
            Vector2 size = IconsPaneSize(s);

            Place((RectTransform)root.transform, Vector2.zero, Vector2.zero, new Vector2(m, m), size);

            // Panel is the frame; children are placed by hand so the glyphs can sit on the slots
            var panel = Find(root.transform, "Panel");
            StretchFull(panel);
            var layout = panel.GetComponent<HorizontalLayoutGroup>();
            if (layout != null) Object.DestroyImmediate(layout);
            var frame = panel.GetComponent<Image>();
            frame.raycastTarget = false;
            StyleFrame(frame, UIFrameKind.Panel, s);

            for (int i = 0; i < AbilitySlots.Length; i++)
            {
                var (slotName, glyphName, overlayName) = AbilitySlots[i];
                var slot = Find(panel, slotName);
                Place(slot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m + i * (icon + SlotSpacing), -m), new Vector2(icon, icon));
                StyleIcon(slot.gameObject, UIIconSize.Medium, s);

                // The cooldown sweep covers the icon exactly. Unity's built-in UISprite it used has rounded,
                // see-through edges, so the sweep looked smaller than the icon
                var sweep = Find(slot, overlayName);
                StretchFull(sweep);
                var sweepImage = sweep.GetComponent<Image>();
                sweepImage.sprite = s.SolidFill;
                sweepImage.type = Image.Type.Filled;
                sweepImage.fillMethod = Image.FillMethod.Radial360;
                sweepImage.preserveAspect = false;
                sweepImage.raycastTarget = false;

                // Input glyph centered on the slot's bottom edge, drawn over the sweep
                var glyph = Find(root.transform, glyphName) ?? Find(slot, glyphName);
                glyph.SetParent(slot, false);
                RemoveGlyphLeftovers(glyph);
                Place(glyph, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(prompt, prompt));
                SetupGlyph(glyph, s);
                glyph.SetAsLastSibling();
            }

            // Dash charges, top-right corner of the dash icon
            var dash = Find(panel, "DashButton");
            var charges = Find(dash, "Text (TMP)").GetComponent<TextMeshProUGUI>();
            Place(charges.rectTransform, Vector2.one, Vector2.one, new Vector2(-4f, -2f), new Vector2(40f, 26f));
            SetupPlainText(charges, TextAlignmentOptions.TopRight);
            StyleText(charges, UIFontSize.Big, true, UIColorRole.Text, s);

            // Toggle rows to the right of the slots: [glyph] Auto-attack, [glyph] Lock-On
            float columnX = m + 3f * icon + 3f * SlotSpacing;
            float rowSpacing = 10f;
            float rowsTop = m + (icon - (2f * prompt + rowSpacing)) * 0.5f;
            PlaceToggleRow(root, panel, "AutoAttackInput", "AutoAttackLabel", columnX, rowsTop, UIColorRole.None, s);
            PlaceToggleRow(root, panel, "LockonInput", "LockonLabel", columnX, rowsTop + prompt + rowSpacing, UIColorRole.Text, s);
        }

        private static void PlaceToggleRow(GameObject root, RectTransform panel, string glyphName, string labelName,
            float x, float top, UIColorRole labelColor, UISettings s)
        {
            float prompt = s.PromptIconSize;
            var glyph = Find(root.transform, glyphName) ?? Find(panel, glyphName);
            glyph.SetParent(panel, false);
            RemoveGlyphLeftovers(glyph);
            Place(glyph, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -top), new Vector2(prompt, prompt));
            SetupGlyph(glyph, s);

            var label = Find(panel, labelName).GetComponent<TextMeshProUGUI>();
            float labelX = x + prompt + 6f;
            Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(labelX, -top), new Vector2(ToggleColumnWidth - prompt - 6f, prompt));
            SetupPlainText(label, TextAlignmentOptions.MidlineLeft);
            StyleText(label, UIFontSize.Small, true, labelColor, s);
        }

        // Glyph sprites aren't square (SPACE, CTRL...): keep their shape inside the Prompt-sized box
        private static void SetupGlyph(RectTransform glyph, UISettings s)
        {
            var image = glyph.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            StyleIcon(glyph.gameObject, UIIconSize.Prompt, s);
        }

        // Each glyph had an unused copy of the cooldown sweep under it
        private static void RemoveGlyphLeftovers(Transform glyph)
        {
            foreach (var child in glyph.Cast<Transform>().ToList())
            {
                if (child.name == "AttackBG") Object.DestroyImmediate(child.gameObject);
            }
        }

        // --- ExperiencePane -----------------------------------------------------------------------------

        private static void RestyleExperiencePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float height = LabelHeight + 4f + HealthBarHeight * 0.75f;
            Place((RectTransform)root.transform, Vector2.one, Vector2.one, new Vector2(-m, -m), new Vector2(ExperienceWidth, height));

            var slider = Find(root.transform, "ExpSlider");
            slider.anchorMin = Vector2.zero;
            slider.anchorMax = new Vector2(1f, 0f);
            slider.pivot = new Vector2(0.5f, 0f);
            slider.offsetMin = Vector2.zero;
            slider.offsetMax = new Vector2(0f, HealthBarHeight * 0.75f);
            slider.localScale = Vector3.one;
            RestyleSliderBar(slider, s, null);

            var level = Find(slider, "LevelTxt").GetComponent<TextMeshProUGUI>();
            Place(level.rectTransform, Vector2.one, new Vector2(1f, 0f), new Vector2(0f, 4f), new Vector2(ExperienceWidth, LabelHeight));
            SetupPlainText(level, TextAlignmentOptions.BottomRight);
            StyleText(level, UIFontSize.Small, true, UIColorRole.Text, s);
        }

        // --- FlameProgressionBar ------------------------------------------------------------------------

        private static void RestyleFlameBar(GameObject root, UISettings s)
        {
            Place((RectTransform)root.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, s.Margin), new Vector2(FlameBarWidth, FlameBarHeight));

            var slider = Find(root.transform, "FlameSlider");
            StretchFull(slider);
            // The fill was a placeholder frame of a loading-icon spritesheet
            RestyleSliderBar(slider, s, AssetDatabase.LoadAssetAtPath<Sprite>(RedFill));
        }

        // --- WaveHintUI ---------------------------------------------------------------------------------

        private static void RestyleWaveHint(GameObject root, UISettings s)
        {
            float m = s.Margin;
            var rootRect = (RectTransform)root.transform;
            Place(rootRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m, -m), new Vector2(TopLeftWidth, 0f));

            // Height follows the text; 0 while the panel is hidden, so it takes no room in the stack
            var rootLayout = GetOrAdd<VerticalLayoutGroup>(root);
            ConfigureVertical(rootLayout, 0f, 0f);

            var panel = Find(root.transform, "MainPanel");
            var frame = panel.GetComponent<Image>();
            frame.raycastTarget = false;
            StyleFrame(frame, UIFrameKind.Panel, s);
            ConfigureVertical(GetOrAdd<VerticalLayoutGroup>(panel.gameObject), m, 0f);

            var text = Find(panel, "Text (TMP)").GetComponent<TextMeshProUGUI>();
            SetupPlainText(text, TextAlignmentOptions.TopLeft);
            text.textWrappingMode = TextWrappingModes.Normal;
            StyleText(text, UIFontSize.Small, true, UIColorRole.None, s);
        }

        // --- UIMain layout ------------------------------------------------------------------------------

        private static bool LayoutUIMain(UnityEngine.SceneManagement.Scene scene, UISettings s)
        {
            var instances = new Dictionary<string, GameObject>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;
                    string path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject));
                    if (HudPrefabs.Contains(path) && !instances.ContainsKey(path)) instances[path] = transform.gameObject;
                }
            }

            foreach (string path in HudPrefabs)
            {
                if (!instances.ContainsKey(path))
                {
                    Debug.LogError($"[HudRestyler] No {path} instance in {UIMainScene}");
                    return false;
                }
            }

            // Panes placed on their own get the prefab's root layout (a revert can't remove root layout
            // overrides). HealthPane and the hint are placed by the TopLeftHud stack instead.
            // Nothing else on the instances is reverted.
            MatchPrefabRootLayout(instances[IconsPanePath]);
            MatchPrefabRootLayout(instances[ExperiencePanePath]);
            MatchPrefabRootLayout(instances[FlameBarPath]);

            var healthPane = instances[HealthPanePath];
            var waveHint = instances[WaveHintPath];
            var canvas = healthPane.transform.parent.name == TopLeftStackName ? healthPane.transform.parent.parent : healthPane.transform.parent;

            // Placed and configured only when created, so later hand tweaks in UIMain survive a re-run
            bool created = Find(canvas, TopLeftStackName) == null;
            var stack = FindOrCreate(canvas, TopLeftStackName);
            if (created)
            {
                stack.SetSiblingIndex(healthPane.transform.GetSiblingIndex());
                Place(stack, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(s.Margin, -s.Margin), new Vector2(TopLeftWidth, 600f));
                ConfigureVertical(GetOrAdd<VerticalLayoutGroup>(stack.gameObject), 0f, s.Margin);
            }

            healthPane.transform.SetParent(stack, false);
            healthPane.transform.SetSiblingIndex(0);
            waveHint.transform.SetParent(stack, false);
            waveHint.transform.SetSiblingIndex(1);
            return true;
        }

        // --- Shared pieces ------------------------------------------------------------------------------

        /// <summary>
        /// A Slider bar (Wrapper/FillImage + Border): the slider drives FillImage's anchors inside Wrapper,
        /// which sits inside the bar frame.
        /// </summary>
        private static void RestyleSliderBar(RectTransform bar, UISettings s, Sprite fillSprite)
        {
            var wrapper = Find(bar, "Wrapper");
            StretchFull(wrapper, BarFrameInset);
            wrapper.SetSiblingIndex(0);

            var fill = Find(wrapper, "FillImage");
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImage = fill.GetComponent<Image>();
            fillImage.raycastTarget = false;
            if (fillSprite != null) fillImage.sprite = fillSprite;

            var border = Find(bar, "Border").GetComponent<Image>();
            StretchFull(border.rectTransform);
            border.raycastTarget = false;
            StyleFrame(border, UIFrameKind.Bar, s);
            border.transform.SetSiblingIndex(1);
        }

        private static void SetupTintedFill(Image image, Sprite sprite, Color color, int origin)
        {
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = origin;
            image.raycastTarget = false;
        }

        private static void SetupPlainText(TextMeshProUGUI text, TextAlignmentOptions alignment)
        {
            text.alignment = alignment;
            text.characterSpacing = 0f;
            text.fontStyle = FontStyles.Normal;
            text.margin = Vector4.zero;
        }

        private static void ConfigureVertical(VerticalLayoutGroup layout, float padding, float spacing)
        {
            int p = Mathf.RoundToInt(padding);
            layout.padding = new RectOffset(p, p, p, p);
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
        }

        private static void SetLayoutSize(GameObject go, float width, float height)
        {
            var element = GetOrAdd<LayoutElement>(go);
            element.preferredWidth = width;
            element.preferredHeight = height;
            element.minHeight = height;
        }
    }
}
