using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Style
{
    /// <summary>
    /// A decoration frame of the UI standard: the 9-sliced panel frame (windows, HUD panels) or bar frame.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIStyleFrame : UIStyleBase
    {
        [SerializeField] private UIFrameKind kind = UIFrameKind.Panel;

        public UIFrameKind Kind => kind;

        public void Configure(UIFrameKind kind)
        {
            this.kind = kind;
        }

        public override bool Apply(UISettings settings)
        {
            var image = GetComponent<Image>();
            if (image == null || settings == null) return false;

            bool changed = false;
            Sprite sprite = settings.GetFrameSprite(kind);
            if (sprite != null && image.sprite != sprite) { image.sprite = sprite; changed = true; }
            if (image.type != Image.Type.Sliced) { image.type = Image.Type.Sliced; changed = true; }
            if (!image.fillCenter) { image.fillCenter = true; changed = true; }
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, settings.FramePixelsPerUnitMultiplier))
            {
                image.pixelsPerUnitMultiplier = settings.FramePixelsPerUnitMultiplier;
                changed = true;
            }
            if (image.color != settings.FrameTint) { image.color = settings.FrameTint; changed = true; }
            return changed;
        }
    }
}
