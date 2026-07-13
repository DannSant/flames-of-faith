using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    [CreateAssetMenu(menuName = "Map/Map Definition")]
    public class MapDefinition : ScriptableObject
    {
        public string mapId; // for save/load compatibility
        public string displayName;
        public string startNodeId;
        public List<NodeDefinition> nodes;
        public AudioClip mapMusic;

        [SerializeField] private bool isValid;
        public bool Valid
        {
            get => isValid;
            set => isValid = value;
        }
    }

}