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
        [Tooltip("How corrupted this level is. Each level adds a base chance for enemies to spawn Corrupted (see CorruptionSettings).")]
        [Min(0)]
        public int taintLevel = 0;
        public bool preventHealthRegen = false;
        public bool preventAttacks = false;
        public bool allowPause = true;

        [Header("Debug Settings")]
        public bool debugShouldPrintObjectCountReport = false;

         [Header("Music Settings")]
        public AudioClip MusicClip;

        public override string ToString()
        {
            return $"{DisplayName} (Scene: {SceneName}, Type: {type}, Act: {actNumber}, Taint: {taintLevel})";
        }

    }
}
