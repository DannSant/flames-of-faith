using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMoveHoming : ProjectileMovementBase
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float turnSpeed = 180f;
        [Tooltip("Rotate the projectile to face where it's flying (arrows). Off for round projectiles.")]
        [SerializeField] private bool rotateTowardsDirection = false;

        private Transform target;

        public override void SetTarget(Transform t)
        {
            target = t;
        }

        protected override void Move()
        {
            if (target == null)
            {
                rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
                FaceDirection();
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
            FaceDirection();
        }

        private void FaceDirection()
        {
            if (!rotateTowardsDirection) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
