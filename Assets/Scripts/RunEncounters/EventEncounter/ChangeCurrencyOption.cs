using Game.RunEncounters;
using Game.Utils;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Currency", fileName = "ChangeCurrencyOption")]
    public class ChangeCurrencyOption : EventOptionActionBase
    {
        public override string Apply(EventContext context)
        {
            if (context.playerWallet == null)
            {
                Debug.LogError("[ChangeCurrencyOption] PlayerWallet missing from context.");
                return string.Empty;
            }

            int finalAmount = isPlainReward
                ? Mathf.RoundToInt(amount)
                : Mathf.RoundToInt(StatBiasedRoll.Roll(amountRange, baseChance, context.playerProgression, biasStat));

            if (finalAmount >= 0)
            {
                context.playerWallet.AddCurrency(finalAmount);
            }
            else
            {
                context.playerWallet.RemoveCurrency(-finalAmount);
            }

            return $"{(finalAmount >= 0 ? "Gained" : "Lost")} {Mathf.Abs(finalAmount)} {resourceName}";
        }
    }

}