using Game.AI.Behaviors;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffFrost : DebuffBase
    {
        private float hardLimitReduction = 0.8f; // max 80% slow
        public override void Initialize(float duration, float strength)
        {
            base.Initialize(duration, strength);

            //If the component is removed, do nothing
            if(this==null) return;
            var behaviorController = GetComponent<BehaviorController>();
            if (behaviorController != null)
            {
                float clampedStrength = Mathf.Min(strength, hardLimitReduction);              
                behaviorController.SetContextSpeedMultiplier(1f - clampedStrength);
            }
            var enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.SetExtraDamageTakenPercentage(strength);
            }

        }

        public override void End()
        {
            var behaviorController = GetComponent<BehaviorController>();
            if (behaviorController != null)
            {
                behaviorController.ResetContextSpeedMultiplier();
            }
            var enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.ResetExtraDamageTakenPercentage();
            }
        }
    }

}