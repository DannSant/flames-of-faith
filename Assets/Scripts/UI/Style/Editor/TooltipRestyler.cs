using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 6: restyles the shared tooltip (GeneralTooltipPane) in place.
    /// - The root stretches over the canvas and is only the space GeneralTooltipPaneUI places the pane in.
    /// - MainPanel: a 600-wide frame at UISettings.tooltipOpacity whose height follows the content, never a
    ///   raycast target. Title (Big, bold), then a Body row: TextColumn (GeneralTooltipTxt + DetailedTooltipTxt,
    ///   Small) and ListColumn (ListTxt, Small, 150 wide; an item's stat lines).
    /// Batchmode: -executeMethod Game.UI.Style.Editor.TooltipRestyler.Restyle
    /// </summary>
    public static class TooltipRestyler
    {
        public const string PanePath = "Assets/Prefabs/UI/Misc/GeneralTooltipPane.prefab";
        private static readonly string[] Scenes = { "Assets/Scenes/UIMain.unity", "Assets/Scenes/LevelSelector.unity" };

        public const float Width = 600f;
        public const float ListWidth = 150f;
        private const float ListPadding = 8f;
        private static readonly Color ListBand = new Color(0f, 0f, 0f, 0.25f);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Tooltip")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[TooltipRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(PanePath, root => RestylePane(root, settings));
            foreach (string scene in Scenes)
            {
                UIRevampTools.RunOnScenes(new[] { scene }, (s, path) => UpgradeRestyler.SnapSceneInstances(s, PanePath));
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[TooltipRestyler] Tooltip restyled.");
        }

        private static void RestylePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            StretchFull((RectTransform)root.transform);

            // The pane: placed by GeneralTooltipPaneUI (pivot top-left), height from its content
            var panel = Find(root.transform, "MainPanel");
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(Width, 0f);
            var oldGrid = panel.GetComponent<GridLayoutGroup>();
            if (oldGrid != null) Object.DestroyImmediate(oldGrid);

            var frameStyle = GetOrAdd<UIStyleFrame>(panel.gameObject);
            frameStyle.Configure(UIFrameKind.Panel, s.TooltipOpacity);
            frameStyle.Apply(s);

            // Never under the pointer: it can't steal hover from what it describes
            var group = GetOrAdd<CanvasGroup>(panel.gameObject);
            group.blocksRaycasts = false;
            group.interactable = false;

            var column = GetOrAdd<VerticalLayoutGroup>(panel.gameObject);
            int p = Mathf.RoundToInt(m);
            column.padding = new RectOffset(p, p, p, p);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.UpperLeft;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            var fitter = GetOrAdd<ContentSizeFitter>(panel.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            var title = FindDeep(root, "Title").GetComponent<TextMeshProUGUI>();
            title.transform.SetParent(panel, false);
            title.transform.SetSiblingIndex(0);
            InLayout(title.gameObject);
            Wrapping(title, s, UIFontSize.Big, bold: true);

            // Body: text column + list column
            var body = FindOrCreate(panel, "Body");
            body.SetSiblingIndex(1);
            var row = GetOrAdd<HorizontalLayoutGroup>(body.gameObject);
            row.padding = new RectOffset(0, 0, 0, 0);
            row.spacing = m;
            row.childAlignment = TextAnchor.UpperLeft;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var textColumn = FindOrCreate(body, "TextColumn");
            textColumn.SetSiblingIndex(0);
            var textLayout = GetOrAdd<VerticalLayoutGroup>(textColumn.gameObject);
            textLayout.padding = new RectOffset(0, 0, 0, 0);
            textLayout.spacing = s.Spacing;
            textLayout.childAlignment = TextAnchor.UpperLeft;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;
            var textElement = GetOrAdd<LayoutElement>(textColumn.gameObject);
            textElement.flexibleWidth = 1f;
            textElement.minWidth = 200f;

            var general = (FindDeep(root, "GeneralTooltipTxt") ?? FindDeep(root, "Text")).GetComponent<TextMeshProUGUI>();
            general.name = "GeneralTooltipTxt";
            general.transform.SetParent(textColumn, false);
            general.transform.SetSiblingIndex(0);
            InLayout(general.gameObject);
            Wrapping(general, s, UIFontSize.Small);

            var detailed = FindOrCreateText(textColumn, "DetailedTooltipTxt");
            detailed.transform.SetSiblingIndex(1);
            Wrapping(detailed, s, UIFontSize.Small);

            var list = (FindDeep(root, "ListColumn") ?? FindDeep(root, "StatSourcePanel"));
            list.name = "ListColumn";
            list.SetParent(body, false);
            list.SetSiblingIndex(1);
            var band = GetOrAdd<Image>(list.gameObject);
            band.enabled = true;
            band.sprite = s.SolidFill;
            band.type = Image.Type.Simple;
            band.color = ListBand;
            var listLayout = GetOrAdd<VerticalLayoutGroup>(list.gameObject);
            int lp = Mathf.RoundToInt(ListPadding);
            listLayout.padding = new RectOffset(lp, lp, lp, lp);
            listLayout.childAlignment = TextAnchor.UpperLeft;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            var listElement = GetOrAdd<LayoutElement>(list.gameObject);
            listElement.ignoreLayout = false;
            listElement.minWidth = ListWidth;
            listElement.preferredWidth = ListWidth;
            listElement.flexibleWidth = 0f;

            var listText = (FindDeep(root, "ListTxt") ?? FindDeep(root, "StatSourceTxt")).GetComponent<TextMeshProUGUI>();
            listText.name = "ListTxt";
            listText.transform.SetParent(list, false);
            InLayout(listText.gameObject);
            Wrapping(listText, s, UIFontSize.Small);

            DestroyChild(panel, "DescriptionPanel");

            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;

            var ui = new SerializedObject(root.GetComponent<GeneralTooltipPaneUI>());
            ui.FindProperty("mainPanel").objectReferenceValue = panel.gameObject;
            ui.FindProperty("titleText").objectReferenceValue = title;
            ui.FindProperty("descriptionText").objectReferenceValue = general;
            ui.FindProperty("detailText").objectReferenceValue = detailed;
            ui.FindProperty("listSection").objectReferenceValue = list.gameObject;
            ui.FindProperty("listText").objectReferenceValue = listText;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform FindDeep(GameObject root, string name) =>
            root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);

        // Moved texts used to be placed by hand; now the layout groups place them
        private static void InLayout(GameObject go)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = false;
        }

        private static void Wrapping(TextMeshProUGUI text, UISettings s, UIFontSize size, bool bold = false)
        {
            CenterWindowLayout.PlainText(text, TextAlignmentOptions.TopLeft);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            StyleText(text, size, false, UIColorRole.Text, s, bold);
        }
    }
}
