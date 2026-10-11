using UnityEngine;

namespace Game.Combat.Projectiles
{
    /// <summary>
    /// 2D course correction for projectiles: turn a direction toward a target by a limited angle.
    /// Pure functions, so the maths can be tested without a scene.
    /// </summary>
    public static class ProjectileSteering
    {
        /// <summary>
        /// Rotates <paramref name="direction"/> toward <paramref name="toTarget"/> by at most
        /// <paramref name="maxDegrees"/>, never past it. Returns a unit vector. A zero target
        /// vector keeps the direction; a zero direction snaps to the target. A target exactly
        /// behind turns counter-clockwise.
        /// </summary>
        public static Vector2 Steer(Vector2 direction, Vector2 toTarget, float maxDegrees)
        {
            if (toTarget.sqrMagnitude < 1e-8f) return direction.normalized;
            if (direction.sqrMagnitude < 1e-8f) return toTarget.normalized;
            if (maxDegrees <= 0f) return direction.normalized;

            float angle = Vector2.SignedAngle(direction, toTarget);
            float turn = Mathf.Clamp(angle, -maxDegrees, maxDegrees);
            Vector2 rotated = Quaternion.Euler(0f, 0f, turn) * direction.normalized;
            return rotated.normalized;
        }

        /// <summary>
        /// Whether the target is still ahead enough to keep steering: within
        /// <paramref name="maxAngle"/> degrees of the current direction. Arrows stop correcting
        /// once they've passed or missed their target instead of turning back.
        /// </summary>
        public static bool IsWithinSteeringCone(Vector2 direction, Vector2 toTarget, float maxAngle)
        {
            if (toTarget.sqrMagnitude < 1e-8f || direction.sqrMagnitude < 1e-8f) return false;
            return Vector2.Angle(direction, toTarget) <= maxAngle;
        }
    }
}
