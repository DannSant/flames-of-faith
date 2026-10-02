using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Corruption gathered during the current wave. It is not persisted: at the end of every wave it is
    /// subtracted from Grace and reset to 0.
    /// </summary>
    public class PlayerCorruption : MonoBehaviour
    {
        private float corruptionValue = 0f;

        public float CorruptionValue => corruptionValue;

        public event Action<float> OnCorruptionChanged;

        public void AddCorruption(float amount)
        {
            corruptionValue = Mathf.Max(0f, corruptionValue + amount);
            OnCorruptionChanged?.Invoke(corruptionValue);
        }

        public void ResetCorruption()
        {
            corruptionValue = 0f;
            OnCorruptionChanged?.Invoke(corruptionValue);
        }
    }
}
