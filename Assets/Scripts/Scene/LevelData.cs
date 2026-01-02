using Game.Map;
using UnityEngine;

namespace Game.Scene
{
   
    [CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public string SceneName;
        public string DisplayName;
        public bool IsUnlocked;
        public bool IsBeaten;
        public int actNumber = 1;
        public bool IsFirstLevel;       

        [Header("Gameplay Settings")]
        public LevelType type;
        public float corruptionIncrease = 1f;

        [Header("Debug Settings")]
        public bool debugShouldPrintObjectCountReport = false;

        public override string ToString()
        {
            return $"{DisplayName} (Scene: {SceneName}, Type: {type}, Act: {actNumber}, Unlocked: {IsUnlocked}, Beaten: {IsBeaten}, corruptionIncrease: {corruptionIncrease})";
        }

    }
}
