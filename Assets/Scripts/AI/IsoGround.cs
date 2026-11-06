using System.Collections.Generic;
using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// The isometric ground as gameplay sees it: where a character stands, and how far apart two characters are on the
    /// ground. Use it for any range, stop distance or area that should be the same from every side (melee attacks,
    /// chasers, future cone or line attacks). The math is the pure <see cref="IsoGroundArea"/>; this adds the scene side.
    ///
    /// A character's ground point is the center of its solid body collider (the first enabled, non-trigger Collider2D on
    /// its root), which is where enemies and the player collide with each other and the world. Without one, its position.
    /// The ground ratio is AttackTelegraphSettings.GroundRatio (the level grids' 1 x 0.5 cells).
    /// </summary>
    public static class IsoGround
    {
        private static readonly List<Collider2D> colliders = new List<Collider2D>();

        public static float Ratio => AttackTelegraphSettings.Instance.GroundRatio;

        /// <summary>Where <paramref name="character"/> stands on the ground (see the class summary).</summary>
        public static Vector2 PointOf(Transform character)
        {
            if (character == null) return Vector2.zero;
            var body = BodyOf(character);
            return body != null ? PointOf(character, body.offset) : (Vector2)character.position;
        }

        /// <summary>A point at <paramref name="localOffset"/> from <paramref name="character"/>, scaled like its colliders.</summary>
        public static Vector2 PointOf(Transform character, Vector2 localOffset) =>
            (Vector2)character.position + Vector2.Scale(localOffset, character.lossyScale);

        /// <summary>Ground distance between two characters' ground points.</summary>
        public static float Distance(Transform a, Transform b) => IsoGroundArea.GroundDistance(PointOf(a), PointOf(b), Ratio);

        /// <summary>The solid body collider a ground point is taken from: the first enabled, non-trigger Collider2D on the root.</summary>
        public static Collider2D BodyOf(Transform character)
        {
            character.GetComponents(colliders);
            foreach (var collider in colliders)
            {
                if (collider.enabled && !collider.isTrigger) return collider;
            }
            return null;
        }
    }
}
