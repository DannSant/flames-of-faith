using UnityEngine;

namespace Game.Effects
{
    public struct EffectMultiplierConfig
    {
        public int count;
        public float scaleValue;
    }
    public interface IEffectMultiplier 
    {        
        void SetEffectID(string effectID);
    }
}
