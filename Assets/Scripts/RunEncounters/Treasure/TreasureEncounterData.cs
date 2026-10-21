using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(
    menuName = "RunEncounters/Treasure Encounter",
    fileName = "TreasureEncounterData"
)]
    public class TreasureEncounterData : ScriptableObject
    {
        [Tooltip("One reward is picked by weight. An item goes to the item bag (take or sell it); " +
                 "currency and experience are given at once and shown in the Results window.")]
        public List<TreasureRewardEntry> rewardTable;
    }
}
