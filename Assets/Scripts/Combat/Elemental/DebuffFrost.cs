using Game.AI.Behaviors;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffFrost : DebuffBase
    {
        private float hardLimitReduction = 0.8f; // max 80% slow
        public override void Initialize(ElementalDebuffData data, float duration, float strength, int generation)
        {
            base.Initialize(data, duration, strength, generation);

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