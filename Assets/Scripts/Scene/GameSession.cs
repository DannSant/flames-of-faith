using Game.Common;
using Game.Effects;
using Game.Map;
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

        /*[Header("Level Progression")]
        public List<LevelData> allLevels = new List<LevelData>();
     
        public List<LevelData> beatenLevels = new List<LevelData>();*/

        [Header("Other Settings")]
        public DifficultyLevel selectedDifficulty = DifficultyLevel.Normal;

        public LevelData currentLevel;
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
            playerData = new PlayerData();
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            LevelSelectionController.Instance.InitialSetup();            
        }

        public void MarkLevelBeaten(LevelData level)
        {
            //level.IsBeaten = true;
            LevelSelectionController.Instance.AdvanceToNextLayer();
            /*if (!beatenLevels.Contains(level))
            {
                beatenLevels.Add(level);
            }

            level.IsBeaten = true;

            // Unlock next level
            int index = allLevels.IndexOf(level);
            if (index >= 0 && index + 1 < allLevels.Count)
            {
                allLevels[index + 1].IsUnlocked = true;
            }*/
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

        public void SaveEffectStore(List<EffectInstance> effects)
        {
            GameSession.Instance.playerData.savedEffects = effects;
            //Debug.Log($"Saved {effects.Count} effects to player data store.");
        }

        public List<EffectInstance> LoadEffectStore()
        {
            //Debug.Log($"Loaded {GameSession.Instance.playerData.savedEffects.Count} effects to player data store.");
            return GameSession.Instance.playerData.savedEffects;

        }

        public void SaveCurrencyAmount(int amount)
        {
            playerData.currencyAmount = amount;
        }

        public int LoadCurrencyAmount()
        {
            return playerData.currencyAmount;
        }

        public void SavePlayerExperienceState(PlayerExperienceData data)
        {
            playerData.playerExperienceData = data;
        }

        public PlayerExperienceData LoadPlayerExperienceState()
        {
            return playerData.playerExperienceData;
        }

    }
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }
}
