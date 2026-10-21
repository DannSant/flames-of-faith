using Game.UI.Navigation;
using Game.UI.RunEncounters;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 8: restyles the run encounter windows in place as center windows with a yellow title.
    /// Campfire: one rested line and Continue. Results: the result lines and Continue. Event: the text above a
    /// bottom column of full-width option buttons that grow with their label, plus an art panel to the left of
    /// the window (shown only for events with art). The treasure no longer has a window (its item goes to the
    /// item bag), so its UIMain instance is removed.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.RunEncounterRestyler.Restyle
    /// </summary>
    public static class RunEncounterRestyler
    {
        public const string CampfirePath = "Assets/Prefabs/UI/RunEncounters/CampfirePane.prefab";
        public const string ResultsPath = "Assets/Prefabs/UI/RunEncounters/ResultsPane.prefab";
        public const string EventPath = "Assets/Prefabs/UI/RunEncounters/EventEncounterPane.prefab";
        public const string OptionPath = "Assets/Prefabs/UI/RunEncounters/EventOptionButton.prefab";
        public const string TreasurePath = "Assets/Prefabs/UI/RunEncounters/TreasurePane.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        public const int WindowPriority = 50;
        public static readonly RectOffset OptionPadding = new RectOffset(10, 10, 6, 6);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Run Encounter Windows")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[RunEncounterRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(CampfirePath, root => RestyleCampfire(root, settings));
            UpgradeRestyler.EditPrefab(ResultsPath, root => RestyleResults(root, settings));
            UpgradeRestyler.EditPrefab(OptionPath, root => RestyleOptionButton(root, settings));
            UpgradeRestyler.EditPrefab(EventPath, root => RestyleEvent(root, settings));

            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) =>
            {
                bool changed = RemovePrefabInstances(scene, TreasurePath);
                return UpgradeRestyler.SnapSceneInstances(scene, CampfirePath, ResultsPath, EventPath) || changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[RunEncounterRestyler] Run encounter windows restyled.");
        }

        // --- Shared ------------------------------------------------------------------------------------

        /// <summary>Window, frame, standard marker and priority; returns MainPanel.</summary>
        private static RectTransform PlaceCenterWindow(GameObject root, UISettings s)
        {
            CenterWindowLayout.PlaceWindow((RectTransform)root.transform, s);
            var mainPanel = Find(root.transform, "MainPanel");
            CenterWindowLayout.FrameWindow(mainPanel, s);

            var window = mainPanel.GetComponent<UIWindow>();
            var so = new SerializedObject(window);
            so.FindProperty("priority").intValue = WindowPriority;
            so.ApplyModifiedPropertiesWithoutUndo();
            CenterWindowLayout.UseStandardMarker(window);
            return mainPanel;
        }

        private static float ContentTop(UISettings s) => s.Margin + InventoryRestyler.TitleRowHeight + s.Spacing;

        /// <summary>A Small text in the space between the title row and the progress button.</summary>
        private static void PlaceBodyText(TextMeshProUGUI text, UISettings s, TextAlignmentOptions alignment)
        {
            float top = ContentTop(s);
            float height = CenterWindowLayout.WindowSize(s).y - top - CenterWindowLayout.ButtonsTop(s, 1) - s.Margin;
            UpgradeRestyler.PlaceTopRow(text.rectTransform, s, top, height);
            var element = text.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = true;
            CenterWindowLayout.PlainText(text, alignment);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = true;
            text.raycastTarget = false;
            StyleText(text, UIFontSize.Small, false, UIColorRole.Text, s);
        }

        /// <summary>Continue in the progress spot; wired in code, so its persistent calls are cleared.</summary>
        private static Button PlaceContinue(RectTransform button, UISettings s)
        {
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            label.text = "Continue";
            StretchFull(label.rectTransform);
            CenterWindowLayout.PlaceStackedButton(button, s, 0);
            button.SetAsLastSibling();

            var component = button.GetComponent<Button>();
            while (component.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(component.onClick, 0);
            }
            return component;
        }

        private static RectTransform FindAnywhere(Transform root, string name)
        {
            foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name == name) return rect;
            }
            return null;
        }

        // --- Campfire ----------------------------------------------------------------------------------

        private static void RestyleCampfire(GameObject root, UISettings s)
        {
            var mainPanel = PlaceCenterWindow(root, s);
            var ui = new SerializedObject(root.GetComponent<CampfireUI>());

            var title = Find(mainPanel, "Title").GetComponent<TextMeshProUGUI>();
            title.text = "Campfire";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            // One text: the rested line and what it gave (set by CampfireUI)
            var info = (TextMeshProUGUI)ui.FindProperty("infoTxt").objectReferenceValue;
            info.transform.SetParent(mainPanel, false);
            info.transform.SetSiblingIndex(title.transform.GetSiblingIndex() + 1);
            info.name = "InfoText";
            info.text = "You feel rested. You gained 5 Grace and 10 health.";
            PlaceBodyText(info, s, TextAlignmentOptions.Top);

            // Leftovers: the separate rested text and the gem icon, in the old description panel
            DestroyChild(mainPanel, "DescriptionPanel");

            var continueButton = PlaceContinue(Find(mainPanel, "ContinueButton"), s);
            ui.FindProperty("continueButton").objectReferenceValue = continueButton;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- Results -----------------------------------------------------------------------------------

        private static void RestyleResults(GameObject root, UISettings s)
        {
            var mainPanel = PlaceCenterWindow(root, s);
            var grid = mainPanel.GetComponent<GridLayoutGroup>();
            if (grid != null) Object.DestroyImmediate(grid);

            // Out of the old description panel, then drop it
            var title = FindAnywhere(mainPanel, "Title").GetComponent<TextMeshProUGUI>();
            var text = FindAnywhere(mainPanel, "Text").GetComponent<TextMeshProUGUI>();
            var continueRect = FindAnywhere(mainPanel, "ContinueButton");
            foreach (var child in new Transform[] { title.transform, text.transform, continueRect })
            {
                child.SetParent(mainPanel, false);
            }
            DestroyChild(mainPanel, "DescriptionPanel");

            title.text = "Results";
            title.transform.SetSiblingIndex(0);
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            text.transform.SetSiblingIndex(1);
            text.text = "Gained 10 health\nLost 5 Grace";
            PlaceBodyText(text, s, TextAlignmentOptions.Top);

            PlaceContinue(continueRect, s);
        }

        // --- Event -------------------------------------------------------------------------------------

        private static void RestyleEvent(GameObject root, UISettings s)
        {
            float m = s.Margin;
            var mainPanel = PlaceCenterWindow(root, s);
            var ui = new SerializedObject(root.GetComponent<EventEncounterUI>());

            // The frame is the background now; events show their own art in the side panel
            DestroyChild(mainPanel, "EventBackground");

            var title = (TextMeshProUGUI)ui.FindProperty("titleText").objectReferenceValue;
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            // Body: the text takes whatever room the options leave; the options sit at the bottom
            var body = FindOrCreate(mainPanel, "Body");
            body.SetSiblingIndex(title.transform.GetSiblingIndex() + 1);
            Stretch(body, Vector2.zero, Vector2.one, m, m, m, ContentTop(s));
            var bodyLayout = GetOrAdd<VerticalLayoutGroup>(body.gameObject);
            bodyLayout.padding = new RectOffset(0, 0, 0, 0);
            bodyLayout.spacing = m;
            bodyLayout.childAlignment = TextAnchor.UpperCenter;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            var description = (TextMeshProUGUI)ui.FindProperty("descriptionText").objectReferenceValue;
            description.transform.SetParent(body, false);
            description.transform.SetSiblingIndex(0);
            var descriptionLayout = GetOrAdd<LayoutElement>(description.gameObject);
            descriptionLayout.ignoreLayout = false;
            descriptionLayout.minHeight = -1f;
            descriptionLayout.preferredHeight = -1f;
            descriptionLayout.flexibleHeight = 1f;
            CenterWindowLayout.PlainText(description, TextAlignmentOptions.TopLeft);
            description.textWrappingMode = TextWrappingModes.Normal;
            description.overflowMode = TextOverflowModes.Overflow;
            description.richText = true;
            description.raycastTarget = false;
            StyleText(description, UIFontSize.Small, false, UIColorRole.Text, s);

            var options = (RectTransform)ui.FindProperty("optionsContainer").objectReferenceValue;
            options.SetParent(body, false);
            options.SetSiblingIndex(1);
            var row = options.GetComponent<HorizontalLayoutGroup>();
            if (row != null) Object.DestroyImmediate(row);
            var column = GetOrAdd<VerticalLayoutGroup>(options.gameObject);
            column.padding = new RectOffset(0, 0, 0, 0);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.LowerCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            for (int i = options.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(options.GetChild(i).gameObject);
            }

            // Art panel: left of the window, the window's size; shown by EventEncounterUI for events with art
            var artPanel = FindOrCreateImage(mainPanel, "ArtPanel");
            artPanel.transform.SetAsFirstSibling();
            Place(artPanel.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-m, 0f), CenterWindowLayout.WindowSize(s));
            artPanel.raycastTarget = false;
            StyleFrame(artPanel, UIFrameKind.Panel, s);
            var art = FindOrCreateImage(artPanel.transform, "Art");
            StretchFull(art.rectTransform, m);
            art.preserveAspect = true;
            art.raycastTarget = false;
            art.color = Color.white;
            artPanel.gameObject.SetActive(false);

            ui.FindProperty("artPanel").objectReferenceValue = artPanel.gameObject;
            ui.FindProperty("artImage").objectReferenceValue = art;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- EventOptionButton: a full-width Small button that grows with its label --------------------

        private static void RestyleOptionButton(GameObject root, UISettings s)
        {
            float height = s.GetButtonHeight(UIButtonSize.Small);
            var rect = (RectTransform)root.transform;
            Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CenterWindowLayout.Width - 2f * s.Margin, height));

            // No LayoutElement on the root: UIStyleButton would pin its preferred height to the Small height
            var rootElement = root.GetComponent<LayoutElement>();
            if (rootElement != null) Object.DestroyImmediate(rootElement);
            var layout = GetOrAdd<VerticalLayoutGroup>(root);
            layout.padding = new RectOffset(OptionPadding.left, OptionPadding.right, OptionPadding.top, OptionPadding.bottom);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var button = root.GetComponent<Button>();
            var graphic = (RectTransform)button.targetGraphic.transform;
            GetOrAdd<LayoutElement>(graphic.gameObject).ignoreLayout = true;
            StretchFull(graphic);

            CenterWindowLayout.StyleSmallButton(rect, s);

            // The label sets the height; never shorter than a Small button
            var label = root.GetComponentInChildren<TextMeshProUGUI>(true);
            var labelLayout = GetOrAdd<LayoutElement>(label.gameObject);
            labelLayout.ignoreLayout = false;
            labelLayout.minHeight = height - OptionPadding.vertical;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
        }
    }
}
