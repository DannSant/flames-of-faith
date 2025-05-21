using Game.Scene;
using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class HomingProjectile : ProjectileBase
    {
        private Transform target;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        protected override void MoveBehavior()
        {
            if (target == null)
            {
                transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
                return;
            }

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public override void ConfigureAfterSpawn()
        {
            base.ConfigureAfterSpawn();
            Transform currentTarget = PlayerManager.Instance.GetPlayerComponent<WeaponManager>().GetCurrentTarget().transform;
            if (currentTarget != null)
            {
                SetTarget(currentTarget);
            }
        }
    }

}