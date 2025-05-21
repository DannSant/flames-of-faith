using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class LinearProjectile : ProjectileBase
    {
        protected override void MoveBehavior()
        {
            Vector2 direction =  moveDirection;

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

}