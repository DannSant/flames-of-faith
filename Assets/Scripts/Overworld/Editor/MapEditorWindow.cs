using Game.Overworld;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Overworld.Editor
{
    public class MapEditorWindow : EditorWindow
    {
        private MapDefinition currentMap;
        private SerializedObject serializedMap;
        private MapGraphView graphView;
        private VisualElement inspectorPanel;

        [MenuItem("Tools/Flames of Faith/Map Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<MapEditorWindow>("Flames of Faith - Overworld Map Editor");
            window.minSize = new Vector2(1200, 800);
        }

        private void CreateGUI()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            // Toolbar
            var toolbar = new Toolbar();
            toolbar.Add(new Button(() => OpenMap()) { text = "Open Map Definition" });
            toolbar.Add(new Button(() => SaveMap()) { text = "Save" });
            toolbar.Add(new Button(() => AddNewNode()) { text = "Add Node" });
            toolbar.Add(new Button(() => DeleteSelected()) { text = "Delete Selected" });
            toolbar.Add(new Toggle() { label = "Snap to Grid", value = true });
            toolbar.Add(new Button(() => graphView?.ZoomToFit()) { text = "Zoom to Fit" });
            toolbar.Add(new Button(() => ValidateMap()) { text = "Validate" });
            rootVisualElement.Add(toolbar);

            // Main content: flex row for graph + inspector
            var content = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, flexGrow = 1 }
            };

            // Graph area
            graphView = new MapGraphView(this);
            graphView.style.flexGrow = 1;
            content.Add(graphView);

            // Inspector panel
            inspectorPanel = new VisualElement
            {
                style = {
                    width = new Length(350, LengthUnit.Pixel),
                    borderLeftWidth = 1,
                    borderLeftColor = new Color(0.2f, 0.2f, 0.2f),
                    paddingLeft = 10,
                    paddingRight = 10,
                    overflow = Overflow.Hidden
                }
            };
            inspectorPanel.Add(new Label("Inspector") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            content.Add(inspectorPanel);

            rootVisualElement.Add(content);

            // Initial load if something selected
            if (Selection.activeObject is MapDefinition map)
            {
                LoadMap(map);
            }
        }

        private void OpenMap()
        {
            var map = EditorUtility.OpenFilePanelWithFilters("Open Map", "Assets", new[] { "Map Definition", "asset" });
            if (!string.IsNullOrEmpty(map))
            {
                var assetPath = "Assets" + map.Substring(Application.dataPath.Length);
                var loadedMap = AssetDatabase.LoadAssetAtPath<MapDefinition>(assetPath);
                if (loadedMap != null)
                {
                    LoadMap(loadedMap);
                }
            }
        }

        public void LoadMap(MapDefinition map)
        {
            currentMap = map;
            serializedMap = new SerializedObject(map);
            graphView?.PopulateFromMap(map, serializedMap);
            UpdateInspector(null); // clear
        }

        public void SaveMap()
        {
            if (currentMap == null) return;
            EditorUtility.SetDirty(currentMap);
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved Map: {currentMap.displayName}");
        }

        public void UpdateInspector(object selected)
        {
            inspectorPanel.Clear();
            inspectorPanel.Add(new Label("Inspector") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            if (selected == null || serializedMap == null)
            {
                inspectorPanel.Add(new Label("Select a node or connection"));
                return;
            }

            // TODO: For now placeholder - later bind to specific SerializedProperty
            // e.g. find node index and use GetArrayElementAtIndex
            var propField = new PropertyField(serializedMap.FindProperty("nodes").GetArrayElementAtIndex(0)); // placeholder
            propField.Bind(serializedMap);
            inspectorPanel.Add(propField);
        }

        private void AddNewNode()
        {
            // TODO: Implement via graphView
            Debug.Log("AddNewNode - to be wired to GraphView");
        }

        private void DeleteSelected()
        {
            // TODO
            Debug.Log("DeleteSelected");
        }

        private void ValidateMap()
        {
            Debug.Log("ValidateMap - basic checks later");
        }
    }
}