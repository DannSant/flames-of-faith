using Game.Effects;
using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMoveLinear : ProjectileMovementBase, IStackScalable
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool rotateTowardsDirection = true;

        [Header("Course Correction")]
        [Tooltip("Gently turn toward the target given by SetTarget. Projectiles fired without a target always fly straight.")]
        [SerializeField] private bool steerTowardsTarget = false;
        [Tooltip("Max degrees per second the projectile turns toward its target. Low values give a slight curve, not homing.")]
        [SerializeField] private float steerTurnRate = 120f;
        [Tooltip("Steering stops for good once the target is more than this many degrees off the flight direction (missed or passed it), so the projectile never turns back.")]
        [Range(0f, 180f)]
        [SerializeField] private float steerMaxAngleToTarget = 90f;

        private Transform target;
        private EnemyHealth targetHealth;
        private bool steering;

        public void ApplyStackScaling(StackScaling scaling)
        {
            speed *= scaling.Get(StackScalingTarget.Speed);
        }

        public override void SetTarget(Transform t)
        {
            target = t;
            targetHealth = t != null ? t.GetComponent<EnemyHealth>() : null;
            steering = steerTowardsTarget && t != null;
        }

        protected override void Move()
        {
            if (steering) Steer();

            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

            if (rotateTowardsDirection)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void Steer()
        {
            // Unity's null: also true once the target has been destroyed.
            if (target == null || (targetHealth != null && targetHealth.IsDead()))
            {
                steering = false;
                return;
            }

            Vector2 toTarget = (Vector2)target.position - rb.position;
            if (!ProjectileSteering.IsWithinSteeringCone(direction, toTarget, steerMaxAngleToTarget))
            {
                steering = false;
                return;
            }

            direction = ProjectileSteering.Steer(direction, toTarget, steerTurnRate * Time.fixedDeltaTime);
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
    }
}
