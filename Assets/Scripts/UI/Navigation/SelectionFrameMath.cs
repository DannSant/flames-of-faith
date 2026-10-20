using UnityEngine;

namespace Game.UI.Navigation
{
    /// <summary>
    /// Pure math for the gamepad selection frame: the rect that wraps a selected element, in the
    /// local space of the frame's parent.
    /// </summary>
    public static class SelectionFrameMath
    {
        /// <summary>
        /// Bounds of the given points (the element's corners, already converted to the frame parent's
        /// local space) grown by padding on each axis (half on each side). Returns center and size.
        /// </summary>
        public static Rect ComputeRect(Vector2[] localCorners, Vector2 padding)
        {
            if (localCorners == null || localCorners.Length == 0) return new Rect(Vector2.zero, padding);

            Vector2 min = localCorners[0];
            Vector2 max = localCorners[0];
            for (int i = 1; i < localCorners.Length; i++)
            {
                min = Vector2.Min(min, localCorners[i]);
                max = Vector2.Max(max, localCorners[i]);
            }

            Vector2 size = max - min + padding;
            Vector2 center = (min + max) * 0.5f;
            return new Rect(center - size * 0.5f, size);
        }

        /// <summary>Frame alpha for a slow pulse between minAlpha and 1. speed 0 = always 1.</summary>
        public static float PulseAlpha(float time, float speed, float minAlpha)
        {
            if (speed <= 0f) return 1f;
            float wave = 0.5f + 0.5f * Mathf.Cos(time * speed * 2f * Mathf.PI);
            return Mathf.Lerp(Mathf.Clamp01(minAlpha), 1f, wave);
        }
    }
}
