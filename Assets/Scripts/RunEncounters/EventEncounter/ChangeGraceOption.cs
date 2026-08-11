using Game.Utils;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Grace", fileName = "ChangeGraceOption")]
    public class ChangeGraceOption : EventOptionActionBase
    {
        public override string Apply(EventContext context)
        {
            if (context.playerGrace == null)
            {
                Debug.LogError("[ChangeGraceOption] PlayeeGRace missing from context.");
                return string.Empty;
            }

            float finalAmount = isPlainReward
                ? amount
                : StatBiasedRoll.Roll(amountRange, baseChance, context.playerProgression, biasStat);

            if (finalAmount >= 0)
            {
                context.playerGrace.AddGrace(finalAmount);
            }
            else
            {
                context.playerGrace.RemoveGrace(-finalAmount);
            }

            return $"{(finalAmount >= 0 ? "Gained" : "Lost")} {Mathf.Abs(finalAmount)} {resourceName}";
        }
    }

}