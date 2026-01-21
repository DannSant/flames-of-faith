using Game.RunEncounters;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Lose Currency", fileName = "LoseCurrencyOption")]
    public class LoseCurrencyOption : EventOptionActionBase
    {
        [SerializeField] private int amount = 10;
        public override void Apply(EventContext context)
        {
            if (context.playerHealth == null)
            {
                Debug.LogError("[LoseCurrencyOption] PlayerHealth missing from context.");
                return;
            }
            context.playerWallet.RemoveCurrency(amount);
        }
    }

}