using System;
using System.Collections.Generic;
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
    public abstract class EffectBehavior : ScriptableObject
    {
        [SerializeField] protected string behaviorId;

        [Tooltip("How extra copies of this relic scale it. Empty = stacking does nothing.")]
        [SerializeField] private List<StackScalingRule> stackScaling = new();

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

            WarnOnQuadraticScaling();
        }

        /// <summary>
        /// Scaling both how many objects spawn and how hard each one hits makes total output grow
        /// with the square of the stack count. Legitimate if deliberate, so this warns rather than
        /// blocks - but it's almost always an authoring slip.
        /// </summary>
        private void WarnOnQuadraticScaling()
        {
            bool scalesSpawnCount = false;
            bool scalesDamage = false;

            foreach (var rule in stackScaling)
            {
                if (rule == null) continue;
                scalesSpawnCount |= rule.Target == StackScalingTarget.SpawnCount;
                scalesDamage |= rule.Target == StackScalingTarget.Damage;
            }

            if (scalesSpawnCount && scalesDamage)
            {
                Debug.LogWarning(
                    $"{name}: scales both SpawnCount and Damage, so stacking it scales total damage " +
                    "quadratically. Intentional on a capstone relic, a mistake everywhere else.", this);
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

        /// <summary> How many copies of the parent effect the player currently holds (never below 1). </summary>
        protected int StackCount => storeOwner != null && parentEffect != null
            ? Mathf.Max(1, storeOwner.GetEffectMultiplierConfig(parentEffect.EffectID).count)
            : 1;

        protected StackScaling GetStackScaling() => new StackScaling(stackScaling, StackCount);

        /// <summary>
        /// Scales an authored spawn count by the SpawnCount rule, if one is authored. This can't
        /// travel through <see cref="IStackScalable"/> like the other targets, because it's a
        /// property of the spawning loop rather than of any one spawned object.
        /// </summary>
        protected int ResolveSpawnCount(int authoredCount)
        {
            float multiplier = GetStackScaling().Get(StackScalingTarget.SpawnCount);
            return Mathf.Max(1, Mathf.RoundToInt(authoredCount * multiplier));
        }

        /// <summary>
        /// The one way a behavior should spawn anything: instantiates the prefab, tags every
        /// damage source on it with the parent effect's id, and pushes the resolved stack scaling
        /// into every component that responds to it.
        /// </summary>
        protected GameObject SpawnEffectObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{name}: no prefab to spawn.");
                return null;
            }

            GameObject instance = parent != null
                ? Instantiate(prefab, position, rotation, parent)
                : Instantiate(prefab, position, rotation);

            string effectID = parentEffect != null ? parentEffect.EffectID : null;
            StackScaling scaling = GetStackScaling();

            // Children included: a prefab's damage source or movement often sits on a child, and
            // the old GetComponent lookups silently skipped those.
            foreach (var multiplier in instance.GetComponentsInChildren<IEffectMultiplier>(true))
            {
                multiplier.SetEffectID(effectID);
            }

            foreach (var scalable in instance.GetComponentsInChildren<IStackScalable>(true))
            {
                scalable.ApplyStackScaling(scaling);
            }

            return instance;
        }

        /// <summary> Called when a trigger happens (attack, hit, etc). </summary>
        public virtual void OnTrigger(EffectTrigger trigger) { }

        /// <summary> Called once when the effect is removed/cleared. </summary>
        public virtual void Cleanup() { }
    }

}