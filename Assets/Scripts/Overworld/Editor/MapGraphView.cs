using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace Game.Overworld.Editor
{
    public class MapGraphView : GraphView
    {
        public class ConnectionEdgeData
        {
            public readonly NodeDefinition sourceNode;
            public readonly ConnectionDefinition connection;

            public ConnectionEdgeData(NodeDefinition sourceNode, ConnectionDefinition connection)
            {
                this.sourceNode = sourceNode;
                this.connection = connection;
            }
        }

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

            // Callbacks
            graphViewChanged = OnGraphViewChanged;
            RegisterCallback<MouseUpEvent>(evt => OnSelectionChanged());
        }

        public void PopulateFromMap(MapDefinition mapDef, SerializedObject serMap)
        {
            map = mapDef;
            serializedMap = serMap;

            ClearAllElements();

            var nodeDict = new Dictionary<string, MapNodeView>();

            foreach (var nodeDef in mapDef.nodes)
            {                
                var nodeView = new MapNodeView(nodeDef, this);
                AddElement(nodeView);
                nodeDict[nodeDef.id] = nodeView;

                if (nodeDef.editorPosition == Vector2.zero && nodeDef.worldPosition != Vector2.zero)
                {
                    Vector2 editorPos = nodeDef.worldPosition * MapEditorConstants.NodeSpacing;
                    editorPos.y = editorPos.y * -1f;
                    nodeDef.editorPosition = editorPos;
                }

                nodeView.SetPosition(new Rect(nodeDef.editorPosition, new Vector2(180, 120)));

                if(nodeDef.id == "Node_ec3609f1")
                {                    
                    Debug.Log($"Node_ec3609f1 editor position: {nodeDef.editorPosition}");
                }
            }

            // Create existing connections
            foreach (var nodeDef in mapDef.nodes)
            {
                foreach (var conn in nodeDef.connections)
                {
                    if (!nodeDict.TryGetValue(nodeDef.id, out var source) ||
                        !nodeDict.TryGetValue(conn.toNodeId, out var target))
                        continue;

                    Port sourcePort = GetPortForDirection(source, conn.direction, isOutput: true);
                    Port targetPort = GetPortForDirection(target, conn.direction, isOutput: false);

                    if (sourcePort != null && targetPort != null)
                    {
                        var edge = new Edge
                        {
                            output = sourcePort,
                            input = targetPort,
                            userData = new ConnectionEdgeData(nodeDef, conn)
                        };
                        AddElement(edge);
                    }
                }
            }

            FrameAll();
        }

        private void OnSelectionChanged()
        {
            var selectedNode = selection.OfType<MapNodeView>().FirstOrDefault();
            if (selectedNode != null)
            {
                window?.UpdateInspector(selectedNode);
                return;
            }

            var selectedEdge = selection.OfType<Edge>().FirstOrDefault(e => e.userData is ConnectionEdgeData);
            window?.UpdateInspector(selectedEdge);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.movedElements != null)
            {
                foreach (var elem in change.movedElements)
                {
                    if (elem is MapNodeView nodeView)
                    {
                        Undo.RecordObject(map, "Move Node");
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    HandleNewEdge(edge);
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is Edge edge && edge.userData is ConnectionEdgeData connData)
                    {
                        Undo.RecordObject(map, "Delete Connection");
                        connData.sourceNode.connections.Remove(connData.connection);
                        EditorUtility.SetDirty(map);
                    }
                }
            }

            return change;
        }

        private void HandleNewEdge(Edge edge)
        {
            if (edge.output?.node is not MapNodeView source ||
                edge.input?.node is not MapNodeView target)
                return;

            MapDirection dir = MapDirection.Right;
            if (edge.output == source.downOutPort) dir = MapDirection.Down;
            else if (edge.output == source.upOutPort) dir = MapDirection.Up;
            else if (edge.output == source.leftOutPort) dir = MapDirection.Left;
            else if (edge.output == source.rightOutPort) dir = MapDirection.Right;

            var newConn = new ConnectionDefinition
            {
                toNodeId = target.nodeDef.id,
                direction = dir,
                revealOnClear = true
            };

            Undo.RecordObject(map, "Create Connection");
            source.nodeDef.connections.Add(newConn);

            Vector2 offset = dir switch
            {
                MapDirection.Up => new Vector2(0f, 2f),
                MapDirection.Down => new Vector2(0f, -2f),
                MapDirection.Left => new Vector2(-2f, 0f),
                MapDirection.Right => new Vector2(2f, 0f),
                _ => Vector2.zero
            };
            target.nodeDef.worldPosition = source.nodeDef.worldPosition + offset;

            EditorUtility.SetDirty(map);

            edge.userData = new ConnectionEdgeData(source.nodeDef, newConn);

            Debug.Log($"Created connection {source.nodeDef.id} -> {target.nodeDef.id} ({dir})");
        }

        private Port GetPortForDirection(MapNodeView nodeView, MapDirection direction, bool isOutput)
        {
            switch (direction)
            {
                case MapDirection.Up:
                    return isOutput ? nodeView.upOutPort : nodeView.downInPort;
                case MapDirection.Down:
                    return isOutput ? nodeView.downOutPort : nodeView.upInPort;
                case MapDirection.Left:
                    return isOutput ? nodeView.leftOutPort : nodeView.rightInPort;
                case MapDirection.Right:
                    return isOutput ? nodeView.rightOutPort : nodeView.leftInPort;
                default:
                    return isOutput ? nodeView.rightOutPort : nodeView.leftInPort;
            }
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