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
            }
            else
            {
                context.playerHealth.TakeDamage(-finalAmount);
            }

            return $"{(finalAmount >= 0 ? "Gained" : "Lost")} {Mathf.Abs(finalAmount)} {resourceName}";
        }
    }

}