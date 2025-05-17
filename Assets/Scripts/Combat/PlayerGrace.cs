using Game.Common;
using Game.Progression;
using UnityEngine;
using static Game.Combat.PlayerHealth;
using static Game.Progression.PlayerProgression;

namespace Game.Combat
{
    public class PlayerGrace : Singleton<PlayerGrace>
    {
        private int maxGrace = 10;
        private int currentGrace;       

        public delegate void OnGraceChanged(int current, int max);
        public event OnGraceChanged onGraceChanged;

        public int CurrentGrace { get { return currentGrace; } }
        public int MaxGrace { get { return maxGrace; } }

        protected override void  Awake()
        {
            base.Awake();
            currentGrace = 0;
        }

        private void Start()
        {
            maxGrace = PlayerProgression.Instance.GetStatTotal(StatType.MaxGrace);
            PlayerProgression.Instance.onStatUpdated += OnMaxPlayerGraceStatUpdated;
        }

        private void OnDisable()
        {
            PlayerProgression.Instance.onStatUpdated -= OnMaxPlayerGraceStatUpdated;
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