using Game.Map;
using UnityEngine;

namespace Game.Scene
{
   
    [CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
    public class LevelData : ScriptableObject
    {
        public string SceneName;
        public string DisplayName;
        public bool IsUnlocked;
        public bool IsBeaten;
        public int actNumber = 1;
        public bool IsFirstLevel;
        public LevelType type;
    }
}
