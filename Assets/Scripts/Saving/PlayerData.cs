using Game.Effects;
using Game.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Saving
{
    [Serializable]
    public class PlayerData
    {
        public float currentHealth = 0;
        public float currentGrace = 0;
        public Dictionary<StatType, int> savedStats = new Dictionary<StatType, int>();
        public PlayerExperienceData playerExperienceData = new PlayerExperienceData();
        public List<EffectInstance> savedEffects = new();
        public int currencyAmount = 0;    
        public float corruptionLevel = 0f;
    }

}