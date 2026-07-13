using Codice.Client.BaseCommands;
using Game.Overworld;
using Game.Scene;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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

        private NodeDefinition selectedNodeDef;

        [MenuItem("Tools/Flames of Faith/Map Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<MapEditorWindow>("Overworld Map Editor");
            window.minSize = new Vector2(1200, 800);
        }

        private void CreateGUI()
        {           
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            // Toolbar
            var toolbar = new Toolbar();
            toolbar.Add(new Button(() => OpenMap()) { text = "Open Map Definition" });
            toolbar.Add(new Button(() => CreateNewMap()) { text = "New Map" });
            toolbar.Add(new Button(() => SaveMap()) { text = "Save" });
            toolbar.Add(new Button(() => AddNewNode()) { text = "Add Node" });
            toolbar.Add(new Button(() => DeleteSelected()) { text = "Delete Selected" });
            toolbar.Add(new Button(() => BakeEditorPositionsToWorldPositions()) { text = "Bake Positions" });
            toolbar.Add(new Button(() => ResetEditorPositions()) { text = "Reset Editor Positions" });
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
            string path = EditorUtility.OpenFilePanelWithFilters("Open Map Definition", "Assets", new[] { "Map Definition", "asset" });
            if (string.IsNullOrEmpty(path)) return;

            string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
            var loadedMap = AssetDatabase.LoadAssetAtPath<MapDefinition>(assetPath);
            if (loadedMap != null)
            {
                LoadMap(loadedMap);
            }
            else
            {
                Debug.LogError("Failed to load MapDefinition at: " + assetPath);
            }
        }

        private void CreateNewMap()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Map Definition", "newMap", "asset", "Choose a location to save the new map definition");
            if (string.IsNullOrEmpty(path)) return;

            var newMap = CreateInstance<MapDefinition>();
            newMap.mapId = "newMap";
            newMap.displayName = "newMap";

            var startNode = new NodeDefinition
            {
                id = "level1",
                isFirstLevel = true,
                connections = new List<ConnectionDefinition>()
            };

            newMap.nodes = new List<NodeDefinition> { startNode };
            newMap.startNodeId = "level1";

            AssetDatabase.CreateAsset(newMap, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LoadMap(newMap);
        }

        public void LoadMap(MapDefinition map)
        {
            if (map == null) return;

            currentMap = map;
            serializedMap = new SerializedObject(map);

            if (graphView != null)
            {
                graphView.PopulateFromMap(map, serializedMap);
            }

            UpdateInspector(null);
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
            if(inspectorPanel == null)
            {              
                Debug.LogWarning("Inspector panel is null, cannot update inspector");
                return;
            }
            inspectorPanel.Clear();
            inspectorPanel.Add(new Label("Inspector") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
          
            if (selected is MapNodeView nodeView && serializedMap != null)
            {
                selectedNodeDef = nodeView.nodeDef;
                // Find the index in the nodes list
                int index = currentMap.nodes.IndexOf(selectedNodeDef);
                if (index >= 0)
                {
                    var nodesProp = serializedMap.FindProperty("nodes");
                    var nodeProp = nodesProp.GetArrayElementAtIndex(index);
                    var propField = new PropertyField(nodeProp);
                    propField.Bind(serializedMap);
                    inspectorPanel.Add(propField);
                }
            }
            else
            {
                inspectorPanel.Add(new Label("Select a node or connection in the graph"));
            }
        }

        private void BakeEditorPositionsToWorldPositions()
        {
            if(currentMap == null) return;
            var map = currentMap;
            // Find the minimum editor coordinates
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            foreach (var n in map.nodes)
                min = Vector2.Min(min, n.editorPosition);

            float scale = MapEditorConstants.NodeSpacing;

            foreach (var n in map.nodes)
            {
                Vector2 relative = (n.editorPosition - min) / scale;
                relative.y = -relative.y;                      // <--- Invert back
                n.worldPosition = new Vector2(Mathf.Round(relative.x), Mathf.Round(relative.y));
            }
        }

        private void ResetEditorPositions()
        {
            if (currentMap == null) return;
            foreach (var n in currentMap.nodes)
            {
                n.editorPosition = Vector2.zero;
            }
            graphView.PopulateFromMap(currentMap, serializedMap);
        }

        private void AddNewNode()
        {
            if (currentMap == null) return;

            Undo.RecordObject(currentMap, "Add Node");

            var newNode = new NodeDefinition
            {
                id = "Node_" + System.Guid.NewGuid().ToString("N")[..8],
                connections = new List<ConnectionDefinition>(),
                editorPosition = new Vector2(0f, 0f)
            };

            currentMap.nodes.Add(newNode);
            EditorUtility.SetDirty(currentMap);

            serializedMap.Update();
            graphView.PopulateFromMap(currentMap, serializedMap);
        }

        private void DeleteSelected()
        {
            if (currentMap == null) return;
            if (selectedNodeDef == null) return;

            Undo.RecordObject(currentMap, "Delete Node");

            currentMap.nodes.Remove(selectedNodeDef);
            selectedNodeDef = null;
            EditorUtility.SetDirty(currentMap);

            serializedMap.Update();
            graphView.PopulateFromMap(currentMap, serializedMap);
            UpdateInspector(null);
        }

        private void ValidateMap()
        {
            if (currentMap == null) return;

            bool isValid = true;
            var nodes = currentMap.nodes ?? new List<NodeDefinition>();

            // Rule: no duplicate ids
            var seenIds = new HashSet<string>();
            foreach (var node in nodes)
            {
                if (string.IsNullOrEmpty(node.id))
                {
                    Debug.LogError($"ValidateMap: node has empty id.");
                    isValid = false;
                    continue;
                }

                if (!seenIds.Add(node.id))
                {
                    Debug.LogError($"ValidateMap: duplicate node id '{node.id}'.");
                    isValid = false;
                }
            }

            // Rule: no duplicate world positions
            var seenPositions = new HashSet<Vector2>();
            foreach (var node in nodes)
            {
                if (!seenPositions.Add(node.worldPosition))
                {
                    Debug.LogError($"ValidateMap: duplicate worldPosition {node.worldPosition} (node '{node.id}').");
                    isValid = false;
                }
            }

            // Rule: exactly one first level
            var firstLevelNodes = nodes.FindAll(n => n.isFirstLevel);
            if (firstLevelNodes.Count != 1)
            {
                Debug.LogError($"ValidateMap: expected exactly one node with isFirstLevel, found {firstLevelNodes.Count}.");
                isValid = false;
            }

            // Rule: exactly one boss node
            var bossNodes = nodes.FindAll(n => n.nodeType == LevelType.Boss);
            if (bossNodes.Count != 1)
            {
                Debug.LogError($"ValidateMap: expected exactly one node with LevelType.Boss, found {bossNodes.Count}.");
                isValid = false;
            }

            // Rule: at least one path between first level and boss (edges treated as bidirectional)
            if (firstLevelNodes.Count == 1 && bossNodes.Count == 1)
            {
                var adjacency = new Dictionary<string, List<string>>();
                foreach (var node in nodes)
                {
                    if (!adjacency.ContainsKey(node.id))
                        adjacency[node.id] = new List<string>();

                    if (node.connections == null) continue;
                    foreach (var connection in node.connections)
                    {
                        if (string.IsNullOrEmpty(connection.toNodeId)) continue;

                        adjacency[node.id].Add(connection.toNodeId);

                        if (!adjacency.ContainsKey(connection.toNodeId))
                            adjacency[connection.toNodeId] = new List<string>();
                        adjacency[connection.toNodeId].Add(node.id);
                    }
                }

                string startId = firstLevelNodes[0].id;
                string bossId = bossNodes[0].id;

                var visited = new HashSet<string> { startId };
                var queue = new Queue<string>();
                queue.Enqueue(startId);

                while (queue.Count > 0)
                {
                    string current = queue.Dequeue();
                    if (!adjacency.TryGetValue(current, out var neighbors)) continue;

                    foreach (var neighbor in neighbors)
                    {
                        if (visited.Add(neighbor))
                            queue.Enqueue(neighbor);
                    }
                }

                if (!visited.Contains(bossId))
                {
                    Debug.LogError($"ValidateMap: no path found between first level '{startId}' and boss level '{bossId}'.");
                    isValid = false;
                }
            }

            currentMap.Valid = isValid;
            EditorUtility.SetDirty(currentMap);

            Debug.Log(isValid ? $"ValidateMap: '{currentMap.displayName}' is valid." : $"ValidateMap: '{currentMap.displayName}' failed validation.");
        }
    }
}