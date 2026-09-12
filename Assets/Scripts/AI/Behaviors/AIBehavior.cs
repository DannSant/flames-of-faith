using Game.Enemies;
using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public abstract class AIBehavior : ScriptableObject
    {
        public virtual void Initialize(BehaviorContext ctx) { }

        /// <summary>
        /// Called on every behavior when the enemy is stunned or recovers. Behaviors that start
        /// something the animator normally finishes (a melee hitbox, a shot burst) must cancel it
        /// here: a stun freezes the animator, so the animation event that would have cleaned up
        /// never arrives.
        /// </summary>
        public virtual void OnStunStateChanged(BehaviorContext ctx, bool isStunned) { }

        protected int GetDamageAmount(BehaviorContext context)
        {
            return EnemyDamageCalculator.Calculate(
                context.enemyData,
                context.waveNumber,
                GameSession.Instance.LevelsBeaten,
                EnemyDamageKind.Contact);
        }

        protected int GetRangedDamageAmount(BehaviorContext context)
        {
            return EnemyDamageCalculator.Calculate(
                context.enemyData,
                context.waveNumber,
                GameSession.Instance.LevelsBeaten,
                EnemyDamageKind.Projectile);
        }

    }
    public abstract class AIUpdateBehavior : AIBehavior
    {
        public abstract void Tick(BehaviorContext context);
    }

    public abstract class AIFixedUpdateBehavior : AIBehavior
    {
        public abstract void FixedTick(BehaviorContext context);
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
