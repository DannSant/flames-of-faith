using UnityEngine;

namespace Game.AI {
    public abstract class EnemyAIBase : MonoBehaviour
    {
        protected Transform player;

        public abstract void SetProjectileDamage(int amount);

        public virtual void Initialize(Transform playerTransform)
        {
            player = playerTransform;
        }

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
    }

}