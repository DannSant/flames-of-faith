using Game.Common;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Progression
{
    [Serializable]
    public struct PlayerExperienceData
    {
        public int CurrentLevel;
        public float CurrentXP;
        public int ExperienceReductionStat; 
    }

    public class PlayerExperience : MonoBehaviour, IPrimaryStateLoader
    {
        [Header("Experience Growth Settings")]
        [SerializeField] private float baseXPRequired = 10f;
        [SerializeField] private float baseGrowthRate = 1.2f; // 20% per level by default
        [SerializeField] private float growthReductionPerPoint = 0.02f; // 0.01 per 0.5 points
        [SerializeField] private float minGrowthRate = 1.01f; // Prevent flatline or regress

        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentXP = 0f;

        //events
        public event Action<float,int> OnPlayerExperienceGainEvent;
        public delegate void OnLevelUp(int newLevel, int newXPRequired);
        public event OnLevelUp onLevelUp;
        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            // Optionally initialize XP/Level from save data later
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested += ResetPlayerExperienceState;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayStateResetRequested -= ResetPlayerExperienceState;
            }
        }

        private void ResetPlayerExperienceState()
        {
            currentLevel = 1;
            currentXP = 0f;
            OnPlayerExperienceGainEvent?.Invoke(currentXP, 10);            
            onLevelUp?.Invoke(currentLevel, 10);
        }

        public void AddExperience(float amount)
        {
            int xpToLevelUp = GetXPRequired(currentLevel);
          
            currentXP += amount;
            OnPlayerExperienceGainEvent?.Invoke(currentXP, xpToLevelUp);
            while (currentXP >= xpToLevelUp)
            {
                currentXP -= xpToLevelUp;
                currentLevel++;
                int newXpToLevelUp = GetXPRequired(currentLevel);               
                onLevelUp?.Invoke(currentLevel, newXpToLevelUp);
                OnPlayerExperienceGainEvent?.Invoke(currentXP, xpToLevelUp);
            }
        }

        public float GetCurrentXP() => currentXP;
        public int GetCurrentLevel() => currentLevel;
        public int GetXPRequired(int level)
        {
            float reductionStat = playerProgression.GetStatTotal(StatType.ExperienceToLevelUpReduction);
            return CalculateXPRequired(level, reductionStat);
        }

        /// <summary>
        /// XP required ignoring the XP-to-level-up reduction stat. Used to size experience drops,
        /// so the reduction stat still makes the player level faster.
        /// </summary>
        public int GetUnreducedXPRequired(int level) => CalculateXPRequired(level, 0f);

        private int CalculateXPRequired(int level, float reductionStat)
        {
            float reduction = reductionStat * growthReductionPerPoint;
            float dynamicGrowth = baseGrowthRate - reduction;

            dynamicGrowth = Mathf.Max(minGrowthRate, dynamicGrowth); // Prevent abuse

            return Mathf.CeilToInt(baseXPRequired * Mathf.Pow(dynamicGrowth, level - 1));
        }

        public void LoadState()
        {
            //Load state from GameSession
            var playerExperienceData = GameSession.Instance.LoadPlayerExperienceState();
            currentLevel = playerExperienceData.CurrentLevel;
            currentXP = playerExperienceData.CurrentXP;

            // Get the reduction stat from the state instead of the player progression because we don't know if player progression has loaded yet
            float reductionStat = playerExperienceData.ExperienceReductionStat;

            int requiredExperience = CalculateXPRequired(currentLevel, reductionStat);

            // Invoke events with loaded state
            OnPlayerExperienceGainEvent?.Invoke(currentXP, requiredExperience);
            onLevelUp?.Invoke(currentLevel, requiredExperience);

            
        }

        public void SaveState()
        {           
            GameSession.Instance.SavePlayerExperienceState(new PlayerExperienceData
            {
                CurrentLevel = currentLevel,
                CurrentXP = currentXP,
                ExperienceReductionStat = playerProgression.GetStatTotal(StatType.ExperienceToLevelUpReduction)
            });
        }

        public void ResetState()
        {
            ResetPlayerExperienceState();
        }
    }

}