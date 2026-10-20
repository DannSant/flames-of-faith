using Game.UI.Navigation;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 3: restyles the level-up window (SelectUpgradePane + UpgradeButton) in place as a
    /// center window: "Select an upgrade" (Big, bold), "You are now Level N" (Small, yellow), coins gained
    /// (Small, green, Tiny coin), a column of Small upgrade buttons, Continue in the progress spot.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.UpgradeRestyler.Restyle
    /// </summary>
    public static class UpgradeRestyler
    {
        public const string PanePath = "Assets/Prefabs/UI/Progression/SelectUpgradePane.prefab";
        public const string ButtonPath = "Assets/Prefabs/UI/Progression/UpgradeButton.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        private const float LevelLineHeight = 18f;
        private const float CoinsRowHeight = 28f;
        private const float StarInset = 4f;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Level-up Window")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[UpgradeRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            EditPrefab(ButtonPath, root => RestyleButton(root, settings));
            EditPrefab(PanePath, root => RestylePane(root, settings));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) => SnapSceneInstances(scene, PanePath));

            AssetDatabase.SaveAssets();
            Debug.Log("[UpgradeRestyler] Level-up window restyled.");
        }

        public static void EditPrefab(string path, System.Action<GameObject> edit)
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

        /// <summary>Gives the scene instances of these prefabs their prefab's root layout; drops overrides on deleted objects.</summary>
        public static bool SnapSceneInstances(UnityEngine.SceneManagement.Scene scene, params string[] prefabPaths)
        {
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;
                    string source = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject));
                    if (!prefabPaths.Contains(source)) continue;
                    MatchPrefabRootLayout(transform.gameObject);
                    PrefabUtility.RemoveUnusedOverrides(new[] { transform.gameObject }, InteractionMode.AutomatedAction);
                    changed = true;
                }
            }
            if (!changed) Debug.LogError($"[UpgradeRestyler] No instance of {string.Join(", ", prefabPaths)} in {scene.path}");
            return changed;
        }

        // --- UpgradeButton: one Small, full-width choice ----------------------------------------------

        private static void RestyleButton(GameObject root, UISettings s)
        {
            float height = s.GetButtonHeight(UIButtonSize.Small);
            var rect = (RectTransform)root.transform;
            Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CenterWindowLayout.Width - 2f * s.Margin, height));

            // The layout column sets the width; the button keeps the Small height
            var layout = GetOrAdd<LayoutElement>(root);
            layout.ignoreLayout = false;
            layout.flexibleWidth = 1f;

            var label = root.GetComponentsInChildren<TextMeshProUGUI>(true).First();
            float star = s.TinyIconSize;
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, StarInset + star + 4f, 0f, StarInset + star + 4f, 0f);
            CenterWindowLayout.PlainText(label, TextAlignmentOptions.Center);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            StyleText(label, UIFontSize.Small, false, UIColorRole.Text, s);

            var roleStar = Find(root.transform, "RoleStar");
            Place(roleStar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(StarInset, 0f), new Vector2(star, star));
            StyleIcon(roleStar.gameObject, UIIconSize.Tiny, s);
            roleStar.SetAsLastSibling();

            var style = GetOrAdd<UIStyleButton>(root);
            style.Configure(UIButtonSize.Small);
            style.Apply(s);
        }

        // --- SelectUpgradePane --------------------------------------------------------------------------

        private static void RestylePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            var rootRect = (RectTransform)root.transform;
            CenterWindowLayout.PlaceWindow(rootRect, s);

            var mainPanel = Find(root.transform, "MainPanel");
            CenterWindowLayout.FrameWindow(mainPanel, s);
            var oldColumn = mainPanel.GetComponent<VerticalLayoutGroup>();
            if (oldColumn != null) Object.DestroyImmediate(oldColumn);
            CenterWindowLayout.UseStandardMarker(mainPanel.GetComponent<UIWindow>());

            // Leftovers: the stray upgrade script on Continue, and the placeholder choices
            var continueRect = Find(mainPanel, "ContinueButton");
            var stray = continueRect.GetComponent<UpgradeButtonUI>();
            if (stray != null) Object.DestroyImmediate(stray);
            foreach (var placeholder in mainPanel.GetComponentsInChildren<UpgradeButtonUI>(true).ToList())
            {
                Object.DestroyImmediate(placeholder.gameObject);
            }

            // Header: title, level line
            var title = Find(mainPanel, "SelectUpgradeText").GetComponent<TextMeshProUGUI>();
            title.text = "Select an upgrade";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Text);

            float levelTop = m + InventoryRestyler.TitleRowHeight + 2f;
            var level = FindOrCreateText(mainPanel, "LevelText");
            level.transform.SetSiblingIndex(title.transform.GetSiblingIndex() + 1);
            PlaceTopRow(level.rectTransform, s, levelTop, LevelLineHeight);
            level.text = "You are now Level 2";
            CenterWindowLayout.PlainText(level, TextAlignmentOptions.Center);
            StyleText(level, UIFontSize.Small, false, UIColorRole.Highlight, s);

            // Coins gained: [Tiny coin] Coins gained: 18, centered under the level line
            float coinsTop = levelTop + LevelLineHeight + 4f;
            var coins = Find(root.transform, "CoinsGained") ?? Find(mainPanel, "CoinsGained");
            coins.SetParent(mainPanel, false);
            coins.SetSiblingIndex(level.transform.GetSiblingIndex() + 1);
            PlaceTopRow(coins, s, coinsTop, CoinsRowHeight);
            var coinsLayout = GetOrAdd<HorizontalLayoutGroup>(coins.gameObject);
            coinsLayout.padding = new RectOffset(0, 0, 0, 0);
            coinsLayout.spacing = 6f;
            coinsLayout.childAlignment = TextAnchor.MiddleCenter;
            coinsLayout.childControlWidth = true;
            coinsLayout.childControlHeight = false;
            coinsLayout.childForceExpandWidth = false;
            coinsLayout.childForceExpandHeight = false;

            var coinIcon = Find(coins, "CoinsIcon").GetComponent<Image>();
            coinIcon.transform.SetSiblingIndex(0);
            coinIcon.preserveAspect = true;
            coinIcon.raycastTarget = false;
            GetOrAdd<LayoutElement>(coinIcon.gameObject).ignoreLayout = false;
            Place(coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * s.TinyIconSize);
            StyleIcon(coinIcon.gameObject, UIIconSize.Tiny, s);

            var coinsText = Find(coins, "CoinsText").GetComponent<TextMeshProUGUI>();
            coinsText.transform.SetSiblingIndex(1);
            var textLayout = GetOrAdd<LayoutElement>(coinsText.gameObject);
            textLayout.ignoreLayout = false;
            textLayout.preferredWidth = -1f;
            textLayout.minWidth = -1f;
            Place(coinsText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, CoinsRowHeight));
            coinsText.text = "Coins gained: 18";
            CenterWindowLayout.PlainText(coinsText, TextAlignmentOptions.MidlineLeft);
            coinsText.textWrappingMode = TextWrappingModes.NoWrap;
            StyleText(coinsText, UIFontSize.Small, false, UIColorRole.Positive, s);

            // Choices: a column of Small buttons under the header
            float optionsTop = coinsTop + CoinsRowHeight + m;
            var options = FindOrCreate(mainPanel, "Options");
            options.SetSiblingIndex(coins.GetSiblingIndex() + 1);
            Stretch(options, Vector2.zero, Vector2.one, m, CenterWindowLayout.ButtonsTop(s, 1) + m, m, optionsTop);
            var column = GetOrAdd<VerticalLayoutGroup>(options.gameObject);
            column.padding = new RectOffset(0, 0, 0, 0);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;

            // Continue: the progress spot shared by every center window
            CenterWindowLayout.PlaceStackedButton(continueRect, s, 0);
            continueRect.SetAsLastSibling();

            var ui = new SerializedObject(root.GetComponent<UpgradeSelectionUI>());
            ui.FindProperty("levelText").objectReferenceValue = level;
            ui.FindProperty("optionsContainer").objectReferenceValue = options;
            ui.FindProperty("levelFormat").stringValue = "You are now Level {0}";
            ui.FindProperty("coinsGainedFormat").stringValue = "Coins gained: {0}";
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        // A row stretched across the window's content width, at a distance from the window top
        private static void PlaceTopRow(RectTransform row, UISettings s, float top, float height)
        {
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -top);
            row.sizeDelta = new Vector2(-2f * s.Margin, height);
            row.localScale = Vector3.one;
            row.localRotation = Quaternion.identity;
        }
    }
}
