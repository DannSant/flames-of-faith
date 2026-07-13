using Game.Overworld;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Overworld.Editor
{
    public class MapNodeView : Node
    {
        public readonly NodeDefinition nodeDef;

        // Each side has both an Output port (this node originates a connection in that
        // direction) and an Input port (this node receives a connection from that side),
        // so any node can be the owning/source side of a connection in any direction.
        public Port upOutPort;
        public Port upInPort;
        public Port downOutPort;
        public Port downInPort;
        public Port leftOutPort;
        public Port leftInPort;
        public Port rightOutPort;
        public Port rightInPort;

        public MapNodeView(NodeDefinition def, MapGraphView graphView)
        {
            this.nodeDef = def;

            // Title
            title = string.IsNullOrEmpty(def.id) ? "Node" : def.id.Substring(0, Mathf.Min(12, def.id.Length));
            if (def.levelData != null)
            {
                title += $"\n{def.levelData.name}";
            }

            // Style
            style.minWidth = MapEditorConstants.NodeminWidth;
            style.minHeight = MapEditorConstants.NodeMinHeight;
            style.borderBottomWidth = MapEditorConstants.NodeborderBottomWidth;

            // Create 8 directional ports (Out + In per side)
            CreateDirectionalPorts();

            RefreshExpandedState();
            RefreshPorts();

        }

        private void CreateDirectionalPorts()
        {
            // Top (Up)
            upOutPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(MapDirection));
            upOutPort.portName = "↑ Out";
            titleContainer.Add(upOutPort);

            upInPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(MapDirection));
            upInPort.portName = "↑ In";
            titleContainer.Add(upInPort);

            // Bottom (Down)
            downOutPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(MapDirection));
            downOutPort.portName = "↓ Out";
            extensionContainer.Add(downOutPort);

            downInPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(MapDirection));
            downInPort.portName = "↓ In";
            extensionContainer.Add(downInPort);

            // Left
            leftOutPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(MapDirection));
            leftOutPort.portName = "← Out";
            inputContainer.Add(leftOutPort); // leftContainer / rightContainer exist in Node

            leftInPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(MapDirection));
            leftInPort.portName = "← In";
            inputContainer.Add(leftInPort);

            // Right
            rightOutPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(MapDirection));
            rightOutPort.portName = "→ Out";
            outputContainer.Add(rightOutPort);

            rightInPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(MapDirection));
            rightInPort.portName = "→ In";
            outputContainer.Add(rightInPort);
        }

        // Called when user drags the node
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            if (nodeDef != null)
            {
                nodeDef.editorPosition = newPos.position;
            }
        }
    }
}
