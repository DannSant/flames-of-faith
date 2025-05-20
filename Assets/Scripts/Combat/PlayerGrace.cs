using Game.Common;
using Game.Progression;
using Game.Scene;
using UnityEngine;
using static Game.Combat.PlayerHealth;
using static Game.Progression.PlayerProgression;

namespace Game.Combat
{
    public class PlayerGrace : MonoBehaviour
    {
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

        private void Start()
        {
            var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            maxGrace = playerProgression.GetStatTotal(StatType.MaxGrace);
            playerProgression.onStatUpdated += OnMaxPlayerGraceStatUpdated;
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

        


    }

}