using UnityEngine;

namespace Game.Effects
{
    public struct EffectMultiplierConfig
    {
        public int count;
    }
    public interface IEffectMultiplier 
    {        
        void SetEffectID(string effectID);
    }
}
