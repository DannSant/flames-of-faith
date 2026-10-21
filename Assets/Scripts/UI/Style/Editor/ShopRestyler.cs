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
    /// UI revamp, step 10: restyles the shop in place as a center window between the Inventory and the Stats pane:
    /// "Shop" (Big, bold, yellow), a 5-column grid of cells (the Inventory's slot plus the price under it), the
    /// "not enough" error above Continue, which sits in the progress spot. Item details use the shared tooltip, so
    /// the shop's own description panel goes, and so do the scroll view (20 items fit) and the "(A) Buy" prompt row.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.ShopRestyler.Restyle
    /// </summary>
    public static class ShopRestyler
    {
        public const string PanePath = "Assets/Prefabs/UI/Shop/ShopPane.prefab";
        public const string CellPath = "Assets/Prefabs/UI/Shop/ShopIconUI.prefab";
        public const string SlotPath = "Assets/Prefabs/UI/Inventory/EffectIconUI.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        public const int Columns = 5;
        public const int WindowPriority = 50;
        public const float PriceSpacing = 4f;

        /// <summary>A Small slot over a Tiny-high price row, wide enough for the coin and a 3-digit price.</summary>
        public static Vector2 CellSize(UISettings s) => new Vector2(66f, s.SmallIconSize + s.TinyIconSize + 2f);

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Shop")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[ShopRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(CellPath, root => RestyleCell(root, settings));
            UpgradeRestyler.EditPrefab(PanePath, root => RestylePane(root, settings));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) =>
            {
                // Revert first: a revert reloads the instance, which would drop the snapped root layout
                bool changed = RevertChildLayoutOverrides(scene, PanePath);
                return UpgradeRestyler.SnapSceneInstances(scene, PanePath) || changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[ShopRestyler] Shop restyled.");
        }

        // --- Cell: the Inventory slot over the price ----------------------------------------------------

        private static void RestyleCell(GameObject root, UISettings s)
        {
            Vector2 cell = CellSize(s);
            Place((RectTransform)root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, cell);

            // The old cell was its own icon and button; the slot brings both now
            var oldButton = root.GetComponent<Button>();
            if (oldButton != null) Object.DestroyImmediate(oldButton);
            var oldImage = root.GetComponent<Image>();
            if (oldImage != null) Object.DestroyImmediate(oldImage);

            var slotRect = Find(root.transform, "Slot");
            if (slotRect == null)
            {
                var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPath);
                var slotGo = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, root.transform);
                slotGo.name = "Slot";
                slotRect = (RectTransform)slotGo.transform;
            }
            slotRect.SetSiblingIndex(0);
            Place(slotRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.one * s.SmallIconSize);

            var price = FindOrCreate(root.transform, "Price");
            price.SetSiblingIndex(1);
            price.anchorMin = new Vector2(0f, 0f);
            price.anchorMax = new Vector2(1f, 0f);
            price.pivot = new Vector2(0.5f, 0f);
            price.anchoredPosition = Vector2.zero;
            price.sizeDelta = new Vector2(0f, s.TinyIconSize);
            price.localScale = Vector3.one;
            var row = GetOrAdd<HorizontalLayoutGroup>(price.gameObject);
            row.padding = new RectOffset(0, 0, 0, 0);
            row.spacing = PriceSpacing;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var coin = (Find(root.transform, "CoinsIcon") ?? Find(price, "CoinsIcon")).GetComponent<Image>();
            coin.transform.SetParent(price, false);
            coin.transform.SetSiblingIndex(0);
            coin.preserveAspect = true;
            coin.raycastTarget = false;
            var coinLayout = GetOrAdd<LayoutElement>(coin.gameObject);
            coinLayout.ignoreLayout = false;
            coinLayout.minWidth = coinLayout.preferredWidth = s.TinyIconSize;
            coinLayout.minHeight = coinLayout.preferredHeight = s.TinyIconSize;
            coinLayout.flexibleWidth = coinLayout.flexibleHeight = 0f;
            StyleIcon(coin.gameObject, UIIconSize.Tiny, s);

            var cost = (Find(root.transform, "CoinsText") ?? Find(price, "CoinsText")).GetComponent<TextMeshProUGUI>();
            cost.transform.SetParent(price, false);
            cost.transform.SetSiblingIndex(1);
            var costLayout = GetOrAdd<LayoutElement>(cost.gameObject);
            costLayout.ignoreLayout = false;
            costLayout.minWidth = costLayout.preferredWidth = -1f;
            costLayout.flexibleWidth = costLayout.flexibleHeight = 0f;
            CenterWindowLayout.PlainText(cost, TextAlignmentOptions.MidlineLeft);
            cost.textWrappingMode = TextWrappingModes.NoWrap;
            cost.raycastTarget = false;
            cost.text = "25";
            cost.color = s.TextColor;
            // Color role None: ShopIconUI colors the price (red when the player can't afford it)
            StyleText(cost, UIFontSize.Small, false, UIColorRole.None, s);

            var ui = new SerializedObject(root.GetComponent<ShopIconUI>());
            ui.FindProperty("slot").objectReferenceValue = slotRect.GetComponent<EffectIconUI>();
            ui.FindProperty("slotButton").objectReferenceValue = slotRect.GetComponent<Button>();
            ui.FindProperty("costText").objectReferenceValue = cost;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- Pane -----------------------------------------------------------------------------------------

        private static void RestylePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float buttonHeight = s.GetButtonHeight(UIButtonSize.Small);
            CenterWindowLayout.PlaceWindow((RectTransform)root.transform, s);

            var mainPanel = Find(root.transform, "MainPanel");
            CenterWindowLayout.FrameWindow(mainPanel, s);

            var window = mainPanel.GetComponent<UIWindow>();
            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("priority").intValue = WindowPriority;
            // The shop is modal (player input is blocked until Continue), so it takes focus on open
            windowSo.FindProperty("focusMode").enumValueIndex = (int)UIWindowFocusMode.AutoFocus;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            CenterWindowLayout.UseStandardMarker(window);

            var ui = new SerializedObject(root.GetComponent<ShopControllerUI>());

            var title = Find(mainPanel, "Title").GetComponent<TextMeshProUGUI>();
            title.text = "Shop";
            title.transform.SetSiblingIndex(0);
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            // Grid: the old scroll content, out of the scroll view (the shop never shows more than fits)
            var grid = Find(mainPanel, "Grid") ?? (RectTransform)((GameObject)ui.FindProperty("itemsContainer").objectReferenceValue).transform;
            grid.SetParent(mainPanel, false);
            grid.name = "Grid";
            grid.SetSiblingIndex(1);
            for (int i = grid.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(grid.GetChild(i).gameObject);
            }
            var fitter = grid.GetComponent<ContentSizeFitter>();
            if (fitter != null) Object.DestroyImmediate(fitter);
            var gridImage = grid.GetComponent<Image>();
            if (gridImage != null) Object.DestroyImmediate(gridImage);
            Stretch(grid, Vector2.zero, Vector2.one, m, CenterWindowLayout.ButtonsTop(s, 2) + m, m,
                m + InventoryRestyler.TitleRowHeight + s.Spacing);
            var layout = GetOrAdd<GridLayoutGroup>(grid.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.cellSize = CellSize(s);
            layout.spacing = new Vector2(s.Spacing, s.Spacing);
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;

            // Leftovers: the scroll view, the shop's own description panel (the shared tooltip replaces it),
            // and the gamepad-only "(A) Buy" prompt row
            DestroyChild(mainPanel, "ItemsScrollView");
            DestroyChild(mainPanel, "DescriptionPanel");
            DestroyChild(mainPanel, "InputPrompts");

            // "Not enough Golden Chronos": the row just above Continue
            var error = (TextMeshProUGUI)ui.FindProperty("errorTextUI").objectReferenceValue;
            var oldColor = error.GetComponent<UIStyleColor>();
            if (oldColor != null) Object.DestroyImmediate(oldColor);
            error.transform.SetSiblingIndex(2);
            Place(error.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, m + buttonHeight + s.Spacing),
                new Vector2(CenterWindowLayout.Width - 2f * m, buttonHeight));
            var errorLayout = error.GetComponent<LayoutElement>();
            if (errorLayout != null) errorLayout.ignoreLayout = true;
            CenterWindowLayout.PlainText(error, TextAlignmentOptions.Center);
            error.textWrappingMode = TextWrappingModes.NoWrap;
            error.raycastTarget = false;
            StyleText(error, UIFontSize.Small, false, UIColorRole.Negative, s);

            // Continue: the progress spot, wired in code (its persistent call named a stale type)
            var continueRect = Find(mainPanel, "ContinueButton");
            var label = continueRect.GetComponentInChildren<TextMeshProUGUI>(true);
            label.text = "Continue";
            StretchFull(label.rectTransform);
            CenterWindowLayout.PlaceStackedButton(continueRect, s, 0);
            continueRect.SetAsLastSibling();
            var continueButton = continueRect.GetComponent<Button>();
            while (continueButton.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(continueButton.onClick, 0);
            }

            // The leave / buy dialog draws over the window
            var dialog = root.GetComponentInChildren<ConfirmDialogUI>(true);
            if (dialog != null) dialog.transform.SetAsLastSibling();

            ui.FindProperty("itemsContainer").objectReferenceValue = grid.gameObject;
            ui.FindProperty("continueButton").objectReferenceValue = continueButton;
            ui.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
