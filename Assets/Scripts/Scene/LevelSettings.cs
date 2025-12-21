using UnityEngine;

namespace Game.Scene
{
    public class LevelSettings : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;

        public LevelData LevelData => levelData;
    }

}