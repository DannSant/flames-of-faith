using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Style
{
    /// <summary>
    /// An icon of the UI standard: Small, Medium, or Prompt (input glyphs). Sets a square size on the
    /// RectTransform, or on the LayoutElement when a layout group places the icon.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIStyleIcon : UIStyleBase
    {
        [SerializeField] private UIIconSize size = UIIconSize.Small;

        public UIIconSize Size => size;

        public void Configure(UIIconSize size)
        {
            this.size = size;
        }

        public override bool Apply(UISettings settings)
        {
            if (settings == null) return false;
            return ApplySquareSize((RectTransform)transform, settings.GetIconSize(size));
        }

        internal static bool ApplySquareSize(RectTransform rect, float side)
        {
            bool changed = false;

            var layout = rect.GetComponent<LayoutElement>();
            if (layout != null)
            {
                changed |= SetLayout(layout, side, side);
            }

            // Only fixed-size rects: a stretched rect's size comes from its parent
            if (rect.anchorMin == rect.anchorMax)
            {
                var target = new Vector2(side, side);
                if (rect.sizeDelta != target)
                {
                    rect.sizeDelta = target;
                    changed = true;
                }
            }
            return changed;
        }

        internal static bool SetLayout(LayoutElement layout, float width, float height)
        {
            bool changed = false;
            if (!Mathf.Approximately(layout.preferredWidth, width)) { layout.preferredWidth = width; changed = true; }
            if (!Mathf.Approximately(layout.preferredHeight, height)) { layout.preferredHeight = height; changed = true; }
            if (!Mathf.Approximately(layout.minWidth, width)) { layout.minWidth = width; changed = true; }
            if (!Mathf.Approximately(layout.minHeight, height)) { layout.minHeight = height; changed = true; }
            return changed;
        }
    }
}
