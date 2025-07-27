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
        //void SetMultiplierConfig(EffectMultiplierConfig config);
        void SetEffectStore(EffectStore effectStore);
        void SetEffectID(string effectID);
    }
}
