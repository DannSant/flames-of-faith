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
    /// UI revamp, step 2: restyles the Inventory window (InventoryPane + its EffectIconUI slots) to the
    /// UI standard in place. The item description panel is gone: items use the scene's shared
    /// GeneralTooltipPane. Layout at the 1920x1080 reference: a framed window on the left, between the
    /// HUD's top-left stack and IconsPane; title row ("Inventory" + coins) above an 8-column grid.
    /// Run it again after changing the layout here. In UIMain it only reverts the overrides it replaces.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.InventoryRestyler.Restyle
    /// </summary>
    public static class InventoryRestyler
    {
        public const string InventoryPanePath = "Assets/Prefabs/UI/Inventory/InventoryPane.prefab";
        public const string EffectIconPath = "Assets/Prefabs/UI/Inventory/EffectIconUI.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        public const int Columns = 8;
        public const int Rows = 10;
        public const float TitleRowHeight = 28f;
        private const float SlotIconInset = 5f;
        private const float CoinsWidth = 160f;
        // Favors + coins on the title row, right-aligned (CurrencyRowBuilder)
        private const float WalletWidth = 300f;

        public static Vector2 PaneSize(UISettings s)
        {
            float cell = s.SmallIconSize;
            float width = s.Margin + UIStyleGrid.Span(Columns, cell, s.Spacing) + s.Margin;
            float height = s.Margin + TitleRowHeight + s.Margin + UIStyleGrid.Span(Rows, cell, s.Spacing) + s.Margin;
            return new Vector2(width, height);
        }

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Inventory")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[InventoryRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            EditPrefab(EffectIconPath, root => RestyleEffectIcon(root, settings));
            EditPrefab(InventoryPanePath, root => RestyleInventoryPane(root, settings));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) => CleanUIMainInstance(scene));

            AssetDatabase.SaveAssets();
            Debug.Log("[InventoryRestyler] Inventory restyled.");
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

        // --- EffectIconUI: one item slot ---------------------------------------------------------------

        private static void RestyleEffectIcon(GameObject root, UISettings s)
        {
            var rootRect = (RectTransform)root.transform;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(s.SmallIconSize, s.SmallIconSize);

            // The root image was the item icon; it becomes the slot frame (tinted by quality at runtime)
            var frame = root.GetComponent<Image>();
            bool iconCreated = Find(root.transform, "Icon") == null;
            var icon = FindOrCreateImage(root.transform, "Icon");
            if (iconCreated) icon.sprite = frame.sprite;
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            StretchFull(icon.rectTransform, SlotIconInset);
            icon.transform.SetSiblingIndex(0);

            frame.preserveAspect = false;
            frame.raycastTarget = true; // hover
            StyleFrame(frame, UIFrameKind.Slot, s);

            // The press/disabled tint goes on the item icon
            var button = root.GetComponent<Button>();
            if (button != null) button.targetGraphic = icon;

            // Stack count: a small badge in the bottom-right corner, inside the slot
            var quantity = Find(root.transform, "QtyText").GetComponent<TextMeshProUGUI>();
            Place(quantity.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-3f, 2f), new Vector2(40f, 16f));
            quantity.alignment = TextAlignmentOptions.BottomRight;
            quantity.characterSpacing = 0f;
            quantity.margin = Vector4.zero;
            quantity.raycastTarget = false;
            quantity.text = "2";
            StyleText(quantity, UIFontSize.Small, true, UIColorRole.Text, s);
            quantity.transform.SetAsLastSibling();

            var so = new SerializedObject(root.GetComponent<EffectIconUI>());
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("frame").objectReferenceValue = frame;
            so.FindProperty("quantitytext").objectReferenceValue = quantity;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- InventoryPane ------------------------------------------------------------------------------

        private static void RestyleInventoryPane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            Vector2 size = PaneSize(s);

            // Left side, vertically centered: between the HUD's top-left stack and IconsPane
            Place((RectTransform)root.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(m, m), size);
            StyleFrame(root.GetComponent<Image>(), UIFrameKind.Panel, s);

            // Title row: "Inventory" (Big, bold) on the left
            var title = Find(root.transform, "Title").GetComponent<TextMeshProUGUI>();
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m, -m), new Vector2(size.x * 0.5f, TitleRowHeight));
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.characterSpacing = 0f;
            title.margin = Vector4.zero;
            title.raycastTarget = false;
            StyleText(title, UIFontSize.Big, false, UIColorRole.Text, s, bold: true);

            // The window (UIWindow) covers the whole pane; the grid moves to its own child
            var mainPanel = Find(root.transform, "MainPanel");
            StretchFull(mainPanel);
            var oldGrid = mainPanel.GetComponent<GridLayoutGroup>();
            if (oldGrid != null) Object.DestroyImmediate(oldGrid);

            var window = new SerializedObject(mainPanel.GetComponent<UIWindow>());
            window.FindProperty("overrideMarkerSettings").boolValue = false;
            window.ApplyModifiedPropertiesWithoutUndo();

            // The description panel is replaced by the shared tooltip; the placeholder slot by real ones
            DestroyChild(mainPanel, "DescriptionPanel");
            foreach (var placeholder in mainPanel.GetComponentsInChildren<EffectIconUI>(true).ToList())
            {
                Object.DestroyImmediate(placeholder.gameObject);
            }

            // Item grid: 8 columns of Small slots under the title row
            var gridRect = FindOrCreate(mainPanel, "Grid");
            gridRect.SetSiblingIndex(0);
            Stretch(gridRect, Vector2.zero, Vector2.one, m, m, m, m + TitleRowHeight + m);
            var grid = GetOrAdd<GridLayoutGroup>(gridRect.gameObject);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            var gridStyle = GetOrAdd<UIStyleGrid>(gridRect.gameObject);
            gridStyle.Configure(UIIconSize.Small);
            gridStyle.Apply(s);

            // Favors and coins on the title row, right-aligned: [Favor icon] 42 (+5)  [coin icon 28] 9999
            var wallet = FindOrCreate(mainPanel, CurrencyRowBuilder.WalletName);
            Place(wallet, Vector2.one, Vector2.one, new Vector2(-m, -m), new Vector2(WalletWidth, TitleRowHeight));
            var coins = Find(wallet, "Coins") ?? FindOrCreate(mainPanel, "Coins");
            Place(coins, Vector2.one, Vector2.one, new Vector2(-m, -m), new Vector2(CoinsWidth, TitleRowHeight));
            var coinsLayout = GetOrAdd<HorizontalLayoutGroup>(coins.gameObject);
            coinsLayout.padding = new RectOffset(0, 0, 0, 0);
            coinsLayout.spacing = 6f;
            coinsLayout.childAlignment = TextAnchor.MiddleRight;
            coinsLayout.childControlWidth = true;
            coinsLayout.childControlHeight = false;
            coinsLayout.childForceExpandWidth = false;
            coinsLayout.childForceExpandHeight = false;

            var coinIcon = (Find(mainPanel, "CoinsIcon") ?? Find(coins, "CoinsIcon")).GetComponent<Image>();
            coinIcon.transform.SetParent(coins, false);
            coinIcon.transform.SetSiblingIndex(0);
            coinIcon.preserveAspect = true;
            coinIcon.raycastTarget = false;
            var iconLayout = GetOrAdd<LayoutElement>(coinIcon.gameObject);
            iconLayout.ignoreLayout = false;
            Place(coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * s.TinyIconSize);
            StyleIcon(coinIcon.gameObject, UIIconSize.Tiny, s);

            var coinsText = (Find(mainPanel, "CoinsText") ?? Find(coins, "CoinsText")).GetComponent<TextMeshProUGUI>();
            coinsText.transform.SetParent(coins, false);
            coinsText.transform.SetSiblingIndex(1);
            var textLayout = GetOrAdd<LayoutElement>(coinsText.gameObject);
            textLayout.ignoreLayout = false;
            textLayout.preferredWidth = -1f;
            textLayout.minWidth = -1f;
            Place(coinsText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60f, TitleRowHeight));
            coinsText.alignment = TextAlignmentOptions.MidlineRight;
            coinsText.characterSpacing = 0f;
            coinsText.margin = Vector4.zero;
            coinsText.textWrappingMode = TextWrappingModes.NoWrap;
            StyleText(coinsText, UIFontSize.Small, false, UIColorRole.Text, s);

            var favorsText = CurrencyRowBuilder.Build(wallet, coins, s);

            var controller = new SerializedObject(root.GetComponent<InventoryControllerUI>());
            controller.FindProperty("iconContainer").objectReferenceValue = gridRect;
            controller.FindProperty("favorsText").objectReferenceValue = favorsText;
            controller.FindProperty("coinsTooltip").objectReferenceValue = coins.GetComponent<TooltipTriggerUI>();
            controller.FindProperty("favorsTooltip").objectReferenceValue = favorsText.transform.parent.GetComponent<TooltipTriggerUI>();
            controller.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- UIMain -------------------------------------------------------------------------------------

        // Only what this restyle replaces: the root layout (the scene restated the old values; a revert
        // can't drop root layout overrides, so the prefab's new values are copied) and the leftovers of
        // the deleted placeholder slot. Anything else set on the instance is kept.
        private static bool CleanUIMainInstance(UnityEngine.SceneManagement.Scene scene)
        {
            GameObject instance = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;
                    if (AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(transform.gameObject)) != InventoryPanePath) continue;
                    instance = transform.gameObject;
                    break;
                }
                if (instance != null) break;
            }

            if (instance == null)
            {
                Debug.LogError($"[InventoryRestyler] No {InventoryPanePath} instance in {UIMainScene}");
                return false;
            }

            MatchPrefabRootLayout(instance);
            PrefabUtility.RemoveUnusedOverrides(new[] { instance }, InteractionMode.AutomatedAction);
            return true;
        }
    }
}
