using UnityEngine;

namespace Game.Overworld
{
    public class RunEdge
    {
        public string fromNodeId;
        public string toNodeId;

        public MapDirection direction;
        public bool enabled;
        public bool revealOnClear;
    }
}