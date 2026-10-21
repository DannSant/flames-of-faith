using Game.Common;
using Game.Control;
using Game.GameSettings;
using Game.UI.Style;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The shared tooltip. One per scene; windows find it and call ShowTooltip / HideTooltip.
    /// - Title on top; on the left the main text (General) and the extra details (Detailed: stat sources,
    ///   benefit math, Now/After); on the right an optional vertical list (an item's stat lines).
    ///   Empty parts hide, and the pane's height follows its content.
    /// - It appears beside what it describes: the given anchor, else the gamepad selection, else the
    ///   pointer, on the side with room and kept on screen (TooltipPlacement). It never takes raycasts, so
    ///   it can't steal the pointer from the element it describes (no flicker).
    /// </summary>
    public class GeneralTooltipPaneUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [Tooltip("Main text (GeneralTooltipTxt): item and stat descriptions.")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Detailed text")]
        [Tooltip("Extra details (DetailedTooltipTxt): stat sources, benefit math, next level. Hidden when empty.")]
        [FormerlySerializedAs("sourcesText")]
        [SerializeField] private TextMeshProUGUI detailText;

        [Header("Side list")]
        [Tooltip("Right column for vertical lists (an item's stat lines). Hidden when empty.")]
        [SerializeField] private GameObject listSection;
        [SerializeField] private TextMeshProUGUI listText;

        [Header("Placement")]
        [Tooltip("Space between the tooltip and what it describes.")]
        [SerializeField] private float anchorGap = 12f;
        [Tooltip("Size of the area around the mouse pointer the tooltip keeps clear of.")]
        [SerializeField] private Vector2 pointerSize = new Vector2(24f, 24f);

        private readonly Vector3[] corners = new Vector3[4];

        private void Awake()
        {
            mainPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.onPauseToggled += HandlePauseToggled;
            }
        }

        private void OnDisable()
        {
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.onPauseToggled -= HandlePauseToggled;
            }
        }

        private void HandlePauseToggled(bool isPaused)
        {
            // The panels that trigger tooltips (stats, inventory, etc.) get hidden on
            // pause/unpause without firing OnPointerExit, so the tooltip must be
            // force-closed on every pause transition to avoid getting stuck on screen.
            HideTooltip();
        }

        /// <param name="description">Main text.</param>
        /// <param name="title">Title (rich text, e.g. a quality-colored item name).</param>
        /// <param name="detail">Extra details under the main text (stat sources, benefit, Now/After).</param>
        /// <param name="list">Vertical list for the right column (an item's stat lines).</param>
        /// <param name="anchor">What the tooltip describes; by default the gamepad selection or the pointer.</param>
        public void ShowTooltip(string description, string title = "", string detail = null, string list = null, RectTransform anchor = null)
        {
            mainPanel.SetActive(true);
            SetText(titleText, title);
            SetText(descriptionText, description);
            SetText(detailText, detail);

            bool hasList = !string.IsNullOrEmpty(list);
            if (listSection != null) listSection.SetActive(hasList);
            if (listText != null) listText.SetText(hasList ? list : "");

            Place(anchor);
        }

        public void HideTooltip()
        {
            mainPanel.SetActive(false);
            SetText(titleText, null);
            SetText(descriptionText, null);
            SetText(detailText, null);
            if (listText != null) listText.SetText("");
            if (listSection != null) listSection.SetActive(false);
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text == null) return;
            bool show = !string.IsNullOrEmpty(value);
            text.SetText(show ? value : "");
            text.gameObject.SetActive(show);
        }

        // --- Placement ------------------------------------------------------------------------------------

        private void Place(RectTransform anchor)
        {
            var root = transform as RectTransform;
            var panel = mainPanel.transform as RectTransform;
            if (root == null || panel == null) return;

            // The real size first: the height follows the content
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

            Rect anchorRect = ResolveAnchor(anchor, root);
            float margin = UISettings.Instance.Margin;
            Vector2 topLeft = TooltipPlacement.TopLeft(anchorRect, panel.rect.size, root.rect, margin, anchorGap);

            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0f, 1f);
            panel.localPosition = new Vector3(topLeft.x, topLeft.y, 0f);
        }

        // The described element's rect in the root's space: the given anchor, the gamepad selection, or the pointer
        private Rect ResolveAnchor(RectTransform anchor, RectTransform root)
        {
            if (anchor == null && InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive)
            {
                var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                anchor = selected != null ? selected.transform as RectTransform : null;
            }

            if (anchor != null)
            {
                anchor.GetWorldCorners(corners);
                Vector2 min = root.InverseTransformPoint(corners[0]);
                Vector2 max = min;
                for (int i = 1; i < 4; i++)
                {
                    Vector2 local = root.InverseTransformPoint(corners[i]);
                    min = Vector2.Min(min, local);
                    max = Vector2.Max(max, local);
                }
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }

            if (Mouse.current != null)
            {
                var canvas = GetComponentInParent<Canvas>();
                Camera cam = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.rootCanvas.worldCamera
                    : null;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(root, Mouse.current.position.ReadValue(), cam, out Vector2 pointer))
                {
                    // The pointer's tip is the rect's top-left: the tooltip goes beside and below it
                    return new Rect(pointer.x, pointer.y - pointerSize.y, pointerSize.x, pointerSize.y);
                }
            }

            return new Rect(root.rect.center, Vector2.zero);
        }
    }
}
