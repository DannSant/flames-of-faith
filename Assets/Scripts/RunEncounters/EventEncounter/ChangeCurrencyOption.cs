using Game.RunEncounters;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Currency", fileName = "ChangeCurrencyOption")]
    public class ChangeCurrencyOption : EventOptionActionBase
    {
        [SerializeField] private int amount = 10;
        [SerializeField] private bool increase = true;
        public override void Apply(EventContext context)
        {
            if (context.playerWallet == null)
            {
                Debug.LogError("[ChangeCurrencyOption] PlayerWallet missing from context.");
                return;
            }
            if (increase)
            {
                context.playerWallet.AddCurrency(amount);
            }
            else
            {
                context.playerWallet.RemoveCurrency(amount);
            }
            
        }
    }

}