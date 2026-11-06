using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    /// <summary>
    /// Pure math for the pilgrim walking along a map path: the path between two nodes (an edge's drawn points,
    /// reversed when walking it backwards), the position and heading at a distance along it, how long the walk
    /// takes (speed, capped so long edges don't drag) and which way the pilgrim faces.
    /// </summary>
    public static class MapWalk
    {
        /// <summary>
        /// The points to walk from <paramref name="from"/> to <paramref name="to"/> along a drawn edge: as drawn when
        /// it starts nearer <paramref name="from"/>, else reversed. Without points, a straight line.
        /// </summary>
        public static List<Vector2> PathBetween(IReadOnlyList<Vector2> edgePoints, Vector2 from, Vector2 to)
        {
            var path = new List<Vector2>();
            if (edgePoints == null || edgePoints.Count < 2)
            {
                path.Add(from);
                path.Add(to);
                return path;
            }

            bool forward = (edgePoints[0] - from).sqrMagnitude <= (edgePoints[edgePoints.Count - 1] - from).sqrMagnitude;
            for (int i = 0; i < edgePoints.Count; i++)
            {
                path.Add(edgePoints[forward ? i : edgePoints.Count - 1 - i]);
            }
            return path;
        }

        public static float Length(IReadOnlyList<Vector2> path)
        {
            float length = 0f;
            for (int i = 1; i < (path?.Count ?? 0); i++) length += Vector2.Distance(path[i - 1], path[i]);
            return length;
        }

        /// <summary>The point <paramref name="distance"/> along the path, clamped to its ends.</summary>
        public static Vector2 PointAt(IReadOnlyList<Vector2> path, float distance)
        {
            if (path == null || path.Count == 0) return Vector2.zero;
            if (distance <= 0f) return path[0];
            for (int i = 1; i < path.Count; i++)
            {
                float segment = Vector2.Distance(path[i - 1], path[i]);
                if (distance <= segment && segment > 0f) return Vector2.Lerp(path[i - 1], path[i], distance / segment);
                distance -= segment;
            }
            return path[path.Count - 1];
        }

        /// <summary>The direction of travel <paramref name="distance"/> along the path (its segment's), or zero on a path with no length.</summary>
        public static Vector2 DirectionAt(IReadOnlyList<Vector2> path, float distance)
        {
            if (path == null || path.Count < 2) return Vector2.zero;
            Vector2 last = Vector2.zero;
            for (int i = 1; i < path.Count; i++)
            {
                Vector2 delta = path[i] - path[i - 1];
                float segment = delta.magnitude;
                if (segment <= 0f) continue;
                last = delta / segment;
                if (distance <= segment) return last;
                distance -= segment;
            }
            return last;
        }

        /// <summary>Seconds to walk <paramref name="length"/> at <paramref name="speed"/>, never more than <paramref name="maxDuration"/>.</summary>
        public static float Duration(float length, float speed, float maxDuration)
        {
            if (length <= 0f) return 0f;
            float duration = speed > 0f ? length / speed : maxDuration;
            return maxDuration > 0f ? Mathf.Min(duration, maxDuration) : duration;
        }

        /// <summary>The facing for the animator's DirectionX / DirectionY: the direction of travel, or the previous facing when standing still.</summary>
        public static Vector2 Facing(Vector2 direction, Vector2 previous) =>
            direction.sqrMagnitude > 0.0001f ? direction.normalized : previous;
    }
}
