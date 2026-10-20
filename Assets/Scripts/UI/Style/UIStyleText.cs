using TMPro;
using UnityEngine;

namespace Game.UI.Style
{
    /// <summary>
    /// A text of the UI standard: Small or Big size, the standard font (or its outlined version)
    /// and optionally one of the shared colors. Leave the color role on None for texts a script colors.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class UIStyleText : UIStyleBase
    {
        [SerializeField] private UIFontSize size = UIFontSize.Small;
        [Tooltip("Use the outlined font, for text over bars or straight over gameplay.")]
        [SerializeField] private bool outlined = false;
        [Tooltip("None: the color is left alone (set by a script or by hand).")]
        [SerializeField] private UIColorRole color = UIColorRole.None;

        public UIFontSize Size => size;
        public bool Outlined => outlined;
        public UIColorRole ColorRole => color;

        public void Configure(UIFontSize size, bool outlined, UIColorRole color)
        {
            this.size = size;
            this.outlined = outlined;
            this.color = color;
        }

        public override bool Apply(UISettings settings)
        {
            var text = GetComponent<TMP_Text>();
            if (text == null || settings == null) return false;

            bool changed = false;

            TMP_FontAsset font = outlined ? settings.OutlinedFont : settings.Font;
            if (font != null && text.font != font)
            {
                text.font = font;
                changed = true;
            }

            float fontSize = settings.GetFontSize(size);
            if (text.enableAutoSizing)
            {
                text.enableAutoSizing = false;
                changed = true;
            }
            if (!Mathf.Approximately(text.fontSize, fontSize))
            {
                text.fontSize = fontSize;
                changed = true;
            }

            // A vertex gradient multiplies whatever color is set, so standard colors would never show as set
            if (text.enableVertexGradient)
            {
                text.enableVertexGradient = false;
                changed = true;
            }

            if (color != UIColorRole.None)
            {
                Color target = settings.GetColor(color);
                if (text.color != target)
                {
                    text.color = target;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
