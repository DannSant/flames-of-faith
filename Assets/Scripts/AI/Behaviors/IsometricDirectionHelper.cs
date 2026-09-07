using UnityEngine;

namespace Game.AI.Behaviors
{
    /// <summary>
    /// Snaps an arbitrary aim/move vector onto one of the 8 isometric facings the enemy sprite
    /// sets support. Everything that derives from a facing (animator parameters, projectile
    /// spread) goes through here, so the sprite and the shots can never disagree about which
    /// way the enemy is looking.
    /// </summary>
    public static class IsometricDirectionHelper
    {
        private const float DegreesPerDirection = 45f;
        private const float MinSqrMagnitude = 0.0001f;

        /// <summary>
        /// Returns the given direction rounded to the nearest 45 degrees, as a unit vector.
        /// Returns Vector2.zero for a degenerate input so callers can keep their previous facing.
        /// </summary>
        public static Vector2 SnapTo8(Vector2 direction)
        {
            if (direction.sqrMagnitude < MinSqrMagnitude) return Vector2.zero;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / DegreesPerDirection) * DegreesPerDirection;
            float radians = snappedAngle * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public static bool IsValid(Vector2 direction) => direction.sqrMagnitude >= MinSqrMagnitude;
    }
}
