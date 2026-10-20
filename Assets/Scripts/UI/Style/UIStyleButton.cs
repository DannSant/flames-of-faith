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

            changed |= ApplyArt(settings);

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

        // The standard button art, 9-sliced so any width keeps its bevel. Pressed and disabled (an unfocused
        // gamepad window) show the darker sprite.
        private bool ApplyArt(UISettings settings)
        {
            var button = GetComponent<Button>();
            if (button == null || settings.ButtonSprite == null) return false;

            bool changed = false;
            if (button.targetGraphic is Image image)
            {
                if (image.sprite != settings.ButtonSprite) { image.sprite = settings.ButtonSprite; changed = true; }
                if (image.type != Image.Type.Sliced) { image.type = Image.Type.Sliced; changed = true; }
                if (image.preserveAspect) { image.preserveAspect = false; changed = true; }
            }

            if (button.transition != Selectable.Transition.SpriteSwap) { button.transition = Selectable.Transition.SpriteSwap; changed = true; }
            Sprite pressed = settings.ButtonPressedSprite != null ? settings.ButtonPressedSprite : settings.ButtonSprite;
            var state = new SpriteState
            {
                highlightedSprite = settings.ButtonSprite,
                selectedSprite = settings.ButtonSprite,
                pressedSprite = pressed,
                disabledSprite = pressed,
            };
            if (!button.spriteState.Equals(state)) { button.spriteState = state; changed = true; }
            return changed;
        }
    }
}
