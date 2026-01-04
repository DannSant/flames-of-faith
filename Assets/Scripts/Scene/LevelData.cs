using Game.Map;
using UnityEngine;

namespace Game.Scene
{
    public enum LevelType { Combat, Store, Rest, Event, Boss, Treasure, CorruptionClean }

    [CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public string SceneName;
        public string DisplayName;      
        public int actNumber = 1;            

        [Header("Gameplay Settings")]
        public LevelType type;
        public float corruptionIncrease = 1f;

        [Header("Debug Settings")]
        public bool debugShouldPrintObjectCountReport = false;

        public override string ToString()
        {
            return $"{DisplayName} (Scene: {SceneName}, Type: {type}, Act: {actNumber}, corruptionIncrease: {corruptionIncrease})";
        }

    }
}
