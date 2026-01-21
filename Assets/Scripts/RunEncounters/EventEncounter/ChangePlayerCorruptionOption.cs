using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Corruption", fileName = "ChangeCorruptionOption")]
    public class ChangePlayerCorruptionOption : EventOptionActionBase
    {
        [SerializeField] private int amount = 1;
        [SerializeField] private bool increase = true;
        public override void Apply(EventContext context)
        {
            if (context.playerCorruption == null)
            {
                Debug.LogError("[LoseCurrencyOption] PlayerCorruption missing from context.");
                return;
            }
            if (increase)
            {
                context.playerCorruption.AddCorruption(amount);
            }
            else
            {
                context.playerCorruption.ReduceCorruption(amount);
            }
                
        }
    }
}
