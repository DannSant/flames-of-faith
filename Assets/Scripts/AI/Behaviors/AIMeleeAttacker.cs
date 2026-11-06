using Game.Combat;
using Game.Enemies;
using Game.Misc;
using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class MeleeAttackerBehaviorState
    {
        public float attackTimer = 0f;
        public bool isAttacking = false;
        public Collider2D meleeCollider;
        // Toward the player when the swing started: the warning and the hitbox both use it
        public Vector2 lockedDirection = Vector2.down;
        public AttackTelegraph telegraph;
        // The authored circle the ground area is built from: its radius and offset, in the hitbox's local space
        public float areaRadius = 0.5f;
        public Vector2 areaOffset;
    }
    [CreateAssetMenu(menuName = "Behaviors/On Update/MeleeAttacker")]
    public class AIMeleeAttacker : AIUpdateBehavior, IAnimationEventReceiver
    {
        [Tooltip("The enemy attacks only while the player is closer than this.")]
        [SerializeField] private float maxRange = 7f;
        [Tooltip("Seconds between attacks.")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private string colliderObjectName = "MeleeCollider";
        [Tooltip("Distance on the ground from the enemy's center at which the melee hitbox is placed, toward the player " +
            "(shortened vertically by the ground ratio, like the area itself).")]
        [SerializeField] private float orbitRadius = 0.5f;
        [Tooltip("Seconds from the start of the attack animation to its hit frame (its attack state's " +
            "MeleeAnimationAttackBehavior Attack Delay x the clip length). The warning fills over this time, so it's full when the hit lands.")]
        [Min(0.01f)]
        [SerializeField] private float animationHitTime = 0.6666667f;

        public float AnimationHitTime => animationHitTime;


        public override void Tick(BehaviorContext context)
        {
            if (context.enemyTransform == null || context.playerTransform == null) return;
            var rb = context.enemyTransform.GetComponent<Rigidbody2D>();
            var colliderTransform = context.enemyTransform.Find(colliderObjectName);
            if (colliderTransform == null) return;

            var colliderObj = colliderTransform.gameObject;

            if (rb == null || colliderObj == null) return;

            float distance = Vector2.Distance(rb.position, context.playerTransform.position);

            var state = context.GetState<MeleeAttackerBehaviorState>(this);

            if (state.telegraph == null && AttackTelegraphSettings.Instance.ShowTelegraphs)
            {
                state.telegraph = AttackTelegraph.Create(context.enemyTransform, AttackTelegraphSettings.Instance);
            }

            // Initialize melee collider if not already set
            if (state.meleeCollider == null)
            {
                state.meleeCollider = CreateGroundHitbox(colliderObj.GetComponent<Collider2D>(), GroundRatio, state);
                if (state.meleeCollider != null) {
                    state.meleeCollider.enabled = false;
                    if (colliderObj.TryGetComponent(out EnemyDamage damage))
                    {
                        float damageAmount = GetDamageAmount(context);
                        damage.SetDamageAmount(damageAmount);
                    }
                }

            }

            if (state.isAttacking) return;
            if (distance <= maxRange)
            {
                if (state.attackTimer <= 0f)
                {
                    Attack(context, state);
                }
                else
                {
                    state.attackTimer -= Time.deltaTime;
                }
            }
        }

        public override void OnStunStateChanged(BehaviorContext context, bool isStunned)
        {
            if (!isStunned) return;

            // The frozen animator will never deliver the end event that normally disables the
            // hitbox, so an enemy stunned mid-swing would keep a live melee collider for the whole
            // stun. isAttacking is deliberately left set: when the stun ends the animation resumes
            // and its own end event clears it, so the interrupted swing finishes rather than
            // restarting from the top.
            var state = context.GetState<MeleeAttackerBehaviorState>(this);
            if (state.meleeCollider != null)
            {
                state.meleeCollider.enabled = false;
            }
            // A frozen enemy isn't about to hit: drop the warning and the held position (like the shooters)
            HideTelegraph(state);
            context.isShooting = false;
            SetKnockbackImmune(context, false);
        }

        private void Attack(BehaviorContext context, MeleeAttackerBehaviorState state)
        {
            state.isAttacking = true;
            state.attackTimer = attackCooldown;

            // The direction is chosen when the swing starts, so the warning shows where the hit really lands
            Vector2 toPlayer = (Vector2)(context.playerTransform.position - context.enemyTransform.position);
            if (toPlayer.sqrMagnitude > 0.0001f) state.lockedDirection = toPlayer.normalized;

            // Hold still and keep facing the target while winding up (isShooting = "an attack is in progress")
            context.isShooting = true;
            // ...and can't be knocked off the spot the warning shows (tester feedback, v0.5.2). Released with isShooting.
            SetKnockbackImmune(context, true);

            if (state.telegraph != null && state.meleeCollider != null)
            {
                var (center, radius) = HitArea(state, context.enemyTransform.position);
                state.telegraph.Show(center, radius, animationHitTime);
            }

            if (context.enemyAnimController != null)
            {
                context.enemyAnimController.PlayAttack();
            }
        }

        // Segments of the ground hitbox's outline: enough that it can't be told apart from the drawn ellipse
        public const int HitboxSegments = 48;

        private static float GroundRatio => AttackTelegraphSettings.Instance.GroundRatio;

        /// <summary>
        /// Where the hitbox sits relative to the enemy: <paramref name="orbitRadius"/> along the locked direction,
        /// measured on the ground (<see cref="IsoGroundArea.GroundOffset"/>).
        /// </summary>
        public static Vector2 HitboxOffset(Vector2 direction, float orbitRadius, float groundRatio) =>
            IsoGroundArea.GroundOffset(direction, orbitRadius, groundRatio);

        /// <summary>
        /// The hitbox is the ellipse the warning draws, so the hit area is exactly what's shown. A CircleCollider2D
        /// can't be an ellipse, so the prefab's circle (its radius and offset stay the design values) is swapped for
        /// a PolygonCollider2D outlining the same ground circle, with the circle's trigger and layer settings. Physics
        /// then decides the hit against the player's own collider, where they stand. Other collider shapes are kept as is.
        /// </summary>
        public static Collider2D CreateGroundHitbox(Collider2D authored, float groundRatio, MeleeAttackerBehaviorState state)
        {
            if (!(authored is CircleCollider2D circle)) return authored;

            state.areaRadius = circle.radius;
            state.areaOffset = circle.offset;
            var polygon = circle.GetComponent<PolygonCollider2D>();
            if (polygon == null) polygon = circle.gameObject.AddComponent<PolygonCollider2D>();
            polygon.points = IsoGroundArea.Outline(circle.offset, circle.radius, groundRatio, HitboxSegments);
            polygon.isTrigger = circle.isTrigger;
            polygon.sharedMaterial = circle.sharedMaterial;
            polygon.includeLayers = circle.includeLayers;
            polygon.excludeLayers = circle.excludeLayers;
            circle.enabled = false;
            return polygon;
        }

        // The world ground circle the hitbox will cover at the locked direction: its center and radius
        private (Vector2 center, float radius) HitArea(MeleeAttackerBehaviorState state, Vector2 enemyPosition)
        {
            var hitbox = state.meleeCollider.transform;
            Vector2 scale = hitbox.lossyScale;
            Vector2 hitboxPosition = enemyPosition + HitboxOffset(state.lockedDirection, orbitRadius, GroundRatio);
            Vector2 center = hitboxPosition + Vector2.Scale(state.areaOffset, scale);
            return (center, state.areaRadius * Mathf.Abs(scale.x));
        }

        // Same span as isShooting: on when the swing starts, off at its end event, a stun or a freeze
        private static void SetKnockbackImmune(BehaviorContext context, bool immune)
        {
            if (context.enemyTransform != null && context.enemyTransform.TryGetComponent(out Knockback knockback))
            {
                knockback.SetImmune(immune);
            }
        }

        private static void HideTelegraph(MeleeAttackerBehaviorState state)
        {
            if (state.telegraph != null) state.telegraph.Hide();
        }

        public void OnAnimationEventEnd(BehaviorContext context, string eventName)
        {
            var state = context.GetState<MeleeAttackerBehaviorState>(this);
            state.isAttacking = false;
            context.isShooting = false;
            SetKnockbackImmune(context, false);
            HideTelegraph(state);

            if (state.meleeCollider != null)
            {
                state.meleeCollider.enabled = false;
            }else
            {
                Debug.LogWarning("Melee collider is not set in AIMeleeAttacker behavior state.");
            }

        }

        public void OnAnimationEventStart(BehaviorContext context, string eventName)
        {
            var state = context.GetState<MeleeAttackerBehaviorState>(this);
            // The hit lands: the warning has done its job
            HideTelegraph(state);
            if (state.meleeCollider != null)
            {
                state.meleeCollider.enabled = true;
                SetColliderPosition(state.meleeCollider.gameObject, state.lockedDirection, context.enemyTransform);
            }
            else
            {
                Debug.LogWarning("Melee collider is not set in AIMeleeAttacker behavior state.");
            }
        }

        private void SetColliderPosition(GameObject attackColliderTransform, Vector2 direction, Transform pivotPoint)
        {
            attackColliderTransform.transform.position = pivotPoint.position + (Vector3)HitboxOffset(direction, orbitRadius, GroundRatio);
        }
    }

}
