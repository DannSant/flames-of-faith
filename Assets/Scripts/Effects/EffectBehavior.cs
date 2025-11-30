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
        OnSecondaryAction // e.g., a special ability
    }
    public abstract class EffectBehavior : ScriptableObject
    {
        // References
        protected GameObject ownerObject;
        protected EffectStore storeOwner;
        protected Effect parentEffect;
        /// <summary> Called once when the effect is added to the owner. </summary>
        public virtual void Initialize(GameObject owner, EffectStore store, Effect effect) {
            ownerObject = owner;
            storeOwner = store;
            parentEffect = effect;
        }

        /// <summary> Called when a trigger happens (attack, hit, etc). </summary>
        public virtual void OnTrigger(EffectTrigger trigger) { }

        /// <summary> Called once when the effect is removed/cleared. </summary>
        public virtual void Cleanup() { }
    }

}