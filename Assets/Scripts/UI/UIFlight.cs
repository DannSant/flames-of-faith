using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Path of a UI element flying from one point to another (e.g. a number flying from the HUD into a window):
    /// eased out, so it leaves fast and settles on its target, along an arc that bulges up by arcHeight at the middle.
    /// </summary>
    public static class UIFlight
    {
        /// <summary>The point at time t (0..1, clamped) of the flight from `from` to `to`.</summary>
        public static Vector3 Evaluate(Vector3 from, Vector3 to, float t, float arcHeight)
        {
            float eased = EaseOutCubic(Mathf.Clamp01(t));
            Vector3 point = Vector3.LerpUnclamped(from, to, eased);
            // 4t(1-t): 0 at both ends, 1 in the middle
            point.y += arcHeight * 4f * eased * (1f - eased);
            return point;
        }

        public static float EaseOutCubic(float t)
        {
            float inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }
    }
}
