using Game.Common;
using Game.Effects;
using Game.Map;
using Game.Metaprogression;
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

        [Header("Other Settings")]
        public DifficultyLevel selectedDifficulty = DifficultyLevel.Normal;

        public LevelData currentLevel;

        //state
        private bool isNewRun = true;
        private bool isInitialized = false;
        private PlayerData playerData;
        private int levelsBeaten = 0; // Track how many levels have been beaten in this session

        //Properties
        public bool IsInitialized => isInitialized;
        public bool IsNewRun  => isNewRun;
        public PlayerData PlayerData => playerData;
        public int LevelsBeaten => levelsBeaten;


        protected override void Awake()
        {
            base.Awake();
            playerData = new PlayerData();
            Application.targetFrameRate = 60;            
        }

        private void Start()
        {
            Initialize();           
        }

        public void Initialize()
        {
            levelsBeaten = 0;
            LevelSelectionController.Instance.InitialSetup();            
        }

        public void MarkLevelBeaten(LevelData level)
        {           
            LevelSelectionController.Instance.AdvanceToNextLayer();
            levelsBeaten++;            
        }

        public void SetIsNewRun(bool value)
        {
            isNewRun = value;
        }

        public void SaveCurrentHealth(float health)
        {
            playerData.currentHealth = health;
        }

        public void SaveCurrentGrace(float grace)
        {
            playerData.currentGrace = grace;
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

        public float LoadCurrentHealth()
        {
            return playerData.currentHealth;
        }
        public float LoadCurrentGrace()
        {
            Debug.Log($"Loaded Grace: {playerData.currentGrace}");
            return playerData.currentGrace;
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
