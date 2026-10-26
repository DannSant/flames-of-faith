using System.Linq;
using Game.UI.InputPrompts;
using Game.UI.Navigation;
using Game.UI.Overworld;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// UI revamp, step 12: restyles the overworld map's HUD (LevelSelector scene) in place.
    /// The map window (LevelSelectorUI, with the former CurrencyPane merged in) sits top right, as wide as the Stats
    /// pane under it: the act title (Big, bold, yellow), the level under the player (Small) and the coins (Tiny coin,
    /// Small text), and Save &amp; Exit (Small) with its (Y) prompt. The gamepad prompts (InputPrompts) are a framed
    /// window bottom left: the map rows while moving, the menu rows (LB/RB, B) while in the UI.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.LevelSelectorRestyler.Restyle
    /// </summary>
    public static class LevelSelectorRestyler
    {
        public const string WindowPath = "Assets/Prefabs/UI/LevelSelector/LevelSelectorUI.prefab";
        public const string PromptsPath = "Assets/Prefabs/UI/LevelSelector/InputPrompts.prefab";
        public const string CurrencyPanePath = "Assets/Prefabs/UI/LevelSelector/CurrencyPane.prefab";
        private const string LevelSelectorScene = "Assets/Scenes/LevelSelector.unity";
        private const string CoinSprite = "Assets/AssetPackages/ClassicPixelRPGUI/Artwok/Icons/32_coin.png";
        private const string XboxIcons = "Assets/Sprites/UI/XboxIcons/";

        /// <summary>The map window is as wide as the Stats pane under it.</summary>
        public const float WindowWidth = 486f;
        public const float CoinsWidth = 150f;
        // Favors + coins on row 2, right-aligned (CurrencyRowBuilder)
        public const float WalletWidth = 260f;

        public static float WindowHeight(UISettings s) =>
            s.Margin + InventoryRestyler.TitleRowHeight + s.Spacing + s.TinyIconSize + s.Spacing + s.GetButtonHeight(UIButtonSize.Small) + s.Margin;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Map HUD")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[LevelSelectorRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            UpgradeRestyler.EditPrefab(WindowPath, root => RestyleWindow(root, settings));
            UpgradeRestyler.EditPrefab(PromptsPath, root => RestylePrompts(root, settings));

            UIRevampTools.RunOnScenes(new[] { LevelSelectorScene }, (scene, path) =>
            {
                // The coins are part of the map window now
                bool changed = RemovePrefabInstances(scene, CurrencyPanePath);
                // Revert first: a revert reloads the instance, which would drop the snapped root layout
                changed |= RevertChildLayoutOverrides(scene, WindowPath);
                changed |= RevertChildLayoutOverrides(scene, PromptsPath);
                changed |= UpgradeRestyler.SnapSceneInstances(scene, WindowPath, PromptsPath);
                changed |= WirePresenter(scene);
                changed |= FlattenCanvasDepth(scene);
                return changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[LevelSelectorRestyler] Map HUD restyled.");
        }

        // --- The map window ----------------------------------------------------------------------------

        private static void RestyleWindow(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float buttonHeight = s.GetButtonHeight(UIButtonSize.Small);
            float rowTop = m + InventoryRestyler.TitleRowHeight + s.Spacing;
            float rowHeight = s.TinyIconSize;

            Place((RectTransform)root.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-m, -m), new Vector2(WindowWidth, WindowHeight(s)));
            var frame = GetOrAdd<Image>(root);
            frame.enabled = true;
            frame.raycastTarget = true;
            StyleFrame(frame, UIFrameKind.Panel, s);
            CenterWindowLayout.UseStandardMarker(root.GetComponent<UIWindow>());

            var title = Find(root.transform, "ActTitle").GetComponent<TextMeshProUGUI>();
            title.text = "Act I - Jesterwoods";
            title.transform.SetSiblingIndex(0);
            CenterWindowLayout.PlaceTitle(title, s, UIColorRole.Highlight);

            // Row 2: the level under the player on the left, the Favors and coins on the right
            var level = (Find(root.transform, "LevelName") ?? FindDeep(root.transform, "LevelNameTxt")).GetComponent<TextMeshProUGUI>();
            level.transform.SetParent(root.transform, false);
            level.name = "LevelName";
            level.transform.SetSiblingIndex(1);
            DestroyChild(root.transform, "LevelNamePane");
            Place(level.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(m, -rowTop),
                new Vector2(WindowWidth - 2f * m - WalletWidth - s.Spacing, rowHeight));
            CenterWindowLayout.PlainText(level, TextAlignmentOptions.MidlineLeft);
            level.textWrappingMode = TextWrappingModes.NoWrap;
            level.overflowMode = TextOverflowModes.Ellipsis;
            level.raycastTarget = false;
            level.text = "Jesterwood glade";
            StyleText(level, UIFontSize.Small, false, UIColorRole.Text, s);

            var wallet = FindOrCreate(root.transform, CurrencyRowBuilder.WalletName);
            wallet.SetSiblingIndex(2);
            Place(wallet, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-m, -rowTop), new Vector2(WalletWidth, rowHeight));
            var coins = Find(wallet, "Coins") ?? FindOrCreate(root.transform, "Coins");
            Place(coins, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-m, -rowTop), new Vector2(CoinsWidth, rowHeight));
            var row = GetOrAdd<HorizontalLayoutGroup>(coins.gameObject);
            row.padding = new RectOffset(0, 0, 0, 0);
            row.spacing = 4f;
            row.childAlignment = TextAnchor.MiddleRight;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var coin = FindOrCreateImage(coins, "CoinsIcon");
            coin.transform.SetSiblingIndex(0);
            coin.sprite = LoadSprite(CoinSprite);
            coin.preserveAspect = true;
            coin.raycastTarget = false;
            FixedLayout(coin.gameObject, s.TinyIconSize, s.TinyIconSize);
            StyleIcon(coin.gameObject, UIIconSize.Tiny, s);

            var coinsText = FindOrCreateText(coins, "CoinsText");
            coinsText.transform.SetSiblingIndex(1);
            CenterWindowLayout.PlainText(coinsText, TextAlignmentOptions.MidlineRight);
            coinsText.textWrappingMode = TextWrappingModes.NoWrap;
            coinsText.text = "1250";
            StyleText(coinsText, UIFontSize.Small, false, UIColorRole.Text, s);

            var favorsText = CurrencyRowBuilder.Build(wallet, coins, s);

            // Save & Exit (Small) bottom right, its (Y) prompt just left of it
            var exit = Find(root.transform, "ExitButton");
            var exitLabel = exit.GetComponentInChildren<TextMeshProUGUI>(true);
            exitLabel.text = "Save & Exit";
            StretchFull(exitLabel.rectTransform);
            Place(exit, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-m, m), new Vector2(CenterWindowLayout.ProgressButtonWidth, buttonHeight));
            CenterWindowLayout.StyleSmallButton(exit, s);
            exit.SetAsLastSibling();

            var prompt = Find(root.transform, "InputPromptsSaveAndExit");
            Place(prompt, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-m - CenterWindowLayout.ProgressButtonWidth - s.Spacing, m + (buttonHeight - s.TinyIconSize) * 0.5f),
                Vector2.one * s.TinyIconSize);
            foreach (Transform child in prompt)
            {
                var image = child.GetComponent<Image>();
                if (image == null) continue;
                Place((RectTransform)child, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * s.TinyIconSize);
                image.preserveAspect = true;
                image.raycastTarget = false;
                StyleIcon(child.gameObject, UIIconSize.Tiny, s);
            }

            var info = new SerializedObject(GetOrAdd<OverworldMapInfoUI>(root));
            info.FindProperty("actTitleText").objectReferenceValue = title;
            info.FindProperty("levelNameText").objectReferenceValue = level;
            info.FindProperty("coinsText").objectReferenceValue = coinsText;
            info.FindProperty("favorsText").objectReferenceValue = favorsText;
            info.FindProperty("coinsTooltip").objectReferenceValue = coins.GetComponent<TooltipTriggerUI>();
            info.FindProperty("favorsTooltip").objectReferenceValue = favorsText.transform.parent.GetComponent<TooltipTriggerUI>();
            info.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- The gamepad prompts -------------------------------------------------------------------------

        private static void RestylePrompts(GameObject root, UISettings s)
        {
            float m = s.Margin;
            var rect = (RectTransform)root.transform;
            Place(rect, Vector2.zero, Vector2.zero, new Vector2(m, m), new Vector2(240f, 120f));

            GetOrAdd<CanvasRenderer>(root);
            var frame = GetOrAdd<Image>(root);
            frame.raycastTarget = false;
            StyleFrame(frame, UIFrameKind.Panel, s);

            var column = GetOrAdd<VerticalLayoutGroup>(root);
            column.padding = new RectOffset((int)m, (int)m, (int)m, (int)m);
            column.spacing = s.Spacing;
            column.childAlignment = TextAnchor.UpperLeft;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = false;
            column.childForceExpandHeight = false;
            var fitter = GetOrAdd<ContentSizeFitter>(root);
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // The old hand-placed rows
            DestroyChild(root.transform, "Xbox");

            var mapRows = Group(root.transform, "MapRows", 0, s);
            Row(mapRows, "Move", 0, s, "Move", "xbox_stick_l");
            Row(mapRows, "EnterLevel", 1, s, "Enter level", "xbox_button_color_a");
            Row(mapRows, "Menu", 2, s, "Menu / Stats", "xbox_button_color_y");

            var menuRows = Group(root.transform, "MenuRows", 1, s);
            Row(menuRows, "SwapWindows", 0, s, "Swap windows", "xbox_lb", "xbox_rb");
            Row(menuRows, "Back", 1, s, "Back", "xbox_button_color_b");

            // Gamepad only; the map rows while moving, the menu rows while the gamepad is in the UI
            var visibility = root.GetComponents<InputPromptVisibilityUI>().ToList();
            while (visibility.Count < 3) visibility.Add(root.AddComponent<InputPromptVisibilityUI>());
            ConfigureVisibility(visibility[0], null, GlobalFocusRequirement.Ignore);
            ConfigureVisibility(visibility[1], mapRows.gameObject, GlobalFocusRequirement.NoWindowFocused);
            ConfigureVisibility(visibility[2], menuRows.gameObject, GlobalFocusRequirement.AnyWindowFocused);
            for (int i = visibility.Count - 1; i >= 3; i--) Object.DestroyImmediate(visibility[i]);
        }

        private static RectTransform Group(Transform parent, string name, int index, UISettings s)
        {
            var group = FindOrCreate(parent, name);
            group.SetSiblingIndex(index);
            group.gameObject.SetActive(true);
            var layout = GetOrAdd<VerticalLayoutGroup>(group.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = s.Spacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return group;
        }

        /// <summary>A prompt row: Tiny icon(s), then a Small label.</summary>
        private static void Row(RectTransform parent, string name, int index, UISettings s, string label, params string[] icons)
        {
            var row = FindOrCreate(parent, name);
            row.SetSiblingIndex(index);
            var layout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = s.Spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < icons.Length; i++)
            {
                var icon = FindOrCreateImage(row, i == 0 ? "Icon" : $"Icon{i + 1}");
                icon.transform.SetSiblingIndex(i);
                icon.sprite = LoadSprite(XboxIcons + icons[i] + ".png");
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                FixedLayout(icon.gameObject, s.TinyIconSize, s.TinyIconSize);
                StyleIcon(icon.gameObject, UIIconSize.Tiny, s);
            }

            var text = FindOrCreateText(row, "Label");
            text.transform.SetSiblingIndex(icons.Length);
            text.text = label;
            CenterWindowLayout.PlainText(text, TextAlignmentOptions.MidlineLeft);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            var textLayout = GetOrAdd<LayoutElement>(text.gameObject);
            textLayout.minHeight = s.TinyIconSize;
            StyleText(text, UIFontSize.Small, false, UIColorRole.Text, s);
        }

        private static void ConfigureVisibility(InputPromptVisibilityUI visibility, GameObject target, GlobalFocusRequirement requirement)
        {
            var so = new SerializedObject(visibility);
            so.FindProperty("showOnKeyboardMouse").boolValue = false;
            so.FindProperty("showOnGamepad").boolValue = true;
            var targets = so.FindProperty("targets");
            targets.arraySize = target != null ? 1 : 0;
            if (target != null) targets.GetArrayElementAtIndex(0).objectReferenceValue = target;
            so.FindProperty("window").objectReferenceValue = null;
            so.FindProperty("focusCondition").enumValueIndex = (int)WindowFocusCondition.Any;
            so.FindProperty("globalRequirement").enumValueIndex = (int)requirement;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- LevelSelector scene ---------------------------------------------------------------------------

        /// <summary>The presenter feeds the map window (the former CurrencyPane's display is gone).</summary>
        private static bool WirePresenter(UnityEngine.SceneManagement.Scene scene)
        {
            var presenter = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<OverworldPlayerStatePresenter>(true)).FirstOrDefault();
            var info = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<OverworldMapInfoUI>(true)).FirstOrDefault();
            if (presenter == null || info == null)
            {
                Debug.LogError("[LevelSelectorRestyler] The presenter or the map window is missing from the scene");
                return false;
            }

            var so = new SerializedObject(presenter);
            so.FindProperty("mapInfo").objectReferenceValue = info;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (PrefabUtility.IsPartOfPrefabInstance(presenter))
            {
                // Drops the override of the old playerStatePanel field
                PrefabUtility.RemoveUnusedOverrides(new[] { PrefabUtility.GetOutermostPrefabInstanceRoot(presenter) }, InteractionMode.AutomatedAction);
            }
            return true;
        }

        /// <summary>
        /// The Stats pane, HealthPane and tooltip instances sat thousands of units toward the camera on this Screen
        /// Space - Camera canvas, so wide screens clipped them. Canvas children belong on the canvas plane.
        /// </summary>
        private static bool FlattenCanvasDepth(UnityEngine.SceneManagement.Scene scene)
        {
            bool changed = false;
            foreach (var canvas in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Canvas>(true)).Where(c => c.isRootCanvas))
            {
                foreach (Transform child in canvas.transform)
                {
                    var position = child.localPosition;
                    if (Mathf.Approximately(position.z, 0f)) continue;
                    child.localPosition = new Vector3(position.x, position.y, 0f);
                    if (PrefabUtility.IsPartOfPrefabInstance(child)) PrefabUtility.RecordPrefabInstancePropertyModifications(child);
                    EditorUtility.SetDirty(child);
                    changed = true;
                }
            }
            return changed;
        }

        // --- Helpers ---------------------------------------------------------------------------------------

        private static void FixedLayout(GameObject go, float width, float height)
        {
            var element = GetOrAdd<LayoutElement>(go);
            element.ignoreLayout = false;
            element.minWidth = element.preferredWidth = width;
            element.minHeight = element.preferredHeight = height;
            element.flexibleWidth = element.flexibleHeight = 0f;
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null) Debug.LogError($"[LevelSelectorRestyler] No sprite at {path}");
            return sprite;
        }

        private static RectTransform FindDeep(Transform root, string name) =>
            root.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(t => t.name == name);
    }
}
