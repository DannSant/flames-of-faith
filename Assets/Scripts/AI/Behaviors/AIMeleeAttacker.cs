using Game.Combat;
using Game.Enemies;
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
    }
    [CreateAssetMenu(menuName = "Behaviors/On Update/MeleeAttacker")]
    public class AIMeleeAttacker : AIUpdateBehavior, IAnimationEventReceiver
    {
        [Tooltip("The enemy attacks only while the player is closer than this.")]
        [SerializeField] private float maxRange = 7f;
        [Tooltip("Seconds between attacks.")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private string colliderObjectName = "MeleeCollider";
        [Tooltip("Distance from the enemy's center at which the melee hitbox is placed, toward the player.")]
        [SerializeField] private float orbitRadius = 0.5f;
        [Tooltip("Seconds between the start of the swing and the hit: the warning shows the hit area for this long, " +
            "and the attack animation is slowed so its hit frame lands then.")]
        [Min(0.05f)]
        [SerializeField] private float telegraphTime = 0.8f;
        [Tooltip("Seconds into the attack animation, at normal speed, when the hit lands (its attack state's " +
            "MeleeAnimationAttackBehavior Attack Delay x the clip length). Used to slow the animation to Telegraph Time.")]
        [Min(0.01f)]
        [SerializeField] private float animationHitTime = 0.3f;

        public float TelegraphTime => telegraphTime;
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
                state.meleeCollider = colliderObj.GetComponent<Collider2D>();
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

            if (state.telegraph != null && state.meleeCollider != null)
            {
                var (center, radius) = HitArea(state.meleeCollider, context.enemyTransform.position, state.lockedDirection);
                state.telegraph.Show(center, radius, telegraphTime);
            }

            if (context.enemyAnimController != null)
            {
                context.enemyAnimController.PlayAttack(AttackAnimationSpeed(animationHitTime, telegraphTime));
            }
        }

        /// <summary>Speed of the attack animation so its hit frame (at <paramref name="animationHitTime"/>) lands after <paramref name="telegraphTime"/>.</summary>
        public static float AttackAnimationSpeed(float animationHitTime, float telegraphTime) =>
            telegraphTime <= 0f ? 1f : Mathf.Max(0.05f, animationHitTime / telegraphTime);

        /// <summary>Where the hitbox sits relative to the enemy: <paramref name="orbitRadius"/> along the locked direction.</summary>
        public static Vector2 HitboxOffset(Vector2 direction, float orbitRadius) =>
            direction.sqrMagnitude > 0.0001f ? direction.normalized * orbitRadius : Vector2.zero;

        // The world circle the hitbox will cover: its position plus the collider's own offset, and its radius
        private (Vector2 center, float radius) HitArea(Collider2D hitbox, Vector2 enemyPosition, Vector2 direction)
        {
            Vector2 scale = hitbox.transform.lossyScale;
            Vector2 hitboxPosition = enemyPosition + HitboxOffset(direction, orbitRadius);
            Vector2 center = hitboxPosition + Vector2.Scale(hitbox.offset, scale);
            float radius = hitbox switch
            {
                CircleCollider2D circle => circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)),
                BoxCollider2D box => 0.5f * Mathf.Max(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y)),
                _ => 0.5f,
            };
            return (center, radius);
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
            attackColliderTransform.transform.position = pivotPoint.position + (Vector3)HitboxOffset(direction, orbitRadius);
        }
    }

}
