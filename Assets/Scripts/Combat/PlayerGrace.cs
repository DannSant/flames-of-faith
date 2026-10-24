using Game.Common;
using Game.Misc;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// What the current Grace does to the player, computed by gameplay so the UI never re-implements the formulas.
    /// </summary>
    public struct GraceStatus
    {
        public float grace;
        public bool isCorrupted;
        // Damage change in percent: positive with Grace above 0, negative while Corrupted
        public float damagePercent;
        // Flat reductions while Corrupted (0 otherwise)
        public float armorPenalty;
        public float healingPenalty;
    }

    public class PlayerGrace : MonoBehaviour, IInitializeAfterStateReady, IDependentStateLoader
    {
        // The range, starting Grace and damage per point are on CorruptionSettings

        private float currentGrace = 5;
        private bool wasCorrupted;

        public delegate void OnGraceChanged(float current, float max);
        public event OnGraceChanged onGraceChanged;
        public event Action<bool> OnCorruptedStateChanged;
        public event Action<GraceStatus> OnGraceStatusChanged;

        public float CurrentGrace => currentGrace;
        public float MinGrace => CorruptionSettings.Instance.MinGrace;
        public float MaxGrace => CorruptionSettings.Instance.MaxGrace;

        // Grace below 0 means the player is Corrupted; how far below 0 drives the stat penalties
        public float CorruptedLevel => Mathf.Max(0f, -currentGrace);
        public bool IsCorrupted => currentGrace < 0f;

        // Grace Affinity: Grace gained at the end of every wave (also raises the Grace pickup drop chance)
        public float GraceAffinity => playerProgression != null
           ? playerProgression.GetFinalStat(StatType.GraceAffinity)
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
            return CorruptionSettings.Instance.GetGraceDamageMultiplier(currentGrace);
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
        /// Resolves the end of a wave: Grace Affinity is added and the Corruption gathered during the wave is subtracted.
        /// Returns the new Grace value.
        /// </summary>
        public float ApplyWaveResolution(float graceAffinity, float corruption)
        {
            float before = currentGrace;
            SetGrace(currentGrace + graceAffinity - corruption);

            float change = currentGrace - before;
            if (change > 0f)
            {
                DamageNumberSpawner.Instance.SpawnGraceGainedNumber(transform.position, change);
            }
            else if (change < 0f)
            {
                DamageNumberSpawner.Instance.SpawnGraceLostNumber(transform.position, -change);
            }
            return currentGrace;
        }

        private void SetGrace(float value)
        {
            currentGrace = Mathf.Clamp(value, MinGrace, MaxGrace);
            onGraceChanged?.Invoke(currentGrace, MaxGrace);

            if (IsCorrupted != wasCorrupted)
            {
                wasCorrupted = IsCorrupted;
                CorruptedVisual.GetOrAdd(gameObject).SetCorrupted(wasCorrupted);
                OnCorruptedStateChanged?.Invoke(wasCorrupted);
            }

            if (OnGraceStatusChanged != null)
            {
                OnGraceStatusChanged.Invoke(GetStatus());
            }
        }

        public GraceStatus GetStatus()
        {
            return new GraceStatus
            {
                grace = currentGrace,
                isCorrupted = IsCorrupted,
                damagePercent = (GetDamageMultiplier() - 1f) * 100f,
                armorPenalty = playerProgression != null ? playerProgression.GetCorruptionPenalty(StatType.Armor) : 0f,
                healingPenalty = playerProgression != null ? playerProgression.GetCorruptionPenalty(StatType.HealingReceived) : 0f
            };
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
            SetGrace(CorruptionSettings.Instance.StartingGrace);
        }

        public bool IsAtMaxGrace() => currentGrace >= MaxGrace;

    }

}
