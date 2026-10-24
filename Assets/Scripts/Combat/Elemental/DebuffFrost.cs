using Game.AI.Behaviors;
using UnityEngine;

namespace Game.Combat.Elemental
{
    public class DebuffFrost : DebuffBase
    {
        protected override void OnApplied()
        {
            ApplyModifiers();
        }

        protected override void OnReapplied(float newDuration)
        {
            base.OnReapplied(newDuration);
            // Re-apply the modifiers: a fresh hit may carry a different strength.
            ApplyModifiers();
        }

        private void ApplyModifiers()
        {
            var behaviorController = GetComponent<BehaviorController>();
            if (behaviorController != null)
            {
                float clampedStrength = Mathf.Min(strength, data.MaxSlow);
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