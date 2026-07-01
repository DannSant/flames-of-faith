using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;  // Added for .ToList()
using Game.Overworld;

namespace Game.Overworld.Editor
{
    public class MapGraphView : GraphView
    {
        private readonly MapEditorWindow window;
        private MapDefinition map;
        private SerializedObject serializedMap;

        public MapGraphView(MapEditorWindow window)
        {
            this.window = window;

            // Basic setup
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Grid background
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            // Style
            //styleSheets.Add(Resources.Load<StyleSheet>("MapEditorStyles")); // TODO: create USS later or remove if missing

            // Callbacks
            graphViewChanged = OnGraphViewChanged;
        }

        public void PopulateFromMap(MapDefinition mapDef, SerializedObject serMap)
        {
            map = mapDef;
            serializedMap = serMap;

            ClearAllElements();

            // Create nodes
            var nodeDict = new Dictionary<string, MapNodeView>();
            foreach (var nodeDef in mapDef.nodes)
            {
                var nodeView = new MapNodeView(nodeDef, this);               
                AddElement(nodeView);
                nodeDict[nodeDef.id] = nodeView;

                // Initialize editorPosition if not set
                if (nodeDef.editorPosition == Vector2.zero && nodeDef.worldPosition != Vector2.zero)
                {
                    nodeDef.editorPosition = nodeDef.worldPosition * 100f;   // good visual scale
                }

                // Apply position AFTER adding to graph
                nodeView.SetPosition(new Rect(nodeDef.editorPosition, new Vector2(180, 120)));
            }

            // Create connections (edges) //commented due to inputPort and outputPort are no longer defined
            /*foreach (var nodeDef in mapDef.nodes)
            {
                foreach (var conn in nodeDef.connections)
                {
                    if (nodeDict.TryGetValue(nodeDef.id, out var source) &&
                        nodeDict.TryGetValue(conn.toNodeId, out var target))
                    {
                        var edge = new Edge
                        {
                            output = source.outputPort,
                            input = target.inputPort
                        };
                        AddElement(edge);
                    }
                }
            }*/

            // Refresh
            FrameAll();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Handle moves, adds, removes for undo etc.
            if (change.movedElements != null)
            {
                foreach (var elem in change.movedElements)
                {
                    if (elem is MapNodeView nodeView)
                    {
                        Undo.RecordObject(map, "Move Node");
                        // Update worldPosition from visual position
                        // nodeView.nodeDef.worldPosition = nodeView.GetPosition().position;
                    }
                }
            }

            // TODO: Handle edge creation/deletion to update ConnectionDefinition lists

            return change;
        }

        private void ClearAllElements()
        {
            foreach (var elem in graphElements.ToList())
            {
                RemoveElement(elem);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (startPort != port && startPort.node != port.node &&
                    startPort.direction != port.direction)
                {
                    compatible.Add(port);
                }
            }
            return compatible;
        }

        public void ZoomToFit()
        {
            FrameAll();
        }
    }
}