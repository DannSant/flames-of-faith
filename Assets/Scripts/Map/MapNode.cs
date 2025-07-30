
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Map
{
    public enum LevelType { Combat, Store, Rest, Event, Boss }
    public class MapNode
    {
        public LevelType Type { get; set; }  
        public List<MapNode> Children { get; } = new();
        public List<MapNode> Parents { get; } = new();

        public LevelData levelData;

        public MapNode(LevelType type)
        {
            Type = type;
        }
    }
}
