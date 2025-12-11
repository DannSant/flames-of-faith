
using Game.Combat;
using Game.Control;
using Game.Enemies;
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
        public List<AICollisionBehavior> collisionBehaviors;
        public List<AITriggerBehavior> triggerBehaviors;
        public List<AIDeathBehavior> deathBehaviors;

        private Transform player;
        private BehaviorContext context;
        private EnemyHealth health;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            
        }

        private void Start()
        {
            health.onDeath += Die;
        }

        private void OnDisable()
        {
            health.onDeath -= Die;
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
            foreach (var behavior in updateBehaviors)
                behavior.Tick(context);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
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
            foreach (var behavior in deathBehaviors)
            {
                behavior.OnDeath(context);
            }

            Destroy(gameObject);
        }

        /*private void OnDestroy()
        {
            foreach (var behavior in deathBehaviors)
            {
                behavior.OnDeath(context);
            }
        }*/
        private IEnumerable<AIBehavior> GetAllBehaviors()
        {
            return updateBehaviors.Cast<AIBehavior>()
                .Concat(collisionBehaviors)
                .Concat(triggerBehaviors)
                .Concat(deathBehaviors);
        }

        public void OnAnimationEventStart(string eventName)
        {
            foreach (var behavior in updateBehaviors)
            {
                if (behavior is IAnimationEventReceiver receiver)
                    receiver.OnAnimationEventStart(context, eventName);
            }
        }

        public void OnAnimationEventEnd(string eventName)
        {
           
            foreach (var behavior in updateBehaviors)
            {
                if (behavior is IAnimationEventReceiver receiver)
                    receiver.OnAnimationEventEnd(context, eventName);
            }
        }

        public BehaviorContext GetBehaviorContext()
        {
            return context;
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
