using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Style
{
    /// <summary>
    /// A button of the UI standard: Small (small font) or Big (big font). Sets the height and the
    /// minimum width, and the size of its label (the first text in its children).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIStyleButton : UIStyleBase
    {
        [SerializeField] private UIButtonSize size = UIButtonSize.Small;

        public UIButtonSize Size => size;

        public void Configure(UIButtonSize size)
        {
            this.size = size;
        }

        public override bool Apply(UISettings settings)
        {
            if (settings == null) return false;

            bool changed = false;
            var rect = (RectTransform)transform;
            float height = settings.GetButtonHeight(size);
            float minWidth = settings.GetButtonMinWidth(size);

            var layout = GetComponent<LayoutElement>();
            if (layout != null)
            {
                if (!Mathf.Approximately(layout.preferredHeight, height)) { layout.preferredHeight = height; changed = true; }
                if (!Mathf.Approximately(layout.minHeight, height)) { layout.minHeight = height; changed = true; }
                if (layout.minWidth < minWidth) { layout.minWidth = minWidth; changed = true; }
            }

            Vector2 sizeDelta = rect.sizeDelta;
            if (rect.anchorMin.y == rect.anchorMax.y) sizeDelta.y = height;
            if (rect.anchorMin.x == rect.anchorMax.x) sizeDelta.x = Mathf.Max(sizeDelta.x, minWidth);
            if (sizeDelta != rect.sizeDelta)
            {
                rect.sizeDelta = sizeDelta;
                changed = true;
            }

            var label = GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                // A label with its own text style keeps it; otherwise it follows the button size
                var labelStyle = label.GetComponent<UIStyleText>();
                if (labelStyle != null)
                {
                    changed |= labelStyle.Apply(settings);
                }
                else
                {
                    float fontSize = settings.GetFontSize(settings.GetButtonFontSize(size));
                    if (settings.Font != null && label.font != settings.Font) { label.font = settings.Font; changed = true; }
                    if (label.enableAutoSizing) { label.enableAutoSizing = false; changed = true; }
                    if (!Mathf.Approximately(label.fontSize, fontSize)) { label.fontSize = fontSize; changed = true; }
                }
            }

            return changed;
        }
    }
}
