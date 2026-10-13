using Game.Metaprogression;
using Game.Utils;
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
        public override string Apply(EventContext context)
        {
            if (MetaProgressionManager.Instance == null)
            {
                Debug.LogError("[ChangeFavorsOption] MetaProgressionManager missing.");
                return string.Empty;
            }

            int finalAmount = isPlainReward
                ? Mathf.RoundToInt(amount)
                : Mathf.RoundToInt(StatBiasedRoll.Roll(amountRange, baseChance, context.playerProgression, biasStat));

            MetaProgressionManager.Instance.AddRunFavors(finalAmount);

            string label = string.IsNullOrEmpty(resourceName) ? "Favors" : resourceName;
            return $"{(finalAmount >= 0 ? "Gained" : "Lost")} {Mathf.Abs(finalAmount)} {label}";
        }
    }
}
