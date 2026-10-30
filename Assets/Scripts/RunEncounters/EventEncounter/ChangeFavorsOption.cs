using Game.Metaprogression;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>
    /// Event option that grants (or costs, if negative) Favors for the class being played. Like every
    /// Favor earned in a level, they're kept once the encounter is finished.
    /// </summary>
    [CreateAssetMenu(menuName = "RunEncounters/EventOptionsActions/Change Favors", fileName = "ChangeFavorsOption")]
    public class ChangeFavorsOption : EventOptionActionBase
    {
        public override EventResultKind Kind => EventResultKind.Favors;

        public override EventActionResult Apply(EventContext context)
        {
            if (MetaProgressionManager.Instance == null)
            {
                Debug.LogError("[ChangeFavorsOption] MetaProgressionManager missing.");
                return EventActionResult.Nothing(Kind);
            }

            int finalAmount = Mathf.RoundToInt(RollAmount(context));
            MetaProgressionManager.Instance.AddRunFavors(finalAmount);
            return EventActionResult.Of(Kind, finalAmount);
        }
    }
}
