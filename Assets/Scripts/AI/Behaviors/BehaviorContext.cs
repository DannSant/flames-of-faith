using Game.Enemies;
using System.Collections.Generic;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class BehaviorContext
    {
        public GameObject enemyGameObject;
        public Transform enemyTransform;
        public Transform playerTransform;
        public EnemyData enemyData;
        public int waveNumber;

        //public Dictionary<ScriptableObject, float> cooldownTracker = new();

        // General-purpose state container
        private Dictionary<ScriptableObject, object> behaviorState = new();

        public T GetState<T>(ScriptableObject behavior) where T : class, new()
        {
            if (behaviorState.TryGetValue(behavior, out var value))
                return value as T;

            T newState = new T();
            behaviorState[behavior] = newState;
            return newState;
        }
    }

}