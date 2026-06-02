using UnityEngine;

namespace Game.Combat.Projectiles
{
    public class ProjectileMovementDelayedHoming : ProjectileMovementBase
    {
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float retargetInterval = 2f;
        [SerializeField] private float turnSpeed = 45f;
        [SerializeField] private float offsetRadius = 1f;

        //State
        private Transform target;      
        private float retargetTimer;
        private Vector2 targetOffset;
        private Vector2 desiredDirection;

        public override void Initialize(Vector2 dir)
        {
            base.Initialize(dir);           
            retargetTimer = retargetInterval;
            targetOffset = Random.insideUnitCircle * offsetRadius;


        }

        public override void SetTarget(Transform target)
        {
            this.target = target;
            UpdateDesiredDirection();
        }

        protected override void Move()
        {
            retargetTimer -= Time.fixedDeltaTime;

            if (retargetTimer <= 0f)
            {
                UpdateDesiredDirection();
                retargetTimer = retargetInterval;
            }

            direction = Vector3.RotateTowards(
                direction,
                desiredDirection,
                turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f
            );

            rb.MovePosition(
                rb.position +
                direction * speed * Time.fixedDeltaTime
            );
        }

        private void UpdateDesiredDirection()
        {
            if (target == null)
                return;

            Vector2 aimPoint =  (Vector2)target.position + targetOffset;

            desiredDirection = (aimPoint - rb.position).normalized;
        }
    }

}