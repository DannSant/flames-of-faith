using Game.Map;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scene
{
    [System.Serializable]
    public class LayerConfig
    {
        [Min(0)]
        public int combatCount;
        [Min(0)]
        public int extraCount;
    }
    [CreateAssetMenu(fileName = "ActConfig", menuName = "Level/ActConfig")]
    public class ActConfig : ScriptableObject
    {
        [Header("Act data")]
        public int ActNumber;
        public AudioClip MusicClip;

        [Header("Fixed levels")]
        public LevelData FirstLevel;
        public LevelData BossLevel;

        [Header("Pools")]
        public List<LevelData> combatPool = new();   // Only Combat-type levels
        public List<LevelData> extraPool = new();   // Store/Rest/Event, etc.

        [Header("Structure")]
        [Tooltip("Layer 0 is the first level, last layer is boss. These configs are for the middle layers.")]
        public List<LayerConfig> middleLayers = new();
        public List<LevelType> possibleExtraLevels = new();
    }

}