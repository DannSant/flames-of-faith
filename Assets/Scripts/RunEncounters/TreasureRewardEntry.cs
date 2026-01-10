using Game.Effects;
using NUnit.Framework.Interfaces;
using System;
using UnityEngine;

namespace Game.RunEncounters
{
    public enum TreasureRewardType
    {
        Item,
        Currency,
        Experience,
        Random // any of the above
    }

    [Serializable]
    public class TreasureRewardEntry 
    {
        public TreasureRewardType rewardType;

        [Header("Weight")]
        public float weight = 1f;

        [Header("Item Reward")]
        public Effect item;
        public int itemCount = 1;

        [Header("Currency Reward")]
        public int minCurrency;
        public int maxCurrency;

        [Header("Experience Reward")]
        public int minExperience;
        public int maxExperience;
    }

}