using Game.Progression;
using System;
using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// A stat bonus that depends on another stat ("stat dependency enhancer"): every
    /// <see cref="perPoints"/> points of <see cref="sourceStat"/> give <see cref="amount"/> of
    /// <see cref="targetStat"/> (negative = a cost). Authored on items in the Effect Database
    /// window; used on starting items to give each class an identity.
    /// </summary>
    [Serializable]
    public class StatDependencyRule
    {
        [Tooltip("The stat whose total is counted.")]
        public StatType sourceStat;
        [Tooltip("Every this many points of the source stat apply the amount once. 1 = every point.")]
        [Min(1)]
        public int perPoints = 1;
        [Tooltip("The stat that receives the bonus.")]
        public StatType targetStat;
        [Tooltip("Added to the target stat per step. Negative values reduce it.")]
        public float amount = 1f;
    }
}
