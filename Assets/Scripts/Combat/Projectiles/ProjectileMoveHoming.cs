using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMoveHoming : ProjectileMovementBase
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float turnSpeed = 180f;

        private Transform target;

        public void SetTarget(Transform t)
        {
            target = t;
        }

        protected override void Move()
        {
            if (target == null)
            {
                rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
                return;
            }

            Vector2 toTarget = (Vector2)target.position - rb.position;
            Vector2 desired = toTarget.normalized;

            // Smooth turning
            direction = Vector3.RotateTowards(
                direction,
                desired,
                turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f
            );

            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
    }
}
