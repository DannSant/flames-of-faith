using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>Gives or takes Golden Chronos. A loss takes no more than the player has.</summary>
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Currency", fileName = "ChangeCurrencyOption")]
    public class ChangeCurrencyOption : EventOptionActionBase
    {
        public override EventResultKind Kind => EventResultKind.Chronos;

        public override EventActionResult Apply(EventContext context)
        {
            if (context.playerWallet == null)
            {
                Debug.LogError("[ChangeCurrencyOption] PlayerWallet missing from context.");
                return EventActionResult.Nothing(Kind);
            }

            int finalAmount = Mathf.RoundToInt(RollAmount(context));
            if (finalAmount >= 0)
            {
                context.playerWallet.AddCurrency(finalAmount);
                return EventActionResult.Of(Kind, finalAmount);
            }

            int lost = Mathf.Min(-finalAmount, Mathf.Max(0, context.playerWallet.CurrencyAmount));
            context.playerWallet.RemoveCurrency(lost);
            return EventActionResult.Of(Kind, -lost);
        }
    }

}
