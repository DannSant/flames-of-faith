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
        public int defaultMaxGrace = 10;
        private int maxGrace = 10;
        private int currentGrace;       

        public delegate void OnGraceChanged(int current, int max);
        public event OnGraceChanged onGraceChanged;

        public int CurrentGrace { get { return currentGrace; } }
        public int MaxGrace { get { return maxGrace; } }

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

        public void AddGrace(int amount)
        {
            currentGrace += amount;
            if (currentGrace > maxGrace)
            {
                currentGrace = maxGrace;
            }            
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void RemoveGrace(int amount) {
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
            currentGrace = maxGrace;
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void SaveState()
        {
           //no need to save, it is saved in the player progression component
        }

        public void ResetState()
        {
            maxGrace = defaultMaxGrace;
        }
    }

}