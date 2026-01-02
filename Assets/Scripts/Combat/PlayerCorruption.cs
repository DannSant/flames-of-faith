using Game.Common;
using Game.Scene;
using Game.Utils;
using UnityEngine;

namespace Game.Combat
{
    public class PlayerCorruption : MonoBehaviour, IInitializeAfterStateReady, IPrimaryStateLoader
    {
        private float corruptionValue = 0f;

        public float CorruptionValue => corruptionValue;

        private PlayerGrace playerGrace;
        public event System.Action<float, float, float> OnCorruptionChanged;

        private void Awake()
        {
            playerGrace = GetComponent<PlayerGrace>();
        }

        public void AddCorruption(float amount)
        {
            corruptionValue += amount;
            OnCorruptionChanged?.Invoke(corruptionValue, CalculateCorruptionEffectLevel(), playerGrace.MaxGrace);         
          
        }

        /// <summary>
        /// Calculates the total corruption level based on the current grace and corruption value.
        /// This is calculated by subtracting the current grace from the corruption value,
        /// </summary>
        public float CalculateCorruptionEffectLevel()
        {
            float currentGrace = playerGrace.CurrentGrace;
            float effectiveCorruption = corruptionValue - currentGrace;
            //Debug.Log($"Calculating Corruption Effect Level: CorruptionValue={corruptionValue}, CurrentGrace={currentGrace}, EffectiveCorruption={effectiveCorruption}");
            return Mathf.Max(0f, effectiveCorruption);
        }

        public void ReduceCorruption(float amount)
        {
            corruptionValue -= amount;
            if (corruptionValue < 0f)
            {
                corruptionValue = 0f;
            }
            OnCorruptionChanged?.Invoke(corruptionValue, CalculateCorruptionEffectLevel(), playerGrace.MaxGrace);
        }

        public void InitializeAfterStateReady()
        {
            
        }

        public void LoadState()
        {
            float loadedCorruption = GameSession.Instance.LoadCorruptionLevel();
            AddCorruption(loadedCorruption); 
        }

        public void ResetState()
        {
            corruptionValue = 0f;
        }

        public void SaveState()
        {
           GameSession.Instance.SaveCorruptionLevel(corruptionValue);
        }
    }
}
