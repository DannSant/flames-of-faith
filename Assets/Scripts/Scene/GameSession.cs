using Game.Common;
using UnityEngine;

namespace Game.Scene
{
    public class GameSession : Singleton<GameSession>
    {
        [Header("Character Selection")]
        public int SelectedPlayerIndex = 0;

        [Header("Other Settings")]
        public DifficultyLevel selectedDifficulty = DifficultyLevel.Normal;
        public bool isNewRun = true;

        protected override void Awake()
        {
            base.Awake();           
        }
    }
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }
}
