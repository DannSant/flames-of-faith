using Game.Progression;
using Mono.Cecil;
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
        public StatType stat;      // You already have StatType in your project
        public float value;
        public ModifierType type = ModifierType.Flat;
    }

}