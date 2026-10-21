using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Pure math for the shared tooltip's position: beside the anchor (the hovered element, the gamepad
    /// selection or the pointer), on the side with room, top-aligned with it, and kept inside the bounds.
    /// All rects are in the same space with y up; the result is the pane's top-left corner.
    /// </summary>
    public static class TooltipPlacement
    {
        public static Vector2 TopLeft(Rect anchor, Vector2 paneSize, Rect bounds, float margin, float gap)
        {
            float minX = bounds.xMin + margin;
            float maxX = bounds.xMax - margin - paneSize.x;

            // Right of the anchor when it fits, otherwise left; if neither fits, the side with more room
            float right = anchor.xMax + gap;
            float left = anchor.xMin - gap - paneSize.x;
            float x;
            if (right <= maxX) x = right;
            else if (left >= minX) x = left;
            else x = (bounds.xMax - anchor.xMax) >= (anchor.xMin - bounds.xMin) ? right : left;
            x = maxX < minX ? minX : Mathf.Clamp(x, minX, maxX);

            // Top-aligned with the anchor, pushed up if it would run off the bottom
            float top = anchor.yMax;
            float lowestTop = bounds.yMin + margin + paneSize.y;
            float highestTop = bounds.yMax - margin;
            top = Mathf.Max(top, lowestTop);
            top = Mathf.Min(top, highestTop);

            return new Vector2(x, top);
        }
    }
}
