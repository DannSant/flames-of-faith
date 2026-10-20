using Game.Control;
using Game.UI.Style;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Navigation
{
    // One shared frame that wraps whatever UI element is currently selected, so individual prefabs
    // don't each need their own selection highlight. Only visible while the gamepad is the active
    // scheme - with the mouse, Unity's own hover states do the job.
    // The frame is placed in its parent's local space from the element's corners, so it fits the
    // element at any canvas scale and on Screen Space Camera canvases too. Its own Canvas (override
    // sorting) draws it above every window regardless of its place in the hierarchy.
    public class UISelectionMarkerUI : MonoBehaviour
    {
        [Tooltip("The frame that wraps the selected element. Child of this object, pivot 0.5/0.5. Its look (sprite, color, " +
                 "padding, pulse) comes from UISettings > Gamepad selection.")]
        [SerializeField] private RectTransform marker;
        [Tooltip("Optional. The frame's Image, for the color and pulse.")]
        [SerializeField] private Image markerImage;
        [Tooltip("0 = snap instantly. Higher values slide the frame from one element to the next.")]
        [SerializeField] private float followSpeed = 20f;

        private readonly Vector3[] worldCorners = new Vector3[4];
        private readonly Vector2[] localCorners = new Vector2[4];
        private RectTransform currentTarget;

        private void Awake()
        {
            if (marker != null) marker.gameObject.SetActive(false);
            if (markerImage != null) markerImage.raycastTarget = false;
        }

        private void LateUpdate()
        {
            if (marker == null) return;

            RectTransform target = ResolveTarget();
            var parent = marker.parent as RectTransform;
            if (target == null || parent == null)
            {
                if (marker.gameObject.activeSelf) marker.gameObject.SetActive(false);
                currentTarget = null;
                return;
            }

            // Snap when the frame appears; slide when it moves between elements
            bool snap = !marker.gameObject.activeSelf || followSpeed <= 0f;
            if (!marker.gameObject.activeSelf) marker.gameObject.SetActive(true);
            currentTarget = target;

            UISettings settings = UISettings.Instance;
            UIWindow window = target.GetComponentInParent<UIWindow>();
            Vector2 padding = window != null && window.OverrideMarkerSettings ? window.MarkerPadding : settings.SelectionPadding;

            target.GetWorldCorners(worldCorners);
            for (int i = 0; i < 4; i++)
            {
                localCorners[i] = parent.InverseTransformPoint(worldCorners[i]);
            }
            Rect rect = SelectionFrameMath.ComputeRect(localCorners, padding);

            marker.anchorMin = marker.anchorMax = new Vector2(0.5f, 0.5f);
            marker.pivot = new Vector2(0.5f, 0.5f);
            if (snap)
            {
                marker.localPosition = rect.center;
                marker.sizeDelta = rect.size;
            }
            else
            {
                // Time.unscaledDeltaTime: most of these windows open with the game paused.
                float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
                marker.localPosition = Vector3.Lerp(marker.localPosition, rect.center, t);
                marker.sizeDelta = Vector2.Lerp(marker.sizeDelta, rect.size, t);
            }

            if (markerImage != null)
            {
                Color color = settings.SelectionColor;
                color.a *= SelectionFrameMath.PulseAlpha(Time.unscaledTime, settings.SelectionPulseSpeed, settings.SelectionPulseMinAlpha);
                markerImage.color = color;
            }
        }

        private RectTransform ResolveTarget()
        {
            if (InputDeviceManager.Instance == null || !InputDeviceManager.Instance.IsGamepadActive) return null;

            var eventSystem = EventSystem.current;
            if (eventSystem == null) return null;

            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected == null || !selected.activeInHierarchy) return null;

            return selected.transform as RectTransform;
        }
    }
}
