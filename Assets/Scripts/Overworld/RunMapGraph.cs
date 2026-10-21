using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public class RunMapGraph
    {
        public string mapId;
        public int seed;

        public int actNumber;
        /// <summary>The act's name from its MapDefinition (e.g. "Jesterwoods"), shown on the map.</summary>
        public string displayName;
        public AudioClip mapMusic;

        public Dictionary<string, RunNode> nodes;
        public string currentNodeId;
    }

}