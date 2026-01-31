using Game.Common;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using UnityEngine;
using static Game.Combat.PlayerHealth;
using static Game.Progression.PlayerProgression;

namespace Game.Combat
{
    public class PlayerGrace : MonoBehaviour, IInitializeAfterStateReady, IDependentStateLoader
    {
        public float defaultMaxGrace = 10;
        public float defaultStartingGrace = 5;
        private float maxGrace = 10;
        private float currentGrace = 5;

        public delegate void OnGraceChanged(float current, float max);
        public event OnGraceChanged onGraceChanged;

        public float CurrentGrace { get { return currentGrace; } }
        public float MaxGrace { get { return maxGrace; } }

        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        }

        public void InitializeAfterStateReady()
        {
           
            if (playerProgression != null)
            {
                maxGrace = playerProgression.GetStatTotal(StatType.MaxGrace);
                playerProgression.onDerivedStatsChanged += RefreshMaxGrace;
            }

        }

        private void OnDisable()
        {                  
            if (playerProgression != null)
            {
                playerProgression.onDerivedStatsChanged -= RefreshMaxGrace;
            }
        }

        private void RefreshMaxGrace()
        {
            float newMaxGrace = playerProgression.GetStatTotal(StatType.MaxGrace);

            if (newMaxGrace == maxGrace)
                return;

            ApplyMaxGrace(newMaxGrace);

        }

        public void ApplyMaxGrace(float value)
        {
            float increasedAmount = value - maxGrace;
            //float percent = currentGrace / maxGrace;
            maxGrace = Mathf.Max(0, value);
            currentGrace = Mathf.Clamp(currentGrace + increasedAmount, 0, maxGrace);
            onGraceChanged?.Invoke(currentGrace, maxGrace);
        }

        public void AddGrace(float amount)
        {
            currentGrace += amount;
            if (currentGrace > maxGrace)
            {
                currentGrace = maxGrace;
            }
            DamageNumberSpawner.Instance.SpawnGraceGainedNumber(transform.position, amount);
            onGraceChanged?.Invoke(currentGrace, maxGrace);

        }

        public void RemoveGrace(float amount)
        {
            currentGrace -= amount;
            if (currentGrace < 0)
            {
                currentGrace = 0;
            }
            DamageNumberSpawner.Instance.SpawnGraceLostNumber(transform.position, amount);
            onGraceChanged?.Invoke(currentGrace, maxGrace);

        }

        public void LoadState()
        {
            /*var savedStats = GameSession.Instance.PlayerData.savedStats;
            int savedMaxGrace;
            if (savedStats != null && savedStats.TryGetValue(StatType.MaxGrace, out savedMaxGrace)) 
            {
                maxGrace = savedMaxGrace;
            }
          
            currentGrace = GameSession.Instance.LoadCurrentGrace();*/
            currentGrace = GameSession.Instance.LoadCurrentGrace();
            RefreshMaxGrace();
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

        public bool IsAtMaxGrace() => currentGrace >= maxGrace;
        
    }

}