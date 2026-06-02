using UnityEngine;

namespace Game.Combat.Projectiles
{
    public enum ProjectileType
    {
        Linear,
        Homing,
        Bouncing,
        Arc
    }
    public abstract class ProjectileMovementBase : MonoBehaviour
    {
        protected Rigidbody2D rb;
        protected Vector2 direction;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public virtual void Initialize(Vector2 dir)
        {
            direction = dir.normalized;
        }        

        protected abstract void Move();

        public virtual void SetTarget(Transform target)
        {
            // Optional override for projectiles that need a target reference (e.g., homing)
        }

        private void FixedUpdate()
        {
            Move();
        }
    }

}