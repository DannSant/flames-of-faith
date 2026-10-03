using Game.Common;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Combat
{
    public class PlayerGrace : MonoBehaviour, IInitializeAfterStateReady, IDependentStateLoader
    {
        [Header("Limits")]
        [SerializeField] private float minGrace = -50f;
        [SerializeField] private float maxGrace = 50f;
        public float defaultStartingGrace = 5;

        [Header("Damage")]
        [Tooltip("Damage multiplier added (or removed, when Grace is negative) per point of Grace.")]
        [SerializeField] private float damagePerGracePoint = 0.1f;
        [Tooltip("Lowest damage multiplier negative Grace can push the player to.")]
        [SerializeField] private float minDamageMultiplier = 0.1f;

        private float currentGrace = 5;
        private bool wasCorrupted;

        public delegate void OnGraceChanged(float current, float max);
        public event OnGraceChanged onGraceChanged;
        public event Action<bool> OnCorruptedStateChanged;

        public float CurrentGrace => currentGrace;
        public float MinGrace => minGrace;
        public float MaxGrace => maxGrace;

        // Grace below 0 means the player is Corrupted; how far below 0 drives the stat penalties
        public float CorruptedLevel => Mathf.Max(0f, -currentGrace);
        public bool IsCorrupted => currentGrace < 0f;

        public float GracePerWave => playerProgression != null
           ? playerProgression.GetFinalStat(StatType.GracePerWave)
           : 0f;

        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
        }

        public void InitializeAfterStateReady()
        {
            SetGrace(currentGrace);
        }

        public float GetDamageMultiplier()
        {
            return Mathf.Max(minDamageMultiplier, 1f + damagePerGracePoint * currentGrace);
        }

        public void AddGrace(float amount)
        {
            SetGrace(currentGrace + amount);
            DamageNumberSpawner.Instance.SpawnGraceGainedNumber(transform.position, amount);
        }

        public void RemoveGrace(float amount)
        {
            SetGrace(currentGrace - amount);
            DamageNumberSpawner.Instance.SpawnGraceLostNumber(transform.position, amount);
        }

        /// <summary>
        /// Resolves the end of a wave: Grace per wave is added and the Corruption gathered during the wave is subtracted.
        /// Returns the new Grace value.
        /// </summary>
        public float ApplyWaveResolution(float gracePerWave, float corruption)
        {
            SetGrace(currentGrace + gracePerWave - corruption);
            return currentGrace;
        }

        private void SetGrace(float value)
        {
            currentGrace = Mathf.Clamp(value, minGrace, maxGrace);
            onGraceChanged?.Invoke(currentGrace, maxGrace);

            if (IsCorrupted != wasCorrupted)
            {
                wasCorrupted = IsCorrupted;
                CorruptedVisual.GetOrAdd(gameObject).SetCorrupted(wasCorrupted);
                OnCorruptedStateChanged?.Invoke(wasCorrupted);
            }
        }

        public void LoadState()
        {
            SetGrace(GameSession.Instance.LoadCurrentGrace());
        }

        public void SaveState()
        {
            GameSession.Instance.SaveCurrentGrace(currentGrace);
        }

        public void ResetState()
        {
            SetGrace(defaultStartingGrace);
        }

        public bool IsAtMaxGrace() => currentGrace >= maxGrace;

    }

}
