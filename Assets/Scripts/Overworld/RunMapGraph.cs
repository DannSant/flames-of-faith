using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public class RunMapGraph
    {
        public string mapId;
        public int seed;

        public int actNumber;
        public AudioClip mapMusic;

        public Dictionary<string, RunNode> nodes;
        public string currentNodeId;
    }

}