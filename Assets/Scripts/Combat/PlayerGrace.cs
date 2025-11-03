using Game.Common;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using UnityEngine;
using static Game.Combat.PlayerHealth;
using static Game.Progression.PlayerProgression;

namespace Game.Combat
{
    public class PlayerGrace : MonoBehaviour, ILateInitializable, IDependentStateLoader
    {
        public float defaultMaxGrace = 10;
        public float defaultStartingGrace = 5;
        private float maxGrace = 10;
        private float currentGrace =5;       

        public delegate void OnGraceChanged(float current, float max);
        public event OnGraceChanged onGraceChanged;

        public float CurrentGrace { get { return currentGrace; } }
        public float MaxGrace { get { return maxGrace; } }

        private void  Awake()
        {           
            currentGrace = 0;
        }      

        public void LateInitialize()
        {
            var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            if (playerProgression != null)
            {
                maxGrace = playerProgression.GetStatTotal(StatType.MaxGrace);
                playerProgression.onStatUpdated += OnMaxPlayerGraceStatUpdated;
            }     

            
            
        }

        private void OnDisable()
        {
            var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            playerProgression.onStatUpdated -= OnMaxPlayerGraceStatUpdated;
        }

        private void OnMaxPlayerGraceStatUpdated(StatType statType, int value)
        {
            if (statType == StatType.MaxGrace)
            {
                SetMaxGrace(value);
            }
            
        }

        public void SetMaxGrace(int value)
        {
            maxGrace = value;
            if (currentGrace > maxGrace)
            {
                currentGrace = maxGrace;
            }
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void AddGrace(float amount)
        {
            currentGrace += amount;
            if (currentGrace > maxGrace)
            {
                currentGrace = maxGrace;
            }            
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void RemoveGrace(float amount) {
            currentGrace -= amount;
            if (currentGrace < 0)
            {
                currentGrace = 0;
            }
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void LoadState()
        {
            maxGrace = GameSession.Instance.PlayerData.savedStats[StatType.MaxGrace];
            currentGrace = GameSession.Instance.LoadCurrentGrace();
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void SaveState()
        {
            GameSession.Instance.SaveCurrentGrace(currentGrace);
        }

        public void ResetState()
        {
            maxGrace = defaultMaxGrace;
            currentGrace = defaultStartingGrace;
        }
    }

}