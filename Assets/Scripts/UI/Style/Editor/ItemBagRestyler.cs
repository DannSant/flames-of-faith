using Game.UI.Items;
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
    /// UI revamp, step 5: restyles the end-of-wave item bag (ItemBagPane) in place as a center window.
    /// "Items found" (Big, bold) + "Take or sell each item" (Small, muted), a 6-column grid of the Inventory's
    /// item slots (EffectIconUI, with a yellow outline on the selected one), and Small Take / Sell buttons
    /// (Take in the shared progress spot). The built-in description panel is gone: the shared tooltip shows
    /// the selected item.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.ItemBagRestyler.Restyle
    /// </summary>
    public static class ItemBagRestyler
    {
        public const string PanePath = "Assets/Prefabs/UI/ItemBag/ItemBagPane.prefab";
        private const string UIMainScene = "Assets/Scenes/UIMain.unity";

        public const int Columns = 6;
        private const float SubtitleHeight = 18f;
        private const float OutlineOutset = 2f;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Item Bag")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[ItemBagRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(InventoryRestyler.EffectIconPath, root => AddSelectedMark(root, settings));
            UpgradeRestyler.EditPrefab(PanePath, root => RestylePane(root, settings));
            UIRevampTools.RunOnScenes(new[] { UIMainScene }, (scene, path) => WireScene(scene));

            AssetDatabase.SaveAssets();
            Debug.Log("[ItemBagRestyler] Item bag restyled.");
        }

        // The Inventory slot gets a yellow ring, shown only on the item bag's selected item
        private static void AddSelectedMark(GameObject slot, UISettings s)
        {
            var mark = FindOrCreateImage(slot.transform, "SelectedMark");
            StretchFull(mark.rectTransform, -OutlineOutset);
            mark.sprite = s.SelectedOutline;
            mark.type = Image.Type.Sliced;
            mark.raycastTarget = false;
            var color = GetOrAdd<UIStyleColor>(mark.gameObject);
            color.Configure(UIColorRole.Highlight);
            color.Apply(s);
            mark.transform.SetAsLastSibling();
            mark.gameObject.SetActive(false);

            var so = new SerializedObject(slot.GetComponent<EffectIconUI>());
            so.FindProperty("selectedMark").objectReferenceValue = mark.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RestylePane(GameObject root, UISettings s)
        {
            float m = s.Margin;
            CenterWindowLayout.PlaceWindow((RectTransform)root.transform, s);

            var mainPanel = Find(root.transform, "MainPanel");
            CenterWindowLayout.FrameWindow(mainPanel, s);
            CenterWindowLayout.UseStandardMarker(mainPanel.GetComponent<UIWindow>());

            // Header: title + one-line subtitle
            var title = Find(mainPanel, "TitleText").GetComponent<TextMeshProUGUI>();
            title.text = "Items found";
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Text);

            float subtitleTop = m + InventoryRestyler.TitleRowHeight + 2f;
            var subtitle = FindOrCreateText(mainPanel, "SubtitleText");
            subtitle.transform.SetSiblingIndex(title.transform.GetSiblingIndex() + 1);
            UpgradeRestyler.PlaceTopRow(subtitle.rectTransform, s, subtitleTop, SubtitleHeight);
            subtitle.text = "Take or sell each item";
            CenterWindowLayout.PlainText(subtitle, TextAlignmentOptions.Center);
            StyleText(subtitle, UIFontSize.Small, false, UIColorRole.Muted, s);

            // Buttons first (out of the old description panel), then the old containers can go
            var take = (Find(mainPanel, "TakeButton") ?? mainPanel.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "TakeButton"));
            var sell = (Find(mainPanel, "SellButton") ?? mainPanel.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "SellButton"));
            foreach (var button in new[] { take, sell })
            {
                button.SetParent(mainPanel, false);
                // A stray, disabled UpgradeButtonUI was left on each copied button
                foreach (var stray in button.GetComponents<UpgradeButtonUI>()) Object.DestroyImmediate(stray);
            }
            take.GetComponentInChildren<TextMeshProUGUI>(true).text = "Take";
            sell.GetComponentInChildren<TextMeshProUGUI>(true).text = "Sell";
            CenterWindowLayout.PlaceStackedButton(take, s, 0);
            CenterWindowLayout.PlaceStackedButton(sell, s, 1);

            // Item grid: the old icons panel, moved up under the header
            var grid = Find(mainPanel, "Grid") ?? mainPanel.GetComponentsInChildren<RectTransform>(true).First(t => t.name == "IconsPanel");
            grid.name = "Grid";
            grid.SetParent(mainPanel, false);
            grid.SetSiblingIndex(subtitle.transform.GetSiblingIndex() + 1);
            float gridTop = subtitleTop + SubtitleHeight + m;
            Stretch(grid, Vector2.zero, Vector2.one, m, CenterWindowLayout.ButtonsTop(s, 2) + m, m, gridTop);
            var gridImage = grid.GetComponent<Image>();
            if (gridImage != null) Object.DestroyImmediate(gridImage);
            foreach (var placeholder in grid.Cast<Transform>().ToList()) Object.DestroyImmediate(placeholder.gameObject);

            var layout = GetOrAdd<GridLayoutGroup>(grid.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = Columns;
            var gridStyle = GetOrAdd<UIStyleGrid>(grid.gameObject);
            gridStyle.Configure(UIIconSize.Small);
            gridStyle.Apply(s);

            DestroyChild(mainPanel, "ItemsPanel");
            DestroyChild(mainPanel, "ContinueButton");
            take.SetAsLastSibling();
            sell.SetSiblingIndex(take.GetSiblingIndex());

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventoryRestyler.EffectIconPath).GetComponent<EffectIconUI>();
            var ui = new SerializedObject(root.GetComponent<ItemBagControllerUI>());
            ui.FindProperty("itemIconsContainer").objectReferenceValue = grid;
            ui.FindProperty("itemIconPrefab").objectReferenceValue = slotPrefab;
            ui.FindProperty("itemSellText").objectReferenceValue = sell.GetComponentInChildren<TextMeshProUGUI>(true);
            ui.FindProperty("sellFormat").stringValue = "Sell ({0})";
            ui.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool WireScene(UnityEngine.SceneManagement.Scene scene)
        {
            bool snapped = UpgradeRestyler.SnapSceneInstances(scene, PanePath);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
            var bag = all.Select(t => t.GetComponent<ItemBagControllerUI>()).FirstOrDefault(c => c != null);
            var inventory = all.Select(t => t.GetComponent<InventoryControllerUI>()).FirstOrDefault(c => c != null);
            if (bag == null || inventory == null) return snapped;

            var so = new SerializedObject(bag);
            so.FindProperty("inventoryUI").objectReferenceValue = inventory;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
