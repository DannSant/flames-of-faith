using Game.Scene;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Overworld
{
    [Serializable]
    public class NodeDefinition
    {
        [Header("Identity")]
        [Tooltip("Unique ID for this node")]
        public string id; // must be unique, stable across versions

        [Header("Visual")]
        [Tooltip("World-space sprite position")]
        public Vector2 worldPosition;
        [Tooltip("Determines sprite/icon")]
        public LevelType nodeType; // 

        [Header("Level")]
        [Tooltip("Scene to load ")]
        public LevelData levelData;
        public bool isFirstLevel;
                                    

        //[Header("Connections")]
        public List<ConnectionDefinition> connections;

        //[HideInInspector]
        public Vector2 editorPosition;
    }

}