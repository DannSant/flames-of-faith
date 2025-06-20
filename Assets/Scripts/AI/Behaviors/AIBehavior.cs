using UnityEngine;

namespace Game.AI.Behaviors
{
    public abstract class AIBehavior : ScriptableObject
    {
        public virtual void Initialize(BehaviorContext ctx) { }
        
    }
    public abstract class AIUpdateBehavior : AIBehavior
    {
        public abstract void Tick(BehaviorContext context);
    }

    public abstract class AICollisionBehavior : AIBehavior
    {
        public abstract void HandleCollision(Collision2D collision, BehaviorContext context);
    }

    public abstract class AITriggerBehavior : AIBehavior
    {
        public abstract void HandleCollisionTrigger(Collider2D collider, BehaviorContext context);
    }

    public abstract class AIDeathBehavior : AIBehavior
    {
        public abstract void OnDeath(BehaviorContext context);
    }

    public interface IAnimationEventReceiver
    {
        void OnAnimationEventStart(BehaviorContext context, string eventName);
        void OnAnimationEventEnd(BehaviorContext context, string eventName);
    }
}
