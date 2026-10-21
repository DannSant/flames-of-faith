using Game.Control;
using Game.Metaprogression;
using Game.UI.Navigation;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.SkillTree.Editor
{
    /// <summary>
    /// Builds Prefabs/UI/Menus/SkillTreePanel.prefab from the same art as the other menus
    /// (ClassicPixelRPGUI frames and buttons, PIXEARG font). Run it again to rebuild from scratch;
    /// tweaks made by hand to the prefab are lost then.
    ///
    /// Drop the prefab under the MainMenu Canvas, point a menu button at SkillTreeController.Open,
    /// and set SkillTreeUI's Confirm Dialog to the scene's ConfirmDialogUI.
    /// </summary>
    public static class SkillTreePanelBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/Menus/SkillTreePanel.prefab";
        private const string Art = "Assets/AssetPackages/ClassicPixelRPGUI/Artwok/UI/";
        private const string FontPath = "Assets/AssetPackages/ClassicPixelRPGUI/Fonts/PIXEARG_ SDF.asset";
        private const string CharacterDataPath = "Assets/Resources/CharacterData/";
        private const int NodeCount = 6;

        private static TMP_FontAsset font;

        [MenuItem("Tools/Flames of Faith/Build Skill Tree Panel Prefab")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null &&
                !EditorUtility.DisplayDialog("Skill Tree Panel",
                    $"{PrefabPath} already exists. Rebuild it? Changes made to it by hand will be lost.", "Rebuild", "Cancel"))
            {
                return;
            }

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) Debug.LogWarning($"[SkillTreePanelBuilder] Font not found at {FontPath}; using the TMP default.");

            // Root: always active, holds the controller (so it's ready before the panel first opens)
            var root = new GameObject("SkillTree", typeof(RectTransform));
            Stretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            var controller = root.AddComponent<SkillTreeController>();

            // Panel: the window, toggled on and off
            var panel = CreateImage("SkillTreePanel", root.transform, "Frame_bg_big_and_title.png");
            Stretch(panel.rectTransform, new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.93f));
            panel.gameObject.AddComponent<CanvasGroup>();
            var window = panel.gameObject.AddComponent<UIWindow>();
            var ui = panel.gameObject.AddComponent<SkillTreeUI>();

            // Header
            var title = CreateText("Title", panel.transform, "Skill Tree", 34, TextAlignmentOptions.Center);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(500f, 50f));

            var favors = CreateText("Favors", panel.transform, "Favors: 0", 24, TextAlignmentOptions.Right);
            Place(favors.rectTransform, new Vector2(1f, 1f), new Vector2(-190f, -95f), new Vector2(320f, 36f));

            // Class tabs
            var tabsRow = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabsRow.transform.SetParent(panel.transform, false);
            Place(tabsRow.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(330f, -95f), new Vector2(560f, 50f));
            var tabLayout = tabsRow.GetComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 10f;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            var tabButtons = new List<Button>();
            var tabLabels = new List<TextMeshProUGUI>();
            foreach (var name in new[] { "Warrior", "Archer", "Augur" })
            {
                var tab = CreateButton($"Tab_{name}", tabsRow.transform, "Shop_tab.png", name, 22, out var label);
                tabButtons.Add(tab);
                tabLabels.Add(label);
            }

            // Tree area (left)
            var tree = new GameObject("Tree", typeof(RectTransform));
            tree.transform.SetParent(panel.transform, false);
            Stretch(tree.GetComponent<RectTransform>(), new Vector2(0.03f, 0.08f), new Vector2(0.62f, 0.82f));

            // Connectors (behind the nodes): trunk from the root, a bar across, a drop to each node
            var trunk = CreateImage("Trunk", tree.transform, "Arrow_talents.png");
            Stretch(trunk.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.72f));
            trunk.rectTransform.sizeDelta = new Vector2(9f, 0f);

            float firstX = 0.5f / NodeCount;
            float lastX = 1f - firstX;
            var bar = CreateImage("Bar", tree.transform, "Decor_description_line.png");
            Stretch(bar.rectTransform, new Vector2(firstX, 0.6f), new Vector2(lastX, 0.6f));
            bar.rectTransform.sizeDelta = new Vector2(0f, 7f);

            for (int i = 0; i < NodeCount; i++)
            {
                float x = (i + 0.5f) / NodeCount;
                var drop = CreateImage($"Drop_{i}", tree.transform, "Arrow_talents.png");
                Stretch(drop.rectTransform, new Vector2(x, 0.44f), new Vector2(x, 0.6f));
                drop.rectTransform.sizeDelta = new Vector2(9f, 0f);
            }

            // Root: the class (unlocks it)
            var rootButton = CreateButton("ClassRoot", tree.transform, "Icon_Frame.png", null, 0, out _);
            Place((RectTransform)rootButton.transform, new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(120f, 120f));
            var rootIcon = CreateImage("Icon", rootButton.transform, null);
            Stretch(rootIcon.rectTransform, Vector2.zero, Vector2.one, 16f);
            rootIcon.preserveAspect = true;
            rootIcon.raycastTarget = false;
            var rootLabel = CreateText("Label", rootButton.transform, "Class", 24, TextAlignmentOptions.Center);
            Place(rootLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -22f), new Vector2(220f, 34f));
            rootButton.gameObject.AddComponent<UIFocusRelay>();

            // Item nodes
            var nodeViews = new List<SkillTreeNodeUI>();
            for (int i = 0; i < NodeCount; i++)
            {
                nodeViews.Add(CreateNode(i, tree.transform, (i + 0.5f) / NodeCount));
            }

            // Details (right)
            var details = CreateImage("Details", panel.transform, "Frame_square_256_vert.png");
            Stretch(details.rectTransform, new Vector2(0.64f, 0.12f), new Vector2(0.97f, 0.82f));

            var detailsTitle = CreateText("DetailsTitle", details.transform, "", 24, TextAlignmentOptions.Top);
            Stretch(detailsTitle.rectTransform, new Vector2(0f, 0.84f), new Vector2(1f, 0.97f), 18f);

            var detailsBody = CreateText("DetailsBody", details.transform, "", 19, TextAlignmentOptions.TopLeft);
            Stretch(detailsBody.rectTransform, new Vector2(0f, 0.18f), new Vector2(1f, 0.84f), 22f);
            detailsBody.enableAutoSizing = true;
            detailsBody.fontSizeMin = 12f;
            detailsBody.fontSizeMax = 19f;

            var detailsFooter = CreateText("DetailsFooter", details.transform, "", 18, TextAlignmentOptions.Center);
            Stretch(detailsFooter.rectTransform, new Vector2(0f, 0.03f), new Vector2(1f, 0.18f), 18f);
            // Colored at runtime by SkillTreeUI: green when the unlock / upgrade is available, red when not

            // Footer
            var closeButton = CreateButton("Back", panel.transform, "1_orange_button_s.png", "Back", 25, out _);
            Place((RectTransform)closeButton.transform, new Vector2(1f, 0f), new Vector2(-150f, 50f), new Vector2(200f, 60f));

            // Wire SkillTreeUI
            var uiSo = new SerializedObject(ui);
            uiSo.FindProperty("favorsText").objectReferenceValue = favors;
            SetList(uiSo.FindProperty("tabButtons"), tabButtons);
            SetList(uiSo.FindProperty("tabLabels"), tabLabels);
            uiSo.FindProperty("tabSprite").objectReferenceValue = LoadSprite("Shop_tab.png");
            uiSo.FindProperty("tabSelectedSprite").objectReferenceValue = LoadSprite("Shop_tab_picked.png");
            uiSo.FindProperty("rootButton").objectReferenceValue = rootButton;
            uiSo.FindProperty("rootIcon").objectReferenceValue = rootIcon;
            uiSo.FindProperty("rootLabel").objectReferenceValue = rootLabel;
            SetList(uiSo.FindProperty("nodeViews"), nodeViews);
            uiSo.FindProperty("detailsTitle").objectReferenceValue = detailsTitle;
            uiSo.FindProperty("detailsBody").objectReferenceValue = detailsBody;
            uiSo.FindProperty("detailsFooter").objectReferenceValue = detailsFooter;
            uiSo.FindProperty("closeButton").objectReferenceValue = closeButton;
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            // Window: primary, opens with the GameObject, above the menu panels (0) and below the
            // confirm dialog (101); Cancel closes it
            var windowSo = new SerializedObject(window);
            windowSo.FindProperty("role").enumValueIndex = (int)UIWindowRole.Primary;
            windowSo.FindProperty("openMode").enumValueIndex = (int)UIWindowOpenMode.GameObjectActive;
            windowSo.FindProperty("focusMode").enumValueIndex = (int)UIWindowFocusMode.AutoFocus;
            windowSo.FindProperty("priority").intValue = 10;
            windowSo.FindProperty("defaultSelectable").objectReferenceValue = rootButton;
            windowSo.ApplyModifiedPropertiesWithoutUndo();
            UnityEventTools.AddPersistentListener(window.OnCancel, ui.RequestClose);

            // Wire the controller
            var classes = new List<CharacterClassData>();
            foreach (var name in new[] { "Warrior", "Archer", "Augur" })
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterClassData>($"{CharacterDataPath}{name}.asset");
                if (data != null) classes.Add(data);
                else Debug.LogWarning($"[SkillTreePanelBuilder] Missing {CharacterDataPath}{name}.asset");
            }
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("ui").objectReferenceValue = ui;
            SetList(controllerSo.FindProperty("classes"), classes);
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            // Closed until opened
            panel.gameObject.SetActive(false);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[SkillTreePanelBuilder] Saved {PrefabPath}.", prefab);
            EditorGUIUtility.PingObject(prefab);
        }

        private static SkillTreeNodeUI CreateNode(int index, Transform parent, float x)
        {
            var button = CreateButton($"Node_{index}", parent, "Icon_Frame.png", null, 0, out _);
            var rect = (RectTransform)button.transform;
            Place(rect, new Vector2(x, 0.34f), Vector2.zero, new Vector2(100f, 100f));

            var icon = CreateImage("Icon", button.transform, null);
            Stretch(icon.rectTransform, Vector2.zero, Vector2.one, 14f);
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var title = CreateText("Title", button.transform, "Item", 18, TextAlignmentOptions.Center);
            Place(title.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -20f), new Vector2(150f, 28f));

            var pipsRow = new GameObject("Pips", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            pipsRow.transform.SetParent(button.transform, false);
            Place(pipsRow.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, -46f), new Vector2(100f, 14f));
            var pipLayout = pipsRow.GetComponent<HorizontalLayoutGroup>();
            pipLayout.spacing = 5f;
            pipLayout.childAlignment = TextAnchor.MiddleCenter;
            pipLayout.childControlWidth = false;
            pipLayout.childControlHeight = false;
            pipLayout.childForceExpandWidth = false;
            pipLayout.childForceExpandHeight = false;

            var pip = CreateImage("PipTemplate", pipsRow.transform, null);
            pip.rectTransform.sizeDelta = new Vector2(12f, 12f);
            pip.raycastTarget = false;

            var cost = CreateText("Cost", button.transform, "0", 18, TextAlignmentOptions.Center);
            Place(cost.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -70f), new Vector2(120f, 26f));

            var node = button.gameObject.AddComponent<SkillTreeNodeUI>();
            var so = new SerializedObject(node);
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("costText").objectReferenceValue = cost;
            so.FindProperty("pipTemplate").objectReferenceValue = pip;
            so.ApplyModifiedPropertiesWithoutUndo();

            pip.gameObject.SetActive(false);
            return node;
        }

        // --- Helpers ----------------------------------------------------------------------------

        private static Sprite LoadSprite(string file)
        {
            if (string.IsNullOrEmpty(file)) return null;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + file);
            if (sprite == null) Debug.LogWarning($"[SkillTreePanelBuilder] Sprite not found: {Art}{file}");
            return sprite;
        }

        private static Image CreateImage(string name, Transform parent, string spriteFile)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = LoadSprite(spriteFile);
            if (image.sprite == null && !string.IsNullOrEmpty(spriteFile)) image.color = new Color(0.15f, 0.12f, 0.1f, 0.9f);
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string spriteFile, string label, float labelSize, out TextMeshProUGUI labelText)
        {
            var image = CreateImage(name, parent, spriteFile);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            labelText = null;
            if (!string.IsNullOrEmpty(label))
            {
                labelText = CreateText("Label", image.transform, label, labelSize, TextAlignmentOptions.Center);
                Stretch(labelText.rectTransform, Vector2.zero, Vector2.one);
            }
            return button;
        }

        /// <summary>Anchors a rect at one point, with a fixed size and offset.</summary>
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }

        /// <summary>Stretches a rect between two anchors, with an optional inset on every side.</summary>
        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, float inset = 0f)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static void SetList<T>(SerializedProperty list, List<T> items) where T : Object
        {
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }
    }
}
