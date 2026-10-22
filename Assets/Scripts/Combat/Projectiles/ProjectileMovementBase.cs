using UnityEngine;

namespace Game.Combat.Projectiles
{
    public enum ProjectileType
    {
        Linear,
        Homing,
        Bouncing,
        Arc
    }
    public abstract class ProjectileMovementBase : MonoBehaviour
    {
        [Tooltip("Units per second. A player projectile takes its speed from its DamageSourceData " +
            "(Resources/Combat/DamageSources) instead; this value is used only without one.")]
        [SerializeField] protected float speed = 5f;

        protected Rigidbody2D rb;
        protected Vector2 direction;

        private Vector2 startPosition;
        private float maxTravelDistance;

        public float Speed { get => speed; set => speed = value; }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (TryGetComponent(out DamageSourceBase source) && source.Data != null && source.Data.ProjectileSpeed > 0f)
            {
                speed = source.Data.ProjectileSpeed;
            }
        }

        public virtual void Initialize(Vector2 dir)
        {
            direction = dir.normalized;
        }

        protected abstract void Move();

        public virtual void SetTarget(Transform target)
        {
            // Optional override for projectiles that need a target reference (e.g., homing)
        }

        /// <summary>
        /// Destroys the projectile once it is this far from where the cap was set. 0 (the default)
        /// means no cap, so only callers that opt in are affected. Used by free-aim primary shots
        /// so a shot fired without a target can't reach further than auto-targeting would.
        /// A distance rather than a lifetime, so it holds even if the speed is scaled after spawn.
        /// </summary>
        public void SetMaxTravelDistance(float distance)
        {
            startPosition = transform.position;
            maxTravelDistance = Mathf.Max(0f, distance);
        }

        private void FixedUpdate()
        {
            Move();

            if (maxTravelDistance > 0f && Vector2.Distance(startPosition, rb.position) >= maxTravelDistance)
            {
                Destroy(gameObject);
            }
        }
    }

}