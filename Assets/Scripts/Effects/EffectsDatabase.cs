
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects
{
    [CreateAssetMenu(fileName = "EffectsDatabase", menuName = "Effects/EffectsDatabase")]
    public class EffectsDatabase : ScriptableObject
    {
        public List<Effect> availableEffects;
        public List<Effect> unlockableEffects;
    }

}