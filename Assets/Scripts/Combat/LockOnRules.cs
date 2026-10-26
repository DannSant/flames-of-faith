using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>What a tap of the lock key (F / LT, Toggle mode) does.</summary>
    public enum LockTap
    {
        /// <summary>Not locked: lock the current or nearest enemy.</summary>
        Lock,
        /// <summary>Locked with other enemies nearby: move to the next one, clockwise.</summary>
        Cycle,
        /// <summary>Locked and no enemy nearby (the player walked away): release the lock.</summary>
        Unlock,
        /// <summary>Nothing to do: no enemy to lock, or only the locked enemy is nearby.</summary>
        Nothing,
    }

    /// <summary>
    /// The lock-on rules, pure so they are tested without a scene (tester feedback, 2026-10-26). "Nearby" means
    /// within lock range (weapon range × WeaponManager.lockOnRangeMultiplier). A tap never unlocks while
    /// enemies are nearby, the locked one included: the player only needs to unlock after walking away.
    /// </summary>
    public static class LockOnRules
    {
        /// <param name="locked">A lock is on and its target is still alive.</param>
        /// <param name="enemiesNearby">Targetable enemies within lock range, the locked one included.</param>
        /// <param name="othersNearby">Those other than the locked one.</param>
        public static LockTap DecideTap(bool locked, int enemiesNearby, int othersNearby)
        {
            if (!locked) return enemiesNearby > 0 ? LockTap.Lock : LockTap.Nothing;
            if (othersNearby > 0) return LockTap.Cycle;
            return enemiesNearby == 0 ? LockTap.Unlock : LockTap.Nothing;
        }

        /// <summary>
        /// The next target clockwise around <paramref name="origin"/>, starting from the current one's direction:
        /// the index in <paramref name="others"/> with the smallest clockwise turn from <paramref name="current"/>.
        /// Going by direction rather than distance keeps the order stable while enemies move. -1 when empty.
        /// </summary>
        public static int NextClockwise(Vector2 origin, Vector2 current, IReadOnlyList<Vector2> others)
        {
            if (others == null || others.Count == 0) return -1;

            float currentAngle = Angle(current - origin);
            int best = -1;
            float bestTurn = float.MaxValue;
            for (int i = 0; i < others.Count; i++)
            {
                float turn = ClockwiseTurn(currentAngle, Angle(others[i] - origin));
                if (turn < bestTurn)
                {
                    bestTurn = turn;
                    best = i;
                }
            }
            return best;
        }

        // Degrees turned clockwise from one direction to another, in (0, 360]: an enemy exactly behind the
        // current one in the same direction comes last.
        private static float ClockwiseTurn(float fromDeg, float toDeg)
        {
            float turn = Mathf.Repeat(fromDeg - toDeg, 360f);
            return turn <= 0.0001f ? 360f : turn;
        }

        private static float Angle(Vector2 direction) => Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}
