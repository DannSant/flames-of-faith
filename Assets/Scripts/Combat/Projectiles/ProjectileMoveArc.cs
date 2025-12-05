using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMoveArc : ProjectileMovementBase
    {
        [SerializeField] private float gravityScale = 0.5f;
        [SerializeField] private float initialSpeed = 6f;

        protected override void Awake()
        {
            base.Awake();
            rb.gravityScale = gravityScale;
        }

        public override void Initialize(Vector2 dir)
        {
            base.Initialize(dir);
            rb.linearVelocity = dir.normalized * initialSpeed;
        }

        protected override void Move()
        {
            // Rigidbody2D handles the physics
        }
    }
}
