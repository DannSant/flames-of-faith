using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Overworld;

namespace Game.Overworld.Editor
{
    public class MapNodeView : Node
    {
        public readonly NodeDefinition nodeDef;
        public Port inputPort;
        public Port outputPort;

        public MapNodeView(NodeDefinition def, MapGraphView graphView)
        {
            this.nodeDef = def;
            title = string.IsNullOrEmpty(def.id) ? "New Node" : def.id.Substring(0, Mathf.Min(8, def.id.Length)) + "...";

            // Ports (output -> connections list, input for incoming visuals)
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            // Basic styling
            style.minWidth = 160;
            style.minHeight = 90;

            // TODO: Enhance with nodeType color, icon, levelData preview
            RefreshExpandedState();
            RefreshPorts();
        }

        // TODO: Override position changed for live sync
    }
}