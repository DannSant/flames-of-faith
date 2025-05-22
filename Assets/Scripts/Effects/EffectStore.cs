using Game.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Effects
{
    public class EffectStore : MonoBehaviour
    {

        [SerializeField] private List<Effect> startingEffects = new();

        private List<Effect> activeEffects = new();
        public event Action<Effect> OnEffectAdded;
        public event Action<Effect> OnEffectRemoved;

        private void Start()
        {
            foreach(var effect in startingEffects)
            {
                AddEffect(effect);
            }
        }

        public void AddEffect(Effect effect)
        {
            effect.Apply(this.gameObject);
            activeEffects.Add(effect);
            OnEffectAdded?.Invoke(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            activeEffects.Remove(effect);
            effect.Cleanup();
        }
        public bool HasEffect<T>() where T : Effect
        {
            return activeEffects.Any(e => e is T);
        }
        public void ClearAll()
        {
            foreach (var effect in activeEffects)
            {
                effect.Cleanup();
            }
            activeEffects.Clear();
        }
    }

}