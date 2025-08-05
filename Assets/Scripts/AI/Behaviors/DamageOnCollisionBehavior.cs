using Game.Combat;
using Game.Enemies;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.AI.Behaviors
{
    public class DamageOnCollisionBehaviorState
    {
        public float lastDamageTime = 0;        
    }

    [CreateAssetMenu(menuName = "Behaviors/Damage On Collision")]
    public class DamageOnCollisionBehavior : AICollisionBehavior
    {
        [SerializeField] private float cooldownDuration = 1f;      

        public override void HandleCollision(Collision2D collision, BehaviorContext context)
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

            if (health == null) return;

            GameObject enemyObject = context.enemyGameObject;

            var state = context.GetState<DamageOnCollisionBehaviorState>(this);

            
            float lastTime = state.lastDamageTime;

            if (Time.time - lastTime >= cooldownDuration)
            {
                int damageAmount = GetDamageAmount(context);
                health.TakeDamage(damageAmount);
              
                state.lastDamageTime = Time.time; 
            }
            
        }

        /*private int GetDamageAmount(BehaviorContext context)
        {
            var enemyData = context.enemyData;
            int levelDamageBonus = GameSession.Instance.LevelsBeaten * enemyData.damagePerLevel;
            int waveDamageBonus = enemyData.damagePerWave * (context.waveNumber - 1);
            return enemyData.damageBase + waveDamageBonus + levelDamageBonus;
        }*/
        
        
    }

}