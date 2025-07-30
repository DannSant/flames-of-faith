using System.Collections.Generic;
using UnityEngine;

namespace Game.Scene
{
    public class LevelTree
    {
        public LevelNode Root;
    }

   

    public class LevelNode
    {
        public LevelData LevelData;
        public List<LevelNode> Children = new List<LevelNode>();

        public LevelNode(LevelData data)
        {
            LevelData = data;
        }
    }

}