using Game.Common;
using Game.Effects;
using Game.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Effects
{
    [System.Serializable]
    public struct EffectInstance
    {
        public Effect effect;
        public int count;

        public EffectInstance(Effect effect, int count = 1)
        {
            this.effect = effect;
            this.count = count;
        }
    }

    public class EffectStore : MonoBehaviour, IDependentStateLoader
    {

        [SerializeField] private List<Effect> startingEffects = new();

        private List<EffectInstance> activeEffects = new();
        public event Action<Effect> OnEffectAdded;
        public event Action<Effect> OnEffectRemoved;

        public List<EffectInstance> ActiveEffects => activeEffects;

        private void ResetEffects()
        {
            ClearAll();
            foreach (var effect in startingEffects)
            {
                AddEffect(effect);
            }
        }

        public void AddEffect(Effect effect)
        {
            var existing = activeEffects.FirstOrDefault(e => e.effect.EffectID == effect.EffectID);

            if (existing.effect != null)
            {
                // Increase the count for matching effect ID
                int index = activeEffects.FindIndex(e => e.effect.EffectID == effect.EffectID);
                existing.count++;
                activeEffects[index] = existing;
                effect.UpdateEffect(this.gameObject);
            }
            else
            {
                // New effect, apply and track it
                effect.Apply(this.gameObject, this);
                activeEffects.Add(new EffectInstance(effect));
                OnEffectAdded?.Invoke(effect);
            }
        }

        public void RemoveEffect(Effect effect)
        {
            var instance = activeEffects.FirstOrDefault(ei => ei.effect.EffectID == effect.EffectID);
            if (instance.effect != null)
            {
                activeEffects.Remove(instance);
                instance.effect.Cleanup();
                OnEffectRemoved?.Invoke(effect);
            }
        }
        public bool HasEffectByID(string effectID)
        {
            return activeEffects.Any(ei => ei.effect.EffectID == effectID);
        }

        public EffectMultiplierConfig GetEffectMultiplierConfig(string effectID)
        {          
            var effectInstance = activeEffects.FirstOrDefault(ei => ei.effect.EffectID == effectID);
            if (effectInstance.effect != null)
            {
                return new EffectMultiplierConfig
                {
                    count = effectInstance.count,
                    scaleValue = effectInstance.effect.scalingValue
                };
            }

            return new EffectMultiplierConfig();
        }
        public void ClearAll()
        {
            foreach (var effectInstance in activeEffects)
            {
                effectInstance.effect.Cleanup();
            }
            activeEffects.Clear();
        }

        public void LoadState()
        {
            var effects = GameSession.Instance.LoadEffectStore();
            foreach(var effect in effects)
            {
                for(int i = 0; i < effect.count; i++)
                {
                    AddEffect(effect.effect);
                }
            }
        }

        public void SaveState()
        {
           GameSession.Instance.SaveEffectStore(activeEffects);
        }

        public void ResetState()
        {
            ResetEffects();
        }
    }

}