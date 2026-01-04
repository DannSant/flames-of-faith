using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public enum RunNodeState
    {
        LockedHidden,  // fog of war
        Revealed,      // visible but not cleared
        Cleared,
        Blocked        // RNG-disabled or gated
    }

    public class RunNode
    {
        public string id;
        public LevelType nodeType;
        public LevelData levelData;      
        public Vector2 worldPosition;

        public RunNodeState state;

        public List<RunEdge> outgoingEdges;

        public override string ToString()
        {
            return $"{id} (Type: {nodeType}, State: {state}, Position: {worldPosition}, Edges: {outgoingEdges.Count})";
        }
    }
}
