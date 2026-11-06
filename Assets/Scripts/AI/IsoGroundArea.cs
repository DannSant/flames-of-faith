using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// A circle lying on the isometric ground, as it's seen on screen: an ellipse <c>groundRatio</c> times as tall
    /// as it is wide (the level grids' cells are 1 x 0.5, so 0.5). A melee attack's warning and its hitbox are the
    /// same ellipse, from these functions, so the hit area is exactly what's drawn. Pure: the ratio is passed in
    /// (AttackTelegraphSettings.GroundRatio).
    /// </summary>
    public static class IsoGroundArea
    {
        /// <summary>Width and height on screen of a ground circle of <paramref name="radius"/>.</summary>
        public static Vector2 Size(float radius, float groundRatio) => new Vector2(2f * radius, 2f * radius * groundRatio);

        /// <summary>Whether <paramref name="point"/> is inside the ground circle of <paramref name="radius"/> at <paramref name="center"/> (edge included).</summary>
        public static bool Contains(Vector2 center, float radius, float groundRatio, Vector2 point)
        {
            if (radius <= 0f || groundRatio <= 0f) return false;
            Vector2 d = point - center;
            float x = d.x / radius;
            float y = d.y / (radius * groundRatio);
            return x * x + y * y <= 1f + 1e-5f;
        }

        /// <summary>
        /// The point <paramref name="distance"/> away on the ground in the screen direction <paramref name="direction"/>
        /// (e.g. toward the player): it stays in that direction on screen, but its vertical reach is shortened by the
        /// ratio, like everything lying on the ground. Without that the area drifts above or below a north / south target.
        /// </summary>
        public static Vector2 GroundOffset(Vector2 direction, float distance, float groundRatio)
        {
            if (direction.sqrMagnitude <= 0.0001f || groundRatio <= 0f) return Vector2.zero;
            // To ground space (undo the squash), take the distance there, and back to the screen
            Vector2 ground = new Vector2(direction.x, direction.y / groundRatio).normalized * distance;
            return new Vector2(ground.x, ground.y * groundRatio);
        }

        /// <summary>The ellipse's outline as <paramref name="segments"/> points, counter-clockwise from the right, around <paramref name="center"/>.</summary>
        public static Vector2[] Outline(Vector2 center, float radius, float groundRatio, int segments)
        {
            segments = Mathf.Max(3, segments);
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                points[i] = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * groundRatio);
            }
            return points;
        }
    }
}
