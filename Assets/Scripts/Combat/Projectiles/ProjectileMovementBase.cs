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

        private void FixedUpdate()
        {
            Move();
        }
    }

}