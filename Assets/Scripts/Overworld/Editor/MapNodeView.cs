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
        // Directional ports
        public Port upPort;
        public Port downPort;
        public Port leftPort;
        public Port rightPort;

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
            style.minWidth = 180;
            style.minHeight = 120;
            style.borderBottomWidth = 4; // for visual type later

            // Create 4 directional ports
            CreateDirectionalPorts();

            RefreshExpandedState();
            RefreshPorts();

        }

        private void CreateDirectionalPorts()
        {
            // Top (Up)
            upPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(MapDirection));
            upPort.portName = "↑ Up";
            titleContainer.Add(upPort);   // Note: topContainer for top ports

            // Bottom (Down)
            downPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(MapDirection));
            downPort.portName = "↓ Down";
            extensionContainer.Add(downPort);

            // Left
            leftPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(MapDirection));
            leftPort.portName = "← Left";
            inputContainer.Add(leftPort); // leftContainer / rightContainer exist in Node

            // Right
            rightPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(MapDirection));
            rightPort.portName = "→ Right";
            outputContainer.Add(rightPort);
        }

        // Called when user drags the node
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);            
        }
    }
}