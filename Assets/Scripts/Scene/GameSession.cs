using Game.Common;
using Game.Progression;
using Game.Saving;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scene
{
    public class GameSession : Singleton<GameSession>
    {
        [Header("Character Selection")]
        public int SelectedPlayerIndex = 0;

        [Header("Level Progression")]
        public List<LevelData> allLevels = new List<LevelData>();
        public LevelData currentLevel;
        public List<LevelData> beatenLevels = new List<LevelData>();

        [Header("Other Settings")]
        public DifficultyLevel selectedDifficulty = DifficultyLevel.Normal;

        //state
        private bool isNewRun = true;
        private bool isInitialized = false;
        private PlayerData playerData;

        //Properties
        public bool IsInitialized => isInitialized;
        public bool IsNewRun  => isNewRun;
        public PlayerData PlayerData => playerData;
       

        protected override void Awake()
        {
            base.Awake();           
        }

        public void Initialize()
        {
            if (isInitialized) return;

            beatenLevels.Clear();
            

            isInitialized = true;
        }

        public void MarkLevelBeaten(LevelData level)
        {
            if (!beatenLevels.Contains(level))
            {
                beatenLevels.Add(level);
            }

            level.IsBeaten = true;

            // Unlock next level
            int index = allLevels.IndexOf(level);
            if (index >= 0 && index + 1 < allLevels.Count)
            {
                allLevels[index + 1].IsUnlocked = true;
            }
        }

        public List<LevelData> GetAvailableLevels()
        {
            var result = new List<LevelData>(beatenLevels);

            var nextLevel = allLevels.Find(l => !l.IsBeaten);
            if (nextLevel != null && !result.Contains(nextLevel))
            {
                result.Add(nextLevel);
            }

            return result;
        }

        public void SetIsNewRun(bool value)
        {
            isNewRun = value;
        }

        public void SaveCurrentHealth(int health)
        {
            playerData.currentHealth = health;
        }

        public void SaveStats(Dictionary<StatType, int> stats)
        {
            playerData.savedStats.Clear();
            foreach (var stat in stats)
            {
                playerData.savedStats[stat.Key] = stat.Value;
            }
        }

    }
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }
}
