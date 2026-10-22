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
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentXP = 0f;

        //events
        public event Action<float,int> OnPlayerExperienceGainEvent;
        public delegate void OnLevelUp(int newLevel, int newXPRequired);
        public event OnLevelUp onLevelUp;

        // The level curve lives on ExperienceSettings; tests pass their own so the designer's tuning can't break them.
        private ExperienceSettings settingsOverride;
        private ExperienceSettings Settings => settingsOverride != null ? settingsOverride : ExperienceSettings.Instance;

        public void UseSettings(ExperienceSettings settings) => settingsOverride = settings;

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
            int required = GetXPRequired(currentLevel);
            OnPlayerExperienceGainEvent?.Invoke(currentXP, required);
            onLevelUp?.Invoke(currentLevel, required);
        }

        public void AddExperience(float amount)
        {
            int xpToLevelUp = GetXPRequired(currentLevel);

            currentXP += amount;
            OnPlayerExperienceGainEvent?.Invoke(currentXP, xpToLevelUp);
            // One big gain can give several levels: each one costs its own level's requirement
            while (currentXP >= xpToLevelUp)
            {
                currentXP -= xpToLevelUp;
                currentLevel++;
                xpToLevelUp = GetXPRequired(currentLevel);
                onLevelUp?.Invoke(currentLevel, xpToLevelUp);
                OnPlayerExperienceGainEvent?.Invoke(currentXP, xpToLevelUp);
            }
        }

        public float GetCurrentXP() => currentXP;
        public int GetCurrentLevel() => currentLevel;
        /// <summary>XP needed to go from this level to the next (ExperienceSettings' level curve).</summary>
        public int GetXPRequired(int level) => Settings.GetXPRequired(level);

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