using UnityEngine;

namespace Game.Scene
{
    [System.Serializable]
    public class LevelData 
    {
        public string SceneName;
        public string DisplayName;
        public bool IsUnlocked;
        public bool IsBeaten;
        public int actNumber = 1;
        public bool IsFirstLevel;
    }
}
