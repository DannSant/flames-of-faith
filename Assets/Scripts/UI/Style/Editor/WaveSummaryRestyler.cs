using Game.UI.Navigation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 9: restyles the end-of-wave summary (WaveCorruptionSummaryUI) in place as a center window:
    /// "Wave Results" (Big, bold, yellow), a column of Small receipt rows, the Big formula, a Grace bar built like
    /// the HUD's, the Big gold Favors row, and a Continue button in the progress spot (it reads Skip while the
    /// summary plays). The old "Press (A) / Click to continue" prompt row is removed.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.WaveSummaryRestyler.Restyle
    /// </summary>
    public static class WaveSummaryRestyler
    {
        public const string PanePath = "Assets/Prefabs/UI/Waves/WaveCorruptionSummaryUI.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        public const int WindowPriority = 50;
        public const float CountDuration = 2f;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Wave Results")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[WaveSummaryRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(PanePath, root => RestylePane(root, settings));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) =>
            {
                // Revert first: a revert reloads the instance, which would drop the snapped root layout
                bool changed = RevertNameOverride(scene);
                changed |= RevertChildLayoutOverrides(scene);
                return UpgradeRestyler.SnapSceneInstances(scene, PanePath) || changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[WaveSummaryRestyler] Wave results restyled.");
        }

        private static void RestylePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            CenterWindowLayout.PlaceWindow((RectTransform)root.transform, s);

            var mainPanel = Find(root.transform, "MainPanel");
            var oldColumn = mainPanel.GetComponent<VerticalLayoutGroup>();
            if (oldColumn != null) Object.DestroyImmediate(oldColumn);
            CenterWindowLayout.FrameWindow(mainPanel, s);

            var ui = new SerializedObject(root.GetComponent<WaveCorruptionSummaryUI>());
            TextMeshProUGUI Text(string field) => ui.FindProperty(field).objectReferenceValue as TextMeshProUGUI;

            var title = (Find(mainPanel, "Title") ?? Find(mainPanel, "TittleTxt")).GetComponent<TextMeshProUGUI>();
            title.name = "Title";
            title.text = "Wave Results";
            title.transform.SetSiblingIndex(0);
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            // Content: everything between the title and the button, in reveal order; hidden rows collapse
            var content = FindOrCreate(mainPanel, "Content");
            content.SetSiblingIndex(1);
            Stretch(content, Vector2.zero, Vector2.one, m, CenterWindowLayout.ButtonsTop(s, 1) + m, m,
                m + InventoryRestyler.TitleRowHeight + s.Spacing);
            var column = GetOrAdd<VerticalLayoutGroup>(content.gameObject);
            column.padding = new RectOffset(0, 0, 0, 0);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;

            string corruptionHex = s.ToHex(UIColorRole.Corruption);
            string graceHex = s.ToHex(UIColorRole.Grace);
            var rows = new (TextMeshProUGUI text, string sample)[]
            {
                (Text("corruptorRowText"), WaveCorruptionSummaryUI.FormatRow("Corruptor killed in 8.2s", "-2", corruptionHex)),
                (Text("corruptedDamageRowText"), WaveCorruptionSummaryUI.FormatRow("Corrupted damage taken: 40", "-3", corruptionHex)),
                (Text("totalCorruptionRowText"), WaveCorruptionSummaryUI.FormatRow("Corruption this wave", "-5", corruptionHex)),
                (Text("graceAffinityRowText"), WaveCorruptionSummaryUI.FormatRow("Grace Affinity", "+1", graceHex)),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i].text;
                row.transform.SetParent(content, false);
                row.transform.SetSiblingIndex(i);
                row.text = rows[i].sample;
                StyleColumnText(row, TextAlignmentOptions.Left, UIFontSize.Small, UIColorRole.Text, s);
            }

            var gap = FindOrCreate(content, "Gap");
            gap.SetSiblingIndex(rows.Length);
            var gapLayout = GetOrAdd<LayoutElement>(gap.gameObject);
            gapLayout.minHeight = m;
            gapLayout.preferredHeight = m;

            var formula = Text("formulaText");
            formula.transform.SetParent(content, false);
            formula.transform.SetSiblingIndex(rows.Length + 1);
            formula.text = WaveCorruptionSummaryUI.FormatFormulaLeft(5f, 1f, 5, graceHex, corruptionHex) + $" = <color=#{graceHex}>+1</color>";
            StyleColumnText(formula, TextAlignmentOptions.Center, UIFontSize.Big, UIColorRole.Text, s);

            var meter = BuildGraceMeter(content, s, out var positive, out var negative);
            meter.SetSiblingIndex(rows.Length + 2);

            var favors = Text("favorsRowText");
            favors.transform.SetParent(content, false);
            favors.transform.SetSiblingIndex(rows.Length + 3);
            favors.text = WaveCorruptionSummaryUI.FormatFavors(1);
            StyleColumnText(favors, TextAlignmentOptions.Center, UIFontSize.Big, UIColorRole.Favors, s);

            // The old "Press (A) / Click to continue" row: the Continue button replaces it
            DestroyChild(mainPanel, "InputPromptRow");

            var button = BuildContinueButton(mainPanel, s, out var label);

            var window = mainPanel.GetComponent<UIWindow>();
            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("priority").intValue = WindowPriority;
            windowSo.FindProperty("defaultSelectable").objectReferenceValue = button;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            CenterWindowLayout.UseStandardMarker(window);

            ui.FindProperty("continueButton").objectReferenceValue = button;
            ui.FindProperty("continueLabel").objectReferenceValue = label;
            ui.FindProperty("graceMeterPositive").objectReferenceValue = positive;
            ui.FindProperty("graceMeterNegative").objectReferenceValue = negative;
            ui.FindProperty("countDuration").floatValue = CountDuration;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void StyleColumnText(TextMeshProUGUI text, TextAlignmentOptions alignment, UIFontSize size, UIColorRole color, UISettings s)
        {
            var element = text.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = false;
            CenterWindowLayout.PlainText(text, alignment);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            text.raycastTarget = false;
            StyleText(text, size, false, color, s);
        }

        /// <summary>The summary's Grace bar, built like the HUD's (HudRestyler): split at the middle, plus a tick at 0.</summary>
        private static RectTransform BuildGraceMeter(RectTransform parent, UISettings s, out Image positive, out Image negative)
        {
            float inset = HudRestyler.BarFrameInset;
            var meter = FindOrCreate(parent, "GraceMeter");
            var layout = GetOrAdd<LayoutElement>(meter.gameObject);
            layout.minHeight = HudRestyler.GraceBarHeight;
            layout.preferredHeight = HudRestyler.GraceBarHeight;

            var background = FindOrCreateImage(meter, "Background");
            background.sprite = null;
            background.color = HudRestyler.BarBackgroundColor;
            background.raycastTarget = false;
            StretchFull(background.rectTransform, inset);
            background.transform.SetSiblingIndex(0);

            negative = FindOrCreateImage(meter, "FillCorruption");
            Stretch(negative.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), inset, inset, 0f, inset);
            HudRestyler.SetupTintedFill(negative, s.BarFill, s.CorruptionColor, (int)Image.OriginHorizontal.Right);
            negative.fillAmount = 0f;
            negative.transform.SetSiblingIndex(1);

            positive = FindOrCreateImage(meter, "FillGrace");
            Stretch(positive.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), 0f, inset, inset, inset);
            HudRestyler.SetupTintedFill(positive, s.BarFill, s.GraceColor, (int)Image.OriginHorizontal.Left);
            positive.fillAmount = 0.1f;
            positive.transform.SetSiblingIndex(2);

            var tick = FindOrCreateImage(meter, "Tick");
            Place(tick.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2f, HudRestyler.GraceBarHeight));
            tick.sprite = s.SolidFill;
            tick.color = s.MutedTextColor;
            tick.raycastTarget = false;
            tick.transform.SetSiblingIndex(3);

            var border = FindOrCreateImage(meter, "Border");
            StretchFull(border.rectTransform);
            border.raycastTarget = false;
            StyleFrame(border, UIFrameKind.Bar, s);
            border.transform.SetSiblingIndex(4);
            return meter;
        }

        private static Button BuildContinueButton(RectTransform mainPanel, UISettings s, out TextMeshProUGUI label)
        {
            var rect = FindOrCreate(mainPanel, "ContinueButton");
            GetOrAdd<CanvasRenderer>(rect.gameObject);
            var image = GetOrAdd<Image>(rect.gameObject);
            image.raycastTarget = true;
            var button = GetOrAdd<Button>(rect.gameObject);
            button.targetGraphic = image;

            label = FindOrCreateText(rect, "Label");
            StretchFull(label.rectTransform);
            label.text = "Continue";

            CenterWindowLayout.PlaceStackedButton(rect, s, 0);
            rect.SetAsLastSibling();
            return button;
        }

        public static readonly string[] LayoutProperties = { "m_AnchorMin", "m_AnchorMax", "m_AnchoredPosition", "m_SizeDelta", "m_Pivot" };

        /// <summary>
        /// The old layout group left anchors and positions of the window's children as scene overrides (the title
        /// was pinned to the bottom). The restyler owns these children's layout, so only those properties are
        /// reverted, on this window's instance only (the instance root keeps its own, snapped next).
        /// </summary>
        private static bool RevertChildLayoutOverrides(UnityEngine.SceneManagement.Scene scene)
        {
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var summary in root.GetComponentsInChildren<WaveCorruptionSummaryUI>(true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(summary.gameObject)) continue;
                    foreach (var rect in summary.GetComponentsInChildren<RectTransform>(true))
                    {
                        if (rect == summary.transform) continue;
                        var so = new SerializedObject(rect);
                        foreach (string property in LayoutProperties)
                        {
                            var p = so.FindProperty(property);
                            if (p == null || !p.prefabOverride) continue;
                            PrefabUtility.RevertPropertyOverride(p, InteractionMode.AutomatedAction);
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        // The prefab was renamed (it had a trailing space); the scene still overrode the old name
        private static bool RevertNameOverride(UnityEngine.SceneManagement.Scene scene)
        {
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var summary in root.GetComponentsInChildren<WaveCorruptionSummaryUI>(true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(summary.gameObject)) continue;
                    var name = new SerializedObject(summary.gameObject).FindProperty("m_Name");
                    if (!name.prefabOverride) continue;
                    PrefabUtility.RevertPropertyOverride(name, InteractionMode.AutomatedAction);
                    changed = true;
                }
            }
            return changed;
        }
    }
}
