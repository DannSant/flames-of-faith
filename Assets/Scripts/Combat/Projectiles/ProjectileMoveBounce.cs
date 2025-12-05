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
            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
                return;
            }

            direction = Vector2.Reflect(direction, col.contacts[0].normal);
            bounceCount++;
        }
    }

}