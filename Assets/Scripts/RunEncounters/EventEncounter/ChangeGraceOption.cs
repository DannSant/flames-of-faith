using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Grace", fileName = "ChangeGraceOption")]
    public class ChangeGraceOption : EventOptionActionBase
    {
        [SerializeField] private float amount = 10f;
        [SerializeField] private bool increase = true;

        public override void Apply(EventContext context)
        {
            if (context.playerGrace == null)
            {
                Debug.LogError("[ChangeGraceOption] PlayeeGRace missing from context.");
                return;
            }
            if (increase)
            {
                context.playerGrace.AddGrace(amount);
            }
            else
            {
                context.playerGrace.RemoveGrace(amount);
            }

        }
    }

}