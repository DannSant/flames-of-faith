using Game.Utils;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Corruption", fileName = "ChangeCorruptionOption")]
    public class ChangePlayerCorruptionOption : EventOptionActionBase
    {
        public override string Apply(EventContext context)
        {
            if (context.playerCorruption == null)
            {
                Debug.LogError("[LoseCurrencyOption] PlayerCorruption missing from context.");
                return string.Empty;
            }

            int finalAmount = isPlainReward
                ? Mathf.RoundToInt(amount)
                : Mathf.RoundToInt(StatBiasedRoll.Roll(amountRange, baseChance, context.playerProgression, biasStat));

            if (finalAmount >= 0)
            {
                context.playerCorruption.AddCorruption(finalAmount);
            }
            else
            {
                context.playerCorruption.ReduceCorruption(-finalAmount);
            }

            return $"{(finalAmount >= 0 ? "Gained" : "Lost")} {Mathf.Abs(finalAmount)} {resourceName}";
        }
    }
}
