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
        [Header("Reward Rules")]
        public bool allowReject = true;

        [Tooltip("If true, pick one random reward from the list")]
        public bool pickSingleReward = true;

        [Tooltip("If false, rewardType == Random is resolved here")]
        public List<TreasureRewardEntry> rewardTable;
    }
}