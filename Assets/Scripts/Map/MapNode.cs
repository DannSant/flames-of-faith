
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Map
{
  
    [Obsolete]
    public class MapNode
    {
        public LevelType Type { get; set; }  
        public LevelData levelData;

        public MapNode(LevelType type)
        {
            Type = type;
        }
        public override string ToString()
        {
            string levelName = levelData != null ? levelData.DisplayName : "null";
            return $"{Type} ({levelName})";
        }
    }
}
