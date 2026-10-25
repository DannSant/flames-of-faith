using Game.Progression;
using System;
using UnityEngine;

namespace Game.Effects
{
    public enum ModifierType
    {
        Flat,           // +5
        PercentAdd,     // +10% (additive)       
    }
    [Serializable]
    public class StatModifier
    {
        [Tooltip("The stat the item changes.")]
        public StatType stat;
        [Tooltip("How much it adds (negative values reduce it).")]
        public float value;
        [Tooltip("Flat adds the value as points; PercentAdd adds it as a share of the stat (0.1 = +10%).")]
        public ModifierType type = ModifierType.Flat;
    }

}