using Game.Scene;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public abstract class AIBehavior : ScriptableObject
    {
        public virtual void Initialize(BehaviorContext ctx) { }

        protected int GetDamageAmount(BehaviorContext context)
        {
            var enemyData = context.enemyData;
            int levelDamageBonus = GameSession.Instance.LevelsBeaten * enemyData.damagePerLevel;
            int waveDamageBonus = enemyData.damagePerWave * (context.waveNumber - 1);
            return enemyData.damageBase + waveDamageBonus + levelDamageBonus;
        }

        protected int GetRangedDamageAmount(BehaviorContext context)
        {
            var enemyData = context.enemyData;
            int levelDamageBonus = GameSession.Instance.LevelsBeaten * enemyData.damagePerLevel;
            int waveDamageBonus = enemyData.projectileDamagePerWave * (context.waveNumber - 1);
            return enemyData.projectileDamageBase + waveDamageBonus + levelDamageBonus;
        }

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
