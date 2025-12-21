using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMoveBounce : ProjectileMovementBase
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private int maxBounces = 3;

        private int bounceCount = 0;

        protected override void Move()
        {
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            Bounce(col.contacts[0].normal);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Ignore triggers that should not bounce (optional layer check)
            if (collision.isTrigger)
                return;

            Vector2 normal = CalculateTriggerNormal(collision);
            Bounce(normal);
        }

        private void Bounce(Vector2 normal)
        {
            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
                return;
            }

            direction = Vector2.Reflect(direction, normal).normalized;
            bounceCount++;
        }

        private Vector2 CalculateTriggerNormal(Collider2D collider)
        {
            // Closest point on the collider to the projectile
            Vector2 closestPoint = collider.ClosestPoint(rb.position);

            // Direction from contact point to projectile
            Vector2 normal = (rb.position - closestPoint).normalized;

            // Fallback safety (rare edge case)
            if (normal == Vector2.zero)
                normal = -direction;

            return normal;
        }
    }

}