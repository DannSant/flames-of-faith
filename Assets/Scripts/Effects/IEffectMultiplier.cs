using UnityEngine;

namespace Game.Effects
{
    public struct EffectMultiplierConfig
    {
        public int count;
        public float scaleValue;
        public float GetMultiplier() => count * scaleValue;
    }
    public interface IEffectMultiplier 
    {        
        void SetEffectID(string effectID);
    }
}
