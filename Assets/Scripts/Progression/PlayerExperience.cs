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
    }

    public class PlayerExperience : MonoBehaviour, IPrimaryStateLoader
    {
        [Header("Experience Growth Settings")]
        [SerializeField] private float baseXPRequired = 10f;
        [SerializeField] private float baseGrowthRate = 1.2f; // 20% per level by default

        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentXP = 0f;

        //events
        public event Action<float,int> OnPlayerExperienceGainEvent;
        public delegate void OnLevelUp(int newLevel, int newXPRequired);
        public event OnLevelUp onLevelUp;

        private void Start()
        {
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
        /// <summary>
        /// XP needed to go from this level to the next. The curve is the same for every player: the
        /// ExperienceToLevelUpReduction stat makes experience drops bigger instead (see WaveExperienceCalculator).
        /// </summary>
        public int GetXPRequired(int level)
        {
            return Mathf.CeilToInt(baseXPRequired * Mathf.Pow(baseGrowthRate, level - 1));
        }

        public void LoadState()
        {
            //Load state from GameSession
            var playerExperienceData = GameSession.Instance.LoadPlayerExperienceState();
            currentLevel = playerExperienceData.CurrentLevel;
            currentXP = playerExperienceData.CurrentXP;

            int requiredExperience = GetXPRequired(currentLevel);

            // Invoke events with loaded state
            OnPlayerExperienceGainEvent?.Invoke(currentXP, requiredExperience);
            onLevelUp?.Invoke(currentLevel, requiredExperience);

            
        }

        public void SaveState()
        {           
            GameSession.Instance.SavePlayerExperienceState(new PlayerExperienceData
            {
                CurrentLevel = currentLevel,
                CurrentXP = currentXP
            });
        }

        public void ResetState()
        {
            ResetPlayerExperienceState();
        }
    }

}