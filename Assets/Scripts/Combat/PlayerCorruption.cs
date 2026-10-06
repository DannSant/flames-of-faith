using Game.Misc;
using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Corruption gathered during the current wave. It is not persisted: at the end of every wave it is
    /// subtracted from Grace and reset to 0.
    /// Corruption comes from Corrupted Damage taken (converted through CorruptionSettings) and from the Corruptor.
    /// </summary>
    public class PlayerCorruption : MonoBehaviour
    {
        private float corruptedDamageTaken = 0f;
        private int corruptorCorruption = 0;

        private PlayerHealth playerHealth;

        public float CorruptedDamageTaken => corruptedDamageTaken;
        public int CorruptionFromDamage => CorruptionSettings.Instance.GetCorruptionFromDamage(corruptedDamageTaken);
        public int CorruptorCorruption => corruptorCorruption;
        public int CorruptionValue => CorruptionFromDamage + corruptorCorruption;

        public event Action<float> OnCorruptionChanged;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            playerHealth.onDamageTaken += HandleDamageTaken;
        }

        private void OnDisable()
        {
            playerHealth.onDamageTaken -= HandleDamageTaken;
        }

        // Damage reported here is already reduced by armor
        private void HandleDamageTaken(float damage, GameObject attacker)
        {
            if (!CorruptedDamageSource.IsCorrupted(attacker))
            {
                return;
            }

            int before = CorruptionValue;
            corruptedDamageTaken += damage;
            NotifyCorruptionChanged(before);
        }

        /// <summary>
        /// Corruption coming from the Corruptor. Updated live while its timer runs, and set to the final value when the phase ends.
        /// </summary>
        public void SetCorruptorCorruption(int amount)
        {
            int before = CorruptionValue;
            corruptorCorruption = Mathf.Max(0, amount);
            NotifyCorruptionChanged(before);
        }

        private void NotifyCorruptionChanged(int before)
        {
            int gained = CorruptionValue - before;
            if (gained > 0 && DamageNumberSpawner.Instance != null)
            {
                DamageNumberSpawner.Instance.SpawnCorruptionGainedNumber(transform.position, gained);
            }
            OnCorruptionChanged?.Invoke(CorruptionValue);
        }

        public void ResetCorruption()
        {
            corruptedDamageTaken = 0f;
            corruptorCorruption = 0;
            OnCorruptionChanged?.Invoke(CorruptionValue);
        }
    }
}
