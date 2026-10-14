using Game.Combat.Elemental;
using Game.Combat;
using Game.Common;
using Game.Effects;
using Game.Effects.Abilities;
using Game.Effects.EffectBehaviors;
using Game.Progression;
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

    public struct EffectDebuffApplyData
    {
        public Effect effect;
        public ElementalDebuffData elementalDebuffData;
        public int count;
      

        public EffectDebuffApplyData(Effect effect, ElementalDebuffData elementalDebuffData, int count) 
        {
            this.effect = effect;
            this.elementalDebuffData = elementalDebuffData;
            this.count = count;
        }
    }

    public class EffectStore : MonoBehaviour, IDependentStateLoader
    {

        // Pushed in before every new-run reset (PlayerManager, from the class's skill tree).
        private readonly List<Effect> startingKit = new();

        private List<EffectInstance> activeEffects = new();
        public event Action<Effect> OnEffectAdded;
        public event Action<Effect> OnEffectRemoved;
        public event Action OnEffectsChanged;

        public List<EffectInstance> ActiveEffects => activeEffects;

        // Behavior assets are shared ScriptableObject instances reused across runs, so their
        // mutable runtime state (cooldowns, cached subscriptions, ...) lives here instead - see
        // EffectBehaviorContext.
        private EffectBehaviorContext context;

        private void Awake()
        {
            context = new EffectBehaviorContext { ownerObject = gameObject, storeOwner = this };
        }

        private void ResetEffects()
        {
            ClearAll();

            // Fresh context for the new run so no behavior's runtime state (a cooldown timer, a
            // stale cached reference) survives from the previous one.
            context = new EffectBehaviorContext { ownerObject = gameObject, storeOwner = this };

            foreach (var effect in startingKit)
            {
                if (effect != null) AddEffect(effect);
            }
        }

        /// <summary>The items a new run starts with. Applied on the next ResetState.</summary>
        public void SetStartingKit(IEnumerable<Effect> kit)
        {
            startingKit.Clear();
            if (kit != null) startingKit.AddRange(kit);
        }

        public void AddEffect(Effect effect)
        {
            // Check if the effect already exists
            var existingIndex = activeEffects.FindIndex(ei => ei.effect.EffectID == effect.EffectID);


            if (existingIndex >= 0)
            {
                var instance = activeEffects[existingIndex];
                instance.count++;
                activeEffects[existingIndex] = instance;


                foreach (var behavior in effect.Behaviors)
                {
                    behavior.OnTrigger(EffectTrigger.OnStack);
                }

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
                behavior.Initialize(context, effect);
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

        /// <summary>
        /// The first owned ability of type <typeparamref name="T"/>, with the tier of the item
        /// carrying it. Reads the tier from the item itself, never from the behavior asset, which is
        /// shared by every tier of the item.
        /// </summary>
        public bool TryGetAbility<T>(out T ability, out int tier) where T : AbilityBehavior
        {
            foreach (var instance in activeEffects)
            {
                if (instance.effect == null) continue;
                foreach (var behavior in instance.effect.Behaviors)
                {
                    if (behavior is T match)
                    {
                        ability = match;
                        tier = instance.effect.Tier;
                        return true;
                    }
                }
            }

            ability = null;
            tier = 0;
            return false;
        }

        /// <summary>The item carrying the ability for <paramref name="slot"/>, if any (for icons and tooltips).</summary>
        public bool TryGetAbilityItem(AbilitySlot slot, out Effect item)
        {
            foreach (var instance in activeEffects)
            {
                if (instance.effect == null) continue;
                if (instance.effect.Behaviors.Any(b => b is AbilityBehavior a && a.Slot == slot))
                {
                    item = instance.effect;
                    return true;
                }
            }

            item = null;
            return false;
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

        public List<EffectDebuffApplyData> GetElementalTypesToApply(DamageOriginType origin, WeaponClass weaponClass)
        {
            List<EffectDebuffApplyData> result = new();

            foreach (var inst in activeEffects)
            {
                foreach (var behavior in inst.effect.Behaviors)
                {
                    if (behavior is ElementalInfusionEffectBehavior infusion)
                    {
                        var meta = new ElementalMetadata
                        {
                            origin = origin,
                            weaponClass = weaponClass
                        };

                        if (infusion.ShouldApply(meta))
                            result.Add(new EffectDebuffApplyData(inst.effect,infusion.elementalDebuffData,inst.count));
                    }
                }
            }

            return result;
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

        public IEnumerable<EffectInstance> GetEffectInstancesGrantingStat(StatType statType)
        {
            return activeEffects
                .Where(ei => ei.effect.StatModifiers.Any(m => m.stat == statType)
                    || (ei.effect.HasStatDependencies && ei.effect.StatDependencies.Any(r => r != null && r.targetStat == statType)));
        }

        /// <summary>Every stat dependency rule of the active effects, with how many times each item is owned.</summary>
        public List<(StatDependencyRule rule, int count)> GetAllStatDependencies()
        {
            var list = new List<(StatDependencyRule rule, int count)>();
            foreach (var ei in activeEffects)
            {
                if (!ei.effect.HasStatDependencies) continue;
                foreach (var rule in ei.effect.StatDependencies)
                {
                    if (rule != null) list.Add((rule, ei.count));
                }
            }
            return list;
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