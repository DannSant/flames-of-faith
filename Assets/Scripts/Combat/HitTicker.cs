using System.Collections.Generic;

namespace Game.Combat
{
    /// <summary>
    /// Per-target hit timing for a damage area that keeps hurting what stays inside it (the Whirlwind):
    /// a target is hit on contact, then again every <see cref="Interval"/> seconds, each on its own timer.
    /// With an interval of 0 a target is hit once until <see cref="Clear"/> (a new activation).
    /// Pure, so the timing is tested without physics.
    /// </summary>
    public class HitTicker
    {
        private readonly Dictionary<object, float> nextHitTime = new();

        public float Interval { get; set; }

        /// <summary>True when <paramref name="target"/> may be hit at <paramref name="now"/>; records the hit.</summary>
        public bool TryHit(object target, float now)
        {
            if (target == null) return false;
            if (nextHitTime.TryGetValue(target, out float next) && (Interval <= 0f || now < next)) return false;

            nextHitTime[target] = now + Interval;
            return true;
        }

        public void Clear() => nextHitTime.Clear();
    }
}
