using System;
using Game.AI;
using UnityEngine;

namespace Game.Misc
{
    public class Knockback : MonoBehaviour
    {
        [SerializeField] float knockbackTime = 1.5f;
        [Tooltip("Set the push's speed instead of adding it to the body's current velocity, and stop the push when the " +
            "knockback ends, so every push travels Force x Knockback Time. Without it, a push adds to whatever the body " +
            "was already doing and keeps sliding afterwards. On for the player.")]
        [SerializeField] bool consistentPush = false;

        private Rigidbody2D rb;
        private float knockbackTimer = 0f;
       

        public event Action OnKnockbackStarted;

        public bool IsKnockbacked  {get; private set;}

        /// <summary>
        /// While set, knockback is ignored (damage and debuffs still land). A melee enemy sets it for its swing, so a
        /// hit can't push it off the spot its warning shows. Whoever sets it must clear it on every exit path.
        /// </summary>
        public bool IsImmune { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            KnockbackCheck();
        }

        private void KnockbackCheck()
        {
            if (!IsKnockbacked) return;

            
            if (knockbackTimer>0f)
            {
                knockbackTimer -= Time.deltaTime;
            }
            else if (knockbackTimer <= 0f)
            {
                // Reset the knockback state
                EndKnockback();
            }
        }

        /// <summary>
        /// Turns the immunity on or off. Turning it on also ends a knockback already playing out, so the movement
        /// behaviors take the body back and the attack's hold-still stops the slide.
        /// </summary>
        public void SetImmune(bool immune)
        {
            IsImmune = immune;
            if (immune && IsKnockbacked) EndKnockback();
        }

        /// <summary>Pushes away from <paramref name="damageSource"/>'s position (screen space).</summary>
        public void ApplyKnockback(Transform damageSource, float force)
        {
            if (damageSource == null) return;
            Push(PushVelocity((Vector2)(transform.position - damageSource.position), force));
        }

        /// <summary>
        /// Pushes away from an attacker's ground point (<see cref="IsoGround.PointOf"/>), from feet to feet, and along
        /// the ground: a push toward the top or bottom of the screen covers half the screen distance, like every other
        /// distance on the isometric ground. Used by enemy melee hits, whose hitbox position says nothing about where
        /// the attacker stands.
        /// </summary>
        public void ApplyKnockbackFrom(Vector2 sourceGroundPoint, float force)
        {
            Vector2 away = IsoGround.PointOf(transform) - sourceGroundPoint;
            Push(GroundPushVelocity(away, force, IsoGround.Ratio));
        }

        /// <summary>The push's velocity: <paramref name="force"/> along <paramref name="direction"/>; none without a direction.</summary>
        public static Vector2 PushVelocity(Vector2 direction, float force) =>
            direction.sqrMagnitude < 1e-8f ? Vector2.zero : direction.normalized * force;

        /// <summary>
        /// A push of <paramref name="force"/> along the ground in the screen direction <paramref name="away"/>: sideways
        /// at full speed, straight up or down the screen at <paramref name="groundRatio"/> times it.
        /// </summary>
        public static Vector2 GroundPushVelocity(Vector2 away, float force, float groundRatio) =>
            IsoGroundArea.GroundOffset(away, force, groundRatio);

        private void Push(Vector2 velocity)
        {
            if (IsKnockbacked || IsImmune) return;
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb == null) return;

            StartKnockback();
            if (consistentPush)
            {
                rb.linearVelocity = velocity;
            }
            else
            {
                // An impulse of mass x velocity: the same speed change whatever the body's mass
                rb.AddForce(velocity * rb.mass, ForceMode2D.Impulse);
            }
        }

        private void StartKnockback()
        {
            OnKnockbackStarted?.Invoke();
            knockbackTimer = knockbackTime;
            IsKnockbacked = true;
        }

        private void EndKnockback()
        {
            IsKnockbacked = false;
            knockbackTimer = 0f;
            if (consistentPush && rb != null) rb.linearVelocity = Vector2.zero;
        }

    }
}
