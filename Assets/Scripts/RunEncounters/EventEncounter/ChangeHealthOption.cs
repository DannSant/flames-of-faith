using Game.Utils;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Health", fileName = "ChangeHealthOption")]
    public class ChangeHealthOption : EventOptionActionBase
    {
        public override string Apply(EventContext context)
        {
            if (context.playerHealth == null)
            {
                Debug.LogError("[ChangeHealthOption] PlayerHealth missing from context.");
                return string.Empty;
            }

            float finalAmount = isPlainReward
                ? amount
                : StatBiasedRoll.Roll(amountRange, baseChance, context.playerProgression, biasStat);

            if (finalAmount >= 0)
            {
                context.playerHealth.Heal(finalAmount);
                return $"Gained {finalAmount} {resourceName}";
            }

            // Events happen outside combat levels, where a death isn't handled, so the hit is
            // non-lethal. Report what was actually lost (after armor and the 1 HP floor).
            float damageTaken = context.playerHealth.TakeDamage(-finalAmount, nonLethal: true);
            return $"Lost {damageTaken} {resourceName}";
        }
    }

}