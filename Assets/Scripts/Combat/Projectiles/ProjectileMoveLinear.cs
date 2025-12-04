using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMoveLinear : ProjectileMovementBase
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool rotateTowardsDirection = true;

        protected override void Move()
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

            if (rotateTowardsDirection)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
    }
}
