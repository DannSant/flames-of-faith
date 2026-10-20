using Game.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// Tools for the UI revamp (Tools/Flames of Faith/UI Revamp). Each one edits the existing prefabs and
    /// scenes in place and can be run again: it finds what it already made instead of adding it twice.
    /// Batchmode: -executeMethod Game.UI.Style.Editor.UIRevampTools.ApplyFoundation (or ReapplyStyles).
    /// </summary>
    public static class UIRevampTools
    {
        public const string SettingsPath = "Assets/Resources/UI/UISettings.asset";
        public const string MarkerPrefabPath = "Assets/Prefabs/UI/Misc/UISelectionMarkerUI.prefab";
        private const string FontPath = "Assets/AssetPackages/ClassicPixelRPGUI/Fonts/PIXEARG_ SDF.asset";
        private const string OutlinedFontPath = "Assets/AssetPackages/ClassicPixelRPGUI/Fonts/PIXEARG_OUT.asset";
        private const string FramesPath = "Assets/Sprites/UI/Frames/";

        // Scenes whose screen-space canvases scale with the screen
        public static readonly string[] ScreenSpaceScenes =
        {
            "Assets/Scenes/Bootstrapper.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/UIMain.unity",
            "Assets/Scenes/LevelSelector.unity",
        };

        private const string LevelSelectorScene = "Assets/Scenes/LevelSelector.unity";
        // Draws the selection frame above every window of its scene
        private const int MarkerSortingOrder = 100;

        [MenuItem("Tools/Flames of Faith/UI Revamp/Apply Foundation")]
        public static void ApplyFoundation()
        {
            if (!Application.isBatchMode)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                if (!EditorUtility.DisplayDialog("UI Revamp: Foundation",
                        "Creates/fills Resources/UI/UISettings, switches the screen-space canvases to Scale With Screen Size, " +
                        "rebuilds the gamepad selection marker and adds it to LevelSelector. Continue?", "Run", "Cancel"))
                {
                    return;
                }
            }

            var log = new List<string>();
            var settings = EnsureSettingsAsset(log);
            RebuildMarkerPrefab(settings, log);

            RunOnScenes(ScreenSpaceScenes, (scene, path) =>
            {
                bool changed = ApplyCanvasScalers(scene, settings, log, path);
                if (path == LevelSelectorScene) changed |= EnsureMarkerInScene(scene, log, path);
                return changed;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("[UIRevampTools] Foundation done.\n" + string.Join("\n", log));
        }

        [MenuItem("Tools/Flames of Faith/UI Revamp/Re-apply UI Styles")]
        public static void ReapplyStyles()
        {
            var settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
            if (settings == null)
            {
                Debug.LogError($"[UIRevampTools] {SettingsPath} not found. Run Apply Foundation first.");
                return;
            }

            var log = new List<string>();
            foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" })
                         .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool changed = false;
                    foreach (var style in root.GetComponentsInChildren<UIStyleBase>(true))
                    {
                        // Nested prefabs are styled in their own asset
                        if (PrefabUtility.IsPartOfPrefabInstance(style)) continue;
                        changed |= style.Apply(settings);
                    }
                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        log.Add(path);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            Debug.Log("[UIRevampTools] Re-applied UI styles.\n" + (log.Count > 0 ? string.Join("\n", log) : "Nothing changed."));
        }

        // --- Settings ---------------------------------------------------------------------------------

        public static UISettings LoadSettings() => AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);

        private static UISettings EnsureSettingsAsset(List<string> log)
        {
            var settings = LoadSettings();
            if (settings == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources/UI")) AssetDatabase.CreateFolder("Assets/Resources", "UI");
                settings = ScriptableObject.CreateInstance<UISettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                log.Add($"Created {SettingsPath}");
            }

            // Fill only empty references, so hand-picked assets are kept on a re-run
            var so = new SerializedObject(settings);
            FillReference(so, "font", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath), log);
            FillReference(so, "outlinedFont", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutlinedFontPath), log);
            FillReference(so, "panelFrame", LoadFrameSprite("PanelFrame.png"), log);
            FillReference(so, "barFrame", LoadFrameSprite("BarFrame.png"), log);
            FillReference(so, "barFill", LoadFrameSprite("BarFillNeutral.png"), log);
            FillReference(so, "selectionFrame", LoadFrameSprite("SelectionFrame.png"), log);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static Sprite LoadFrameSprite(string file)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FramesPath + file);
            if (sprite == null) Debug.LogWarning($"[UIRevampTools] Sprite not found: {FramesPath}{file}");
            return sprite;
        }

        private static void FillReference(SerializedObject so, string property, Object value, List<string> log)
        {
            var prop = so.FindProperty(property);
            if (prop == null || prop.objectReferenceValue != null || value == null) return;
            prop.objectReferenceValue = value;
            log.Add($"UISettings.{property} = {value.name}");
        }

        // --- Canvas scaling ---------------------------------------------------------------------------

        private static bool ApplyCanvasScalers(UnityEngine.SceneManagement.Scene scene, UISettings settings, List<string> log, string path)
        {
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var scaler in root.GetComponentsInChildren<CanvasScaler>(true))
                {
                    var canvas = scaler.GetComponent<Canvas>();
                    if (canvas == null || canvas.renderMode == RenderMode.WorldSpace) continue;
                    // Only root canvases scale; nested ones follow their parent
                    if (scaler.transform.parent != null && scaler.transform.parent.GetComponentInParent<Canvas>(true) != null) continue;

                    if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize
                        && scaler.referenceResolution == settings.ReferenceResolution
                        && scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
                        && Mathf.Approximately(scaler.matchWidthOrHeight, settings.MatchWidthOrHeight))
                    {
                        continue;
                    }

                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = settings.ReferenceResolution;
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = settings.MatchWidthOrHeight;
                    EditorUtility.SetDirty(scaler);
                    changed = true;
                    log.Add($"{path}: {scaler.name} scales with the screen");
                }
            }
            return changed;
        }

        // --- Selection marker -------------------------------------------------------------------------

        private static void RebuildMarkerPrefab(UISettings settings, List<string> log)
        {
            var root = PrefabUtility.LoadPrefabContents(MarkerPrefabPath);
            try
            {
                // Serialized directly: Unity ignores the overrideSorting setter on a canvas that is a root,
                // which this one is while the prefab is edited on its own (it is nested once placed in a scene)
                var canvas = new SerializedObject(UIBuildHelpers.GetOrAdd<Canvas>(root));
                canvas.FindProperty("m_OverrideSorting").boolValue = true;
                canvas.FindProperty("m_SortingOrder").intValue = MarkerSortingOrder;
                canvas.ApplyModifiedPropertiesWithoutUndo();

                // The old arrow child ("Image") becomes the frame
                RectTransform frame = UIBuildHelpers.Find(root.transform, "Frame") ?? UIBuildHelpers.Find(root.transform, "Image");
                if (frame == null) frame = UIBuildHelpers.FindOrCreate(root.transform, "Frame");
                frame.name = "Frame";
                UIBuildHelpers.Place(frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 40f));

                UIBuildHelpers.GetOrAdd<CanvasRenderer>(frame.gameObject);
                var image = UIBuildHelpers.GetOrAdd<Image>(frame.gameObject);
                image.sprite = settings.SelectionFrame;
                image.type = Image.Type.Sliced;
                image.fillCenter = false;
                image.pixelsPerUnitMultiplier = 1f;
                image.color = settings.SelectionColor;
                image.raycastTarget = false;
                image.maskable = false;

                var marker = root.GetComponent<UISelectionMarkerUI>();
                var so = new SerializedObject(marker);
                so.FindProperty("marker").objectReferenceValue = frame;
                so.FindProperty("markerImage").objectReferenceValue = image;
                so.ApplyModifiedPropertiesWithoutUndo();

                UIBuildHelpers.SetLayerRecursively(root, UIBuildHelpers.UILayer);
                PrefabUtility.SaveAsPrefabAsset(root, MarkerPrefabPath);
                log.Add($"Rebuilt {MarkerPrefabPath} as a wrapping frame");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool EnsureMarkerInScene(UnityEngine.SceneManagement.Scene scene, List<string> log, string path)
        {
            if (scene.GetRootGameObjects().Any(r => r.GetComponentInChildren<UISelectionMarkerUI>(true) != null)) return false;

            var canvas = scene.GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<Canvas>(true))
                .FirstOrDefault(c => c.renderMode != RenderMode.WorldSpace && c.transform.parent == null);
            if (canvas == null)
            {
                log.Add($"{path}: no screen-space canvas, marker not added");
                return false;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            instance.transform.SetAsLastSibling();
            log.Add($"{path}: added the selection marker under {canvas.name}");
            return true;
        }

        // --- Scenes -----------------------------------------------------------------------------------

        /// <summary>Opens each scene, runs edit (returns true if it changed something) and saves it if so.</summary>
        public static void RunOnScenes(IEnumerable<string> scenePaths, Func<UnityEngine.SceneManagement.Scene, string, bool> edit)
        {
            var originalSetup = Application.isBatchMode ? null : EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string path in scenePaths)
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    if (edit(scene, path))
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }
            finally
            {
                if (originalSetup != null && originalSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }
        }
    }
}
