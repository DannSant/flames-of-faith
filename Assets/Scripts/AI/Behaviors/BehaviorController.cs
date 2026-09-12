
using Game.Combat;
using Game.Control;
using Game.Enemies;
using Game.Misc;
using Game.Scene;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
namespace Game.AI.Behaviors
{
   
    
    public class BehaviorController : MonoBehaviour
    {
        public List<AIUpdateBehavior> updateBehaviors;
        public List<AIFixedUpdateBehavior> fixedUpdateBehaviors;
        public List<AICollisionBehavior> collisionBehaviors;
        public List<AITriggerBehavior> triggerBehaviors;
        public List<AIDeathBehavior> deathBehaviors;

        private Transform player;
        private BehaviorContext context;
        private EnemyHealth health;
        private StunHandler stunHandler;
        private Knockback knockback;
        private Rigidbody2D rb;

        private bool IsStunned => stunHandler != null && stunHandler.IsStunned;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            stunHandler = GetComponent<StunHandler>();
            knockback = GetComponent<Knockback>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            health.onDeath += Die;
        }

        private void OnEnable()
        {
            if (stunHandler == null) return;

            stunHandler.OnStunStarted += HandleStunStarted;
            stunHandler.OnStunEnded += HandleStunEnded;
        }

        private void OnDisable()
        {
            health.onDeath -= Die;

            if (stunHandler == null) return;

            stunHandler.OnStunStarted -= HandleStunStarted;
            stunHandler.OnStunEnded -= HandleStunEnded;
        }

        public void Initialize(BehaviorContext ctx)
        {
            context = ctx;
            player = PlayerManager.Instance.GetPlayerComponent<PlayerController>().transform;
            foreach (var behavior in GetAllBehaviors())
            {
                behavior.Initialize(context);
            }
        }

        void Update()
        {
            if (IsStunned) return;

            foreach (var behavior in updateBehaviors)
                behavior.Tick(context);
        }

        void FixedUpdate()
        {
            if (IsStunned)
            {
                HoldStillWhileStunned();
                return;
            }

            foreach (var behavior in fixedUpdateBehaviors)
                behavior.FixedTick(context);
        }

        /// <summary>
        /// The body is Dynamic with no linear damping, so velocity has to be rewritten every tick -
        /// zeroing it once when the stun lands would let a later knockback or a shove from another
        /// enemy slide the enemy for the rest of it. Knockback still owns the body while active.
        /// </summary>
        private void HoldStillWhileStunned()
        {
            if (rb == null) return;
            if (knockback != null && knockback.IsKnockbacked) return;

            rb.linearVelocity = Vector2.zero;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsStunned && stunHandler.BlockContactDamageWhileStunned) return;

            foreach (var behavior in collisionBehaviors)
                behavior.HandleCollision(collision,context);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            foreach (var behavior in triggerBehaviors)
                behavior.HandleCollisionTrigger(collider,context);
        }

        public void Die()
        {
            context.diedSilently = health.DiedSilently;

            foreach (var behavior in deathBehaviors)
            {
                behavior.OnDeath(context);
            }

            Destroy(gameObject);
        }

        private IEnumerable<AIBehavior> GetAllBehaviors()
        {
            return updateBehaviors.Cast<AIBehavior>()
                .Concat(fixedUpdateBehaviors)
                .Concat(collisionBehaviors)
                .Concat(triggerBehaviors)
                .Concat(deathBehaviors);
        }

        public void OnAnimationEventStart(string eventName)
        {
            // A stun freezes the animator, so clip events shouldn't reach here at all - this is the
            // guarantee that nothing slips through on the frame the stun lands. It matters most for
            // the enemies whose movement and projectiles come *only* from animation events (all the
            // slimes), which a Tick-level gate alone would never stop.
            // End events are deliberately still dispatched below so in-flight actions can clean up.
            if (IsStunned) return;

            foreach (var receiver in GetAnimationEventReceivers())
            {
                receiver.OnAnimationEventStart(context, eventName);
            }
        }

        public void OnAnimationEventEnd(string eventName)
        {
            foreach (var receiver in GetAnimationEventReceivers())
            {
                receiver.OnAnimationEventEnd(context, eventName);
            }
        }

        // Any behavior implementing IAnimationEventReceiver gets animation events, no matter
        // which list it was authored into - previously only updateBehaviors were scanned, so a
        // receiver placed in any other list would silently never fire.
        private IEnumerable<IAnimationEventReceiver> GetAnimationEventReceivers()
        {
            return GetAllBehaviors().OfType<IAnimationEventReceiver>();
        }

        public BehaviorContext GetBehaviorContext()
        {
            return context;
        }

        private void HandleStunStarted() => NotifyStunStateChanged(true);
        private void HandleStunEnded() => NotifyStunStateChanged(false);

        private void NotifyStunStateChanged(bool isStunned)
        {
            // Enemy.Initialize can bail before handing us a context, so this can fire uninitialized.
            if (context == null) return;

            foreach (var behavior in GetAllBehaviors())
            {
                behavior.OnStunStateChanged(context, isStunned);
            }
        }

        public void SetContextSpeedMultiplier(float multiplier)
        {
            if (context != null)
            {
                context.speedMultiplier = multiplier;
            }
        }
        public void ResetContextSpeedMultiplier()
        {
            if (context != null)
            {
                context.speedMultiplier = 1.0f;
            }
        }

    }
}
