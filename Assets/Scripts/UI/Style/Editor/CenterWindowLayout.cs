using Game.UI.Navigation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Style.Editor.UIBuildHelpers;

namespace Game.UI.Style.Editor
{
    /// <summary>
    /// The shared layout of the center windows (level-up, pause, death screen, settings), at the 1920x1080
    /// reference: the Inventory's height and vertical position, so the windows line up with the Inventory
    /// on the left and the Stats pane on the right; a centered Big bold title; and the "progress" button
    /// (Continue, Resume, Retry, Back) in the same bottom-center spot in every window. Other buttons stack
    /// above it.
    /// </summary>
    public static class CenterWindowLayout
    {
        public const float Width = 400f;
        public const float ProgressButtonWidth = 160f;

        public static Vector2 WindowSize(UISettings s, float width = Width) => new Vector2(width, InventoryRestyler.PaneSize(s).y);

        /// <summary>Centered horizontally, at the Inventory's height and vertical position.</summary>
        public static Vector2 WindowPosition(UISettings s) => new Vector2(0f, s.Margin);

        public static void PlaceWindow(RectTransform window, UISettings s, float width = Width)
        {
            Place(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WindowPosition(s), WindowSize(s, width));
        }

        /// <summary>The frame image of the window: the standard Panel frame, stretched over the window.</summary>
        public static void FrameWindow(RectTransform panel, UISettings s)
        {
            StretchFull(panel);
            var image = GetOrAdd<Image>(panel.gameObject);
            image.enabled = true;
            image.raycastTarget = true;
            StyleFrame(image, UIFrameKind.Panel, s);
        }

        /// <summary>Title row at the top, centered, Big and bold (Display for headline exceptions).</summary>
        public static void PlaceTitle(TextMeshProUGUI title, UISettings s, UIColorRole color, UIFontSize size = UIFontSize.Big)
        {
            float height = size == UIFontSize.Display ? s.DisplayFontSize + 8f : InventoryRestyler.TitleRowHeight;
            Stretch(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f));
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -s.Margin);
            title.rectTransform.sizeDelta = new Vector2(-2f * s.Margin, height);
            PlainText(title, TextAlignmentOptions.Center);
            DisableLayout(title.gameObject);
            StyleText(title, size, false, color, s, bold: true);
        }

        /// <summary>
        /// A Small button stacked from the bottom: slot 0 is the progress button's spot (bottom center,
        /// the margin up), slot 1 is just above it, and so on.
        /// </summary>
        public static void PlaceStackedButton(RectTransform button, UISettings s, int slot)
        {
            float height = s.GetButtonHeight(UIButtonSize.Small);
            float y = s.Margin + slot * (height + s.Spacing);
            Place(button, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(ProgressButtonWidth, height));
            DisableLayout(button.gameObject);
            StyleSmallButton(button, s);
        }

        /// <summary>Top of the space left between the title rows and the stacked buttons (from the window top).</summary>
        public static float ButtonsTop(UISettings s, int buttonCount)
        {
            float height = s.GetButtonHeight(UIButtonSize.Small);
            return s.Margin + buttonCount * height + (buttonCount - 1) * s.Spacing;
        }

        public static void StyleSmallButton(RectTransform button, UISettings s)
        {
            var style = GetOrAdd<UIStyleButton>(button.gameObject);
            style.Configure(UIButtonSize.Small);
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                PlainText(label, TextAlignmentOptions.Center);
                StyleText(label, UIFontSize.Small, false, UIColorRole.Text, s);
            }
            style.Apply(s);
        }

        /// <summary>The window no longer needs the arrow-era marker padding override.</summary>
        public static void UseStandardMarker(UIWindow window)
        {
            if (window == null) return;
            var so = new SerializedObject(window);
            so.FindProperty("overrideMarkerSettings").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void PlainText(TMP_Text text, TextAlignmentOptions alignment)
        {
            text.alignment = alignment;
            text.characterSpacing = 0f;
            text.margin = Vector4.zero;
            text.enableAutoSizing = false;
        }

        // Elements placed by hand must not be moved by a leftover layout group on their parent
        private static void DisableLayout(GameObject go)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element != null) element.ignoreLayout = true;
        }
    }
}
