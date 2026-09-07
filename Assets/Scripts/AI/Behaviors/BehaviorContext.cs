using Game.Enemies;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.AI.Behaviors
{
    public class BehaviorContext
    {
        public GameObject enemyGameObject;
        public Transform enemyTransform;
        public Transform playerTransform;
        public EnemyData enemyData;
        public int waveNumber;
        public EnemyAnimationController enemyAnimController;
        public bool isMoving;
        public Vector2 moveDirection;
        public bool isShooting;
        // The 8-way facing the enemy is currently committed to. Written when an attack/shot is
        // fired and held for the duration of that action, so the animator and the projectiles
        // spawned by its animation event always agree on the same direction.
        public Vector2 facingDirection;
        public float speedMultiplier = 1f;
        public NavMeshAgent navMeshAgent;
        public AITarget aiFixedTarget;
        public bool diedSilently;

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