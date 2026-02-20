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
        private float lastKnownMaxGrace;
        private float currentGrace = 5;

        public delegate void OnGraceChanged(float current, float max);
        public event OnGraceChanged onGraceChanged;

        public float CurrentGrace { get { return currentGrace; } }
        public float MaxGrace => playerProgression != null
           ? playerProgression.GetStatTotal(StatType.MaxGrace)
           : 0f;

        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        }

        public void InitializeAfterStateReady()
        {
           
            if (playerProgression != null)
            {
                //maxGrace = playerProgression.GetStatTotal(StatType.MaxGrace);
                playerProgression.onDerivedStatsChanged += OnDerivedStatsChanged;
                lastKnownMaxGrace = MaxGrace;
                ClampToMaxGrace();
            }

        }

        private void OnDisable()
        {                  
            if (playerProgression != null)
            {
                playerProgression.onDerivedStatsChanged -= OnDerivedStatsChanged;
            }
        }

        private void OnDerivedStatsChanged()
        {
            float newMax = MaxGrace;

            float delta = newMax - lastKnownMaxGrace;

            if (delta > 0)
            {
                // Increase current grace when max increases
                currentGrace += delta;
            }

            lastKnownMaxGrace = newMax;
            ClampToMaxGrace();
        }

        private void ClampToMaxGrace()
        {
            currentGrace = Mathf.Clamp(currentGrace, 0f, MaxGrace);
            //Debug.Log($"ClampToMaxGrace: currentGrace {currentGrace}: max {max}");
            onGraceChanged?.Invoke(currentGrace, MaxGrace);
        }

        /*private void OnDerivedStatsChanged()
        {
            float newMaxGrace = playerProgression.GetStatTotal(StatType.MaxGrace);  
            Debug.Log("Refreshing Max Grace. New Max Grace: " + newMaxGrace);

            if (Mathf.Approximately(maxGrace,newMaxGrace))
                return;

            ApplyMaxGrace(newMaxGrace);

        }*/

        /*public void ApplyMaxGrace(float value)
        {
            float increasedAmount = value - maxGrace;           
            maxGrace = Mathf.Max(0, value);
            currentGrace = Mathf.Clamp(currentGrace + increasedAmount, 0, maxGrace);
            onGraceChanged?.Invoke(currentGrace, maxGrace);
           

        }*/

        public void AddGrace(float amount)
        {
            currentGrace = Mathf.Clamp(currentGrace + amount, 0f, MaxGrace);
           
            DamageNumberSpawner.Instance.SpawnGraceGainedNumber(transform.position, amount);
            onGraceChanged?.Invoke(currentGrace, MaxGrace);

        }

        public void RemoveGrace(float amount)
        {
            currentGrace = Mathf.Clamp(currentGrace - amount, 0f, MaxGrace);
            
            DamageNumberSpawner.Instance.SpawnGraceLostNumber(transform.position, amount);
            onGraceChanged?.Invoke(currentGrace, MaxGrace);

        }

        public void LoadState()
        {
           
            currentGrace = GameSession.Instance.LoadCurrentGrace();
            lastKnownMaxGrace = MaxGrace;
            //Debug.Log($"Loading Grace {currentGrace}");
            ClampToMaxGrace();
        }

        public void SaveState()
        {           
            GameSession.Instance.SaveCurrentGrace(currentGrace);
           
        }

        public void ResetState()
        {           
            currentGrace = defaultStartingGrace;
            lastKnownMaxGrace = MaxGrace;
            //Debug.Log($"Reseting Grace {currentGrace}");
            ClampToMaxGrace();
        }

        public bool IsAtMaxGrace() => currentGrace >= MaxGrace;
        
    }

}