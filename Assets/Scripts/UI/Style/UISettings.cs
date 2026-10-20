using TMPro;
using UnityEngine;

namespace Game.UI.Style
{
    /// <summary>
    /// The UI standards in one place: the two font sizes, icon and button sizes, margins, the shared
    /// Grace / Corruption / Favors colors, the frame art and the gamepad selection frame.
    /// Elements opt in through the UIStyle* components (UIStyleText, UIStyleIcon, UIStyleButton,
    /// UIStyleFrame), which apply these values at runtime. After editing this asset, run
    /// Tools/Flames of Faith/UI Revamp/Re-apply UI Styles so the prefabs also show the new values in the Editor.
    /// Sizes are in canvas units at the 1920x1080 reference resolution.
    /// Loaded from Resources/UI/UISettings.
    /// </summary>
    [CreateAssetMenu(fileName = "UISettings", menuName = "Game/UI Settings")]
    public class UISettings : ScriptableObject
    {
        private const string ResourcePath = "UI/UISettings";

        [Header("Canvas")]
        [Tooltip("Every screen-space canvas scales with the screen from this resolution.")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [Tooltip("0 = match width, 1 = match height.")]
        [Range(0f, 1f)]
        [SerializeField] private float matchWidthOrHeight = 0.5f;

        [Header("Fonts")]
        [SerializeField] private TMP_FontAsset font;
        [Tooltip("Same font with a dark outline, for text drawn over bars or straight over gameplay.")]
        [SerializeField] private TMP_FontAsset outlinedFont;
        [Min(1f)]
        [SerializeField] private float smallFontSize = 12f;
        [Min(1f)]
        [SerializeField] private float bigFontSize = 20f;

        [Header("Icons")]
        [Min(1f)]
        [SerializeField] private float smallIconSize = 50f;
        [Min(1f)]
        [SerializeField] private float mediumIconSize = 80f;
        [Tooltip("Input glyphs (keys, gamepad buttons) shown next to or on top of other elements. They read like text, so they're sized to it, not to the icons.")]
        [Min(1f)]
        [SerializeField] private float promptIconSize = 28f;

        [Header("Buttons")]
        [Tooltip("Small buttons use the small font.")]
        [Min(1f)]
        [SerializeField] private float smallButtonHeight = 32f;
        [Min(1f)]
        [SerializeField] private float smallButtonMinWidth = 120f;
        [Tooltip("Big buttons use the big font.")]
        [Min(1f)]
        [SerializeField] private float bigButtonHeight = 48f;
        [Min(1f)]
        [SerializeField] private float bigButtonMinWidth = 200f;

        [Header("Layout")]
        [Tooltip("Space from the screen edges to every window, and from a window's frame to its content.")]
        [Min(0f)]
        [SerializeField] private float margin = 15f;
        [Tooltip("Space between elements inside a window.")]
        [Min(0f)]
        [SerializeField] private float spacing = 8f;

        [Header("Colors")]
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color mutedTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color graceColor = new Color(0.38f, 1f, 1f, 1f);
        [SerializeField] private Color corruptionColor = new Color(0.75f, 0.45f, 1f, 1f);
        [SerializeField] private Color favorsColor = new Color(1f, 0.85f, 0.35f, 1f);

        [Header("Frames")]
        [Tooltip("Decorated frame around windows and HUD panels (9-sliced).")]
        [SerializeField] private Sprite panelFrame;
        [Tooltip("Thin frame around bars (9-sliced).")]
        [SerializeField] private Sprite barFrame;
        [Tooltip("Plain bar fill in greys, tinted with the colors above (Grace, Corruption).")]
        [SerializeField] private Sprite barFill;
        [SerializeField] private Color frameTint = Color.white;
        [Tooltip("Higher = thinner frame borders. 1 = the art's own pixel size.")]
        [Min(0.01f)]
        [SerializeField] private float framePixelsPerUnitMultiplier = 1f;

        [Header("Gamepad selection")]
        [Tooltip("Frame drawn around the selected element while the gamepad is active (9-sliced).")]
        [SerializeField] private Sprite selectionFrame;
        [SerializeField] private Color selectionColor = new Color(1f, 0.9f, 0.6f, 1f);
        [Tooltip("Extra space around the selected element, per axis (canvas units). A UIWindow can override it.")]
        [SerializeField] private Vector2 selectionPadding = new Vector2(10f, 10f);
        [Tooltip("Pulses per second. 0 = no pulse.")]
        [Min(0f)]
        [SerializeField] private float selectionPulseSpeed = 1.5f;
        [Range(0f, 1f)]
        [SerializeField] private float selectionPulseMinAlpha = 0.6f;

        private static UISettings instance;

        public Vector2 ReferenceResolution => referenceResolution;
        public float MatchWidthOrHeight => matchWidthOrHeight;
        public TMP_FontAsset Font => font;
        public TMP_FontAsset OutlinedFont => outlinedFont != null ? outlinedFont : font;
        public float SmallFontSize => smallFontSize;
        public float BigFontSize => bigFontSize;
        public float SmallIconSize => smallIconSize;
        public float MediumIconSize => mediumIconSize;
        public float PromptIconSize => promptIconSize;
        public float Margin => margin;
        public float Spacing => spacing;
        public Color TextColor => textColor;
        public Color MutedTextColor => mutedTextColor;
        public Color GraceColor => graceColor;
        public Color CorruptionColor => corruptionColor;
        public Color FavorsColor => favorsColor;
        public Sprite PanelFrame => panelFrame;
        public Sprite BarFrame => barFrame;
        public Sprite BarFill => barFill;
        public Color FrameTint => frameTint;
        public float FramePixelsPerUnitMultiplier => framePixelsPerUnitMultiplier;
        public Sprite SelectionFrame => selectionFrame;
        public Color SelectionColor => selectionColor;
        public Vector2 SelectionPadding => selectionPadding;
        public float SelectionPulseSpeed => selectionPulseSpeed;
        public float SelectionPulseMinAlpha => selectionPulseMinAlpha;

        public static UISettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<UISettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"UISettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<UISettings>();
                    }
                }
                return instance;
            }
        }

        public float GetFontSize(UIFontSize size) => size == UIFontSize.Big ? bigFontSize : smallFontSize;

        public float GetIconSize(UIIconSize size)
        {
            switch (size)
            {
                case UIIconSize.Medium: return mediumIconSize;
                case UIIconSize.Prompt: return promptIconSize;
                default: return smallIconSize;
            }
        }

        public float GetButtonHeight(UIButtonSize size) => size == UIButtonSize.Big ? bigButtonHeight : smallButtonHeight;

        public float GetButtonMinWidth(UIButtonSize size) => size == UIButtonSize.Big ? bigButtonMinWidth : smallButtonMinWidth;

        public UIFontSize GetButtonFontSize(UIButtonSize size) => size == UIButtonSize.Big ? UIFontSize.Big : UIFontSize.Small;

        /// <summary>The color for a role. None returns the plain text color.</summary>
        public Color GetColor(UIColorRole role)
        {
            switch (role)
            {
                case UIColorRole.Muted: return mutedTextColor;
                case UIColorRole.Grace: return graceColor;
                case UIColorRole.Corruption: return corruptionColor;
                case UIColorRole.Favors: return favorsColor;
                default: return textColor;
            }
        }

        public Sprite GetFrameSprite(UIFrameKind kind) => kind == UIFrameKind.Bar ? barFrame : panelFrame;
    }
}
