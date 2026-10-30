using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>Gives or takes Grace; reports the change after Grace's range clamp.</summary>
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Grace", fileName = "ChangeGraceOption")]
    public class ChangeGraceOption : EventOptionActionBase
    {
        public override EventResultKind Kind => EventResultKind.Grace;

        public override EventActionResult Apply(EventContext context)
        {
            if (context.playerGrace == null)
            {
                Debug.LogError("[ChangeGraceOption] PlayerGrace missing from context.");
                return EventActionResult.Nothing(Kind);
            }

            float finalAmount = RollAmount(context);
            float before = context.playerGrace.CurrentGrace;
            if (finalAmount >= 0)
            {
                context.playerGrace.AddGrace(finalAmount);
            }
            else
            {
                context.playerGrace.RemoveGrace(-finalAmount);
            }

            return EventActionResult.Of(Kind, context.playerGrace.CurrentGrace - before);
        }
    }

}
