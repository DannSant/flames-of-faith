using UnityEngine;

namespace Game.UI.Navigation
{
    /// <summary>
    /// Pure math for keeping a gamepad-selected item visible in a vertical ScrollRect. Distances are measured
    /// down from the content's top; normalized position 1 = scrolled to the top, 0 = to the bottom
    /// (ScrollRect.verticalNormalizedPosition).
    /// </summary>
    public static class ScrollRectMath
    {
        /// <summary>
        /// The normalized position that shows the item [itemTop, itemBottom] fully (plus padding), moving as
        /// little as possible: unchanged when it's already visible. Content that fits the viewport stays at the top.
        /// </summary>
        public static float NormalizedPositionToShow(float contentHeight, float viewportHeight,
            float itemTop, float itemBottom, float currentNormalized, float padding = 0f)
        {
            float scrollRange = contentHeight - viewportHeight;
            if (scrollRange <= 0f) return 1f;

            float offset = (1f - Mathf.Clamp01(currentNormalized)) * scrollRange; // pixels scrolled from the top
            float wantedTop = itemTop - padding;
            float wantedBottom = itemBottom + padding;

            if (wantedTop < offset) offset = wantedTop;
            else if (wantedBottom > offset + viewportHeight) offset = wantedBottom - viewportHeight;

            offset = Mathf.Clamp(offset, 0f, scrollRange);
            return 1f - offset / scrollRange;
        }
    }
}
