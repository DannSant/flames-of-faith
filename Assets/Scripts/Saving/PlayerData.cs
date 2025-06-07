using Game.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Saving
{
    [Serializable]
    public class PlayerData
    {
        public int currentHealth = 0;
        public Dictionary<StatType, int> savedStats = new Dictionary<StatType, int>();
    }

}