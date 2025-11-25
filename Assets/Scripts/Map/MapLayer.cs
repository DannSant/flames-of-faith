using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Map
{
    public class MapLayer
    {
        public int LayerIndex;
        public List<MapNode> Nodes = new();

        public MapLayer(int index) => LayerIndex = index;

        public override string ToString()
        {
            return string.Join(" | ", Nodes.Select(n => n.ToString()));
        }

    }

}