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
        public event Action OnEffectsChanged;

        public List<EffectInstance> ActiveEffects => activeEffects;

        private void ResetEffects()
        {
            ClearAll();          
            foreach (var effect in startingEffects)
            {
                AddEffect(effect);
            }
        }

        /*
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
        }*/

        public void AddEffect(Effect effect)
        {
            // Check if the effect already exists (by reference or by EffectID if you prefer)
            var existingIndex = activeEffects.FindIndex(ei => ei.effect == effect);

            if (existingIndex >= 0)
            {
                var instance = activeEffects[existingIndex];
                instance.count++;
                activeEffects[existingIndex] = instance;

                // ⚠ Counts changing affects stat modifiers
                OnEffectsChanged?.Invoke();
                return;
            }

            // Create new instance
            var newInstance = new EffectInstance(effect);
            activeEffects.Add(newInstance);

            // 1) Initialize behaviors
            foreach (var behavior in effect.Behaviors)
            {
                if (behavior == null) continue;
                behavior.Initialize(gameObject, this, effect);
                behavior.OnTrigger(EffectTrigger.OnApply);
            }

            // 2) Call legacy hook (for old effects)
            effect.Apply(gameObject, this);

            OnEffectAdded?.Invoke(effect);
            OnEffectsChanged?.Invoke();

        }

        public void RemoveEffect(Effect effect)
        {
            int index = activeEffects.FindIndex(ei => ei.effect == effect);
            if (index < 0) return;

            var instance = activeEffects[index];

            // You can decide whether to decrement count or remove fully.
            activeEffects.RemoveAt(index);

            // Cleanup behaviors
            foreach (var behavior in effect.Behaviors)
            {
                if (behavior == null) continue;
                behavior.OnTrigger(EffectTrigger.OnRemove);
                behavior.Cleanup();
            }

            // Legacy cleanup
            effect.Cleanup();

            OnEffectRemoved?.Invoke(effect);
            OnEffectsChanged?.Invoke();
        }

        public bool HasEffect<T>() where T : Effect
        {
            return activeEffects.Any(ei => ei.effect is T);
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
                // Behaviors cleanup
                foreach (var behavior in effectInstance.effect.Behaviors)
                {
                    if (behavior == null) continue;
                    behavior.OnTrigger(EffectTrigger.OnRemove);
                    behavior.Cleanup();
                }

                // Legacy
                effectInstance.effect.Cleanup();
            }

            activeEffects.Clear();
        }

        /// <summary>
        /// Called by other systems (combat, damage, etc.) to notify effects.
        /// Example: call Trigger(EffectTrigger.OnAttack) from your attack code.
        /// </summary>
        public void Trigger(EffectTrigger trigger)
        {
            foreach (var instance in activeEffects)
            {
                var effect = instance.effect;
                if (effect == null) continue;

                foreach (var behavior in effect.Behaviors)
                {
                    if (behavior == null) continue;
                    behavior.OnTrigger(trigger);
                }
            }
        }

        public List<StatModifier> GetAllStatModifiers()
        {
            List<StatModifier> list = new List<StatModifier>();

            foreach (var ei in activeEffects)
            {
                foreach (var mod in ei.effect.StatModifiers)
                {
                    int count = ei.count;
                    for (int i = 0; i < count; i++)
                        list.Add(mod);
                }
            }

            return list;
        }

        public void LoadState()
        {
            var effects = GameSession.Instance.LoadEffectStore();
            foreach (var effect in effects)
            {
                for (int i = 0; i < effect.count; i++)
                {
                    AddEffect(effect.effect);
                }
            }
            OnEffectsChanged?.Invoke();
        }

        public void SaveState()
        {
           GameSession.Instance.SaveEffectStore(activeEffects);
        }

        public void ResetState()
        {
            ResetEffects();
            OnEffectsChanged?.Invoke();
        }
    }

}