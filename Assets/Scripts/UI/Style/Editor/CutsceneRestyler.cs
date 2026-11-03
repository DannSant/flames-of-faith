using Game.Cutscenes;
using Game.UI.Cutscenes;
using Game.UI.InputPrompts;
using Game.UI.Navigation;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// Builds the cutscene player (Prefabs/UI/Cutscenes/CutscenePlayer) and places it in the Bootstrapper scene: a root
    /// canvas above the loading screen, and a window with the page image filling the screen, the text in a band across
    /// its bottom quarter, and Next and the hold-to-skip bar at the band's right.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.CutsceneRestyler.Restyle
    /// </summary>
    public static class CutsceneRestyler
    {
        public const string PlayerPath = "Assets/Prefabs/UI/Cutscenes/CutscenePlayer.prefab";
        private const string BootstrapperScene = "Assets/Scenes/Bootstrapper.unity";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";

        /// <summary>The art keeps its bottom quarter free for the text.</summary>
        public const float BandHeightFraction = 0.25f;
        /// <summary>Above the loading screen (99) and the scenes' selection frame (100).</summary>
        public const int CanvasSortingOrder = 101;
        /// <summary>Above every other window, so the gamepad can't reach what's behind the scene.</summary>
        public const int WindowPriority = 200;
        private const float SkipBarInset = 2f;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Restyle Cutscene Window")]
        public static void Restyle()
        {
            var settings = UIRevampTools.LoadSettings();
            if (settings == null)
            {
                Debug.LogError($"[CutsceneRestyler] {UIRevampTools.SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            EnsurePrefab(PlayerPath);
            UpgradeRestyler.EditPrefab(PlayerPath, root => RestylePlayer(root, settings));

            UIRevampTools.RunOnScenes(new[] { BootstrapperScene }, (scene, path) =>
            {
                bool placed = scene.GetRootGameObjects().Any(r => r.GetComponent<CutscenePlayer>() != null);
                if (placed) return false;
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath), scene);
                return true;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[CutsceneRestyler] Cutscene player built.");
        }

        /// <summary>Creates an empty UI prefab at this path the first time, so a restyler can edit it like any other.</summary>
        public static void EnsurePrefab(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            string folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(folder).Replace('\\', '/'), System.IO.Path.GetFileName(folder));
            }

            var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path), typeof(RectTransform));
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void RestylePlayer(GameObject root, UISettings s)
        {
            float m = s.Margin;
            float buttonHeight = s.GetButtonHeight(UIButtonSize.Small);
            float buttonWidth = CenterWindowLayout.ProgressButtonWidth;
            float sideInset = 2f * m + buttonWidth;

            GetOrAdd<CutscenePlayer>(root);
            GetOrAdd<CutsceneDatabaseProvider>(root);

            var canvas = GetOrAdd<Canvas>(root);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasSortingOrder;
            var scaler = GetOrAdd<CanvasScaler>(root);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = s.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = s.MatchWidthOrHeight;
            GetOrAdd<GraphicRaycaster>(root);

            var windowRect = FindOrCreate(root.transform, "Window");
            StretchFull(windowRect);
            var windowGroup = GetOrAdd<CanvasGroup>(windowRect.gameObject);
            var window = GetOrAdd<UIWindow>(windowRect.gameObject);
            var ui = GetOrAdd<CutsceneWindowUI>(windowRect.gameObject);

            // Behind everything: black bars for other aspect ratios, and a click anywhere advances
            var backdrop = FindOrCreateImage(windowRect, "ClickArea");
            StretchFull(backdrop.rectTransform);
            backdrop.sprite = s.SolidFill;
            backdrop.color = Color.black;
            backdrop.raycastTarget = true;
            var clickArea = GetOrAdd<Button>(backdrop.gameObject);
            clickArea.transition = Selectable.Transition.None;
            clickArea.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };

            var page = FindOrCreate(windowRect, "Page");
            StretchFull(page);
            var pageGroup = GetOrAdd<CanvasGroup>(page.gameObject);
            pageGroup.blocksRaycasts = false;

            var image = FindOrCreateImage(page, "Image");
            StretchFull(image.rectTransform);
            image.preserveAspect = true;
            image.raycastTarget = false;

            var band = FindOrCreateImage(page, "Band");
            Stretch(band.rectTransform, Vector2.zero, new Vector2(1f, BandHeightFraction));
            band.sprite = s.SolidFill;
            band.color = s.ModalBackdropColor;
            band.raycastTarget = false;

            var text = FindOrCreateText(band.rectTransform, "Text");
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, sideInset, m, sideInset, m);
            CenterWindowLayout.PlainText(text, TextAlignmentOptions.MidlineLeft);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.text = "Long ago the Drekos came from the stars to rule the world.";
            StyleText(text, UIFontSize.Big, false, UIColorRole.Text, s);

            // Next and the skip bar, stacked at the band's right, outside the Page group so they don't fade
            var next = FindOrCreate(windowRect, "NextButton");
            var nextImage = GetOrAdd<Image>(next.gameObject);
            var nextButton = GetOrAdd<Button>(next.gameObject);
            nextButton.targetGraphic = nextImage;
            var nextLabel = FindOrCreateText(next, "Label");
            StretchFull(nextLabel.rectTransform);
            nextLabel.text = "Next";
            Place(next, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-m, m + buttonHeight + s.Spacing), new Vector2(buttonWidth, buttonHeight));
            CenterWindowLayout.StyleSmallButton(next, s);

            var skip = FindOrCreateImage(windowRect, "SkipBar");
            Place(skip.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-m, m), new Vector2(buttonWidth, buttonHeight));
            skip.raycastTarget = true;
            StyleFrame(skip, UIFrameKind.Bar, s);
            var hold = GetOrAdd<HoldButtonUI>(skip.gameObject);

            var fill = FindOrCreateImage(skip.rectTransform, "Fill");
            StretchFull(fill.rectTransform, SkipBarInset);
            fill.sprite = s.BarFill;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;
            var fillColor = GetOrAdd<UIStyleColor>(fill.gameObject);
            fillColor.Configure(UIColorRole.Highlight);
            fillColor.Apply(s);

            // The Cancel glyph (Esc, B, Circle) tells which button to hold
            var glyph = FindOrCreateImage(skip.rectTransform, "Glyph");
            float glyphSize = s.TinyIconSize;
            Place(glyph.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(SkipBarInset, 0f), new Vector2(glyphSize, glyphSize));
            glyph.raycastTarget = false;
            StyleIcon(glyph.gameObject, UIIconSize.Tiny, s);
            var prompt = new SerializedObject(GetOrAdd<InputPromptIconUI>(glyph.gameObject));
            prompt.FindProperty("action").objectReferenceValue = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath)
                .OfType<InputActionReference>().First(r => r.action != null && r.action.name == "Cancel" && r.action.actionMap.name == "UI");
            prompt.ApplyModifiedPropertiesWithoutUndo();

            var label = FindOrCreateText(skip.rectTransform, "Label");
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, glyphSize + 2f * SkipBarInset);
            CenterWindowLayout.PlainText(label, TextAlignmentOptions.Center);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.text = "Hold to skip";
            StyleText(label, UIFontSize.Small, false, UIColorRole.Text, s);

            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("role").enumValueIndex = (int)UIWindowRole.Primary;
            windowSo.FindProperty("openMode").enumValueIndex = (int)UIWindowOpenMode.GameObjectActive;
            windowSo.FindProperty("priority").intValue = WindowPriority;
            windowSo.FindProperty("focusMode").enumValueIndex = (int)UIWindowFocusMode.AutoFocus;
            windowSo.FindProperty("blocksGameplayInput").boolValue = true;
            windowSo.FindProperty("modal").boolValue = true;
            windowSo.FindProperty("defaultSelectable").objectReferenceValue = nextButton;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            CenterWindowLayout.UseStandardMarker(window);

            var uiSo = new SerializedObject(ui);
            uiSo.FindProperty("windowGroup").objectReferenceValue = windowGroup;
            uiSo.FindProperty("pageGroup").objectReferenceValue = pageGroup;
            uiSo.FindProperty("pageImage").objectReferenceValue = image;
            uiSo.FindProperty("pageText").objectReferenceValue = text;
            uiSo.FindProperty("clickArea").objectReferenceValue = clickArea;
            uiSo.FindProperty("nextButton").objectReferenceValue = nextButton;
            uiSo.FindProperty("nextLabel").objectReferenceValue = nextLabel;
            uiSo.FindProperty("skipBar").objectReferenceValue = hold;
            uiSo.FindProperty("skipFill").objectReferenceValue = fill;
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            var playerSo = new SerializedObject(root.GetComponent<CutscenePlayer>());
            playerSo.FindProperty("window").objectReferenceValue = ui;
            playerSo.ApplyModifiedPropertiesWithoutUndo();

            windowRect.gameObject.SetActive(false);
        }
    }
}
