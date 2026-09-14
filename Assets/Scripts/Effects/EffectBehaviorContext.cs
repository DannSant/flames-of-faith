using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// Runtime state for a single player's active effects, owned by <see cref="EffectStore"/>.
    /// <see cref="EffectBehavior"/> assets are shared ScriptableObject instances (loaded once via
    /// Resources and reused across runs), so any mutable, time-varying state they need - cooldown
    /// timers, cached subscriptions, spawned-object lists - must live here instead of on the
    /// behavior itself, or it would leak between runs and (in a multi-owner scenario) between
    /// stores. Mirrors <see cref="Game.AI.Behaviors.BehaviorContext"/>'s GetState pattern.
    /// </summary>
    public class EffectBehaviorContext
    {
        public GameObject ownerObject;
        public EffectStore storeOwner;

        private readonly Dictionary<EffectBehavior, object> behaviorState = new();

        public T GetState<T>(EffectBehavior behavior) where T : class, new()
        {
            if (behaviorState.TryGetValue(behavior, out var value))
                return value as T;

            T newState = new T();
            behaviorState[behavior] = newState;
            return newState;
        }
    }
}
