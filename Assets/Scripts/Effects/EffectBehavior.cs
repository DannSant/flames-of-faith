using System;
using UnityEngine;

namespace Game.Effects
{
    public enum EffectTrigger
    {
        OnApply,        // when the effect is first added
        OnRemove,       // when the effect is removed
        OnAttack,       // player attacked
        OnHit,          // player hit an enemy
        OnKill,         // player killed an enemy
        OnDamageTaken,  // player took damage
        OnWaveStart,
        OnWaveEnd,
        OnUpdate,       // per-frame, if you need it
        OnSpecialAttack, // e.g., a special ability
        OnStack // when the effect is stacked/increased
    }
    public enum EffectStackBehavior { 
        None,
        AddBounces,
        AddDamage,
        AddSpawnCount
    }
    public abstract class EffectBehavior : ScriptableObject
    {
        [SerializeField] protected string behaviorId;
        [SerializeField] protected EffectStackBehavior stackBehavior = EffectStackBehavior.None;

        // Shared, per-owner state - see EffectBehaviorContext for why this can't be a field on
        // this ScriptableObject instead.
        protected EffectBehaviorContext context;
        protected Effect parentEffect;

        // Convenience accessors so existing/derived behaviors can keep reading these as before.
        protected GameObject ownerObject => context?.ownerObject;
        protected EffectStore storeOwner => context?.storeOwner;

        public string BehaviorId { get => behaviorId; set => behaviorId = value; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only assign a new ID if it's empty
            if (string.IsNullOrEmpty(behaviorId))
            {
                behaviorId = name;
                UnityEditor.EditorUtility.SetDirty(this); // Mark asset as modified so Unity saves it
            }
        }
#endif

        /// <summary> Called once when the effect is added to the owner. </summary>
        public virtual void Initialize(EffectBehaviorContext context, Effect effect) {
            this.context = context;
            parentEffect = effect;
        }

        /// <summary>
        /// Per-behavior mutable runtime state (cooldown timers, cached subscriptions, etc.),
        /// scoped to this behavior's owning EffectBehaviorContext so it resets with the run
        /// instead of persisting on the shared ScriptableObject.
        /// </summary>
        protected T GetState<T>() where T : class, new() => context.GetState<T>(this);

        /// <summary> Called when a trigger happens (attack, hit, etc). </summary>
        public virtual void OnTrigger(EffectTrigger trigger) { }

        /// <summary> Called once when the effect is removed/cleared. </summary>
        public virtual void Cleanup() { }
    }

}