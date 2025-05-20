using Game.Common;
using Game.Scene;
using System;
using UnityEngine;

namespace Game.Progression
{
    public class PlayerExperience : MonoBehaviour
    {
        [Header("Experience Growth Settings")]
        [SerializeField] private float baseXPRequired = 10f;
        [SerializeField] private float baseGrowthRate = 1.2f; // 20% per level by default
        [SerializeField] private float growthReductionPerPoint = 0.02f; // 0.01 per 0.5 points
        [SerializeField] private float minGrowthRate = 1.01f; // Prevent flatline or regress

        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentXP = 0;

        public event Action<int,int> OnPlayerExperienceGainEvent;
        public delegate void OnLevelUp(int newLevel, int newXPRequired);
        public event OnLevelUp onLevelUp;
        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            // Optionally initialize XP/Level from save data later
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += ResetPlayerExperienceState;
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= ResetPlayerExperienceState;
            }
        }

        private void ResetPlayerExperienceState()
        {
            currentLevel = 1;
            currentXP = 0;
            OnPlayerExperienceGainEvent?.Invoke(currentXP, 10);            
            onLevelUp?.Invoke(currentLevel, 10);
        }

        public void AddExperience(int amount)
        {
            int xpToLevelUp = GetXPRequired(currentLevel);
          
            currentXP += amount;
            OnPlayerExperienceGainEvent?.Invoke(currentXP, xpToLevelUp);
            while (currentXP >= xpToLevelUp)
            {
                currentXP -= xpToLevelUp;
                currentLevel++;
                onLevelUp?.Invoke(currentLevel, GetXPRequired(currentLevel));
                //PlayerProgression.Instance.GetStatTotal(StatType.ExperienceToLevelUpReduction);
                
            }
        }

        public int GetCurrentXP() => currentXP;
        public int GetCurrentLevel() => currentLevel;
        public int GetXPRequired(int level)
        {
            float reductionStat = playerProgression.GetStatTotal(StatType.ExperienceToLevelUpReduction);
            float reduction = reductionStat * growthReductionPerPoint;
            float dynamicGrowth = baseGrowthRate - reduction;

            dynamicGrowth = Mathf.Max(minGrowthRate, dynamicGrowth); // Prevent abuse

            return Mathf.CeilToInt(baseXPRequired * Mathf.Pow(dynamicGrowth, level - 1));
        }
    }

}