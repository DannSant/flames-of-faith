using Game.Control;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.Navigation
{
    // One shared marker (frame, arrow...) that follows whatever UI element is currently selected,
    // so individual prefabs don't each need their own selection highlight. Only visible while the
    // gamepad is the active scheme - with the mouse, Unity's own hover states do the job.
    public class UISelectionMarkerUI : MonoBehaviour
    {
        [Tooltip("The visual that moves onto the selected element. Must be a child of a Canvas, pivot 0.5/0.5.")]
        [SerializeField] private RectTransform marker;
        [Tooltip("Default extra size around the selected element, in canvas units. A UIWindow can override this.")]
        [SerializeField] private Vector2 padding = new Vector2(8f, 8f);
        [Tooltip("Default offset. A UIWindow can override this.")]
        [SerializeField] private Vector2 offset = Vector2.zero;
        [Tooltip("Resize the marker to match the selected element. Turn off for a fixed-size marker like an arrow.")]
        [SerializeField] private bool matchSize = true;
        [Tooltip("0 = snap instantly. Higher values slide the marker to its target.")]
        [SerializeField] private float followSpeed = 20f;

        private RectTransform currentTarget;

        private void Awake()
        {
            if (marker != null) marker.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (marker == null) return;

            RectTransform target = ResolveTarget();
            if (target == null)
            {
                if (marker.gameObject.activeSelf) marker.gameObject.SetActive(false);
                currentTarget = null;
                return;
            }

            bool changedTarget = target != currentTarget;
            currentTarget = target;

            if (!marker.gameObject.activeSelf)
            {
                marker.gameObject.SetActive(true);
                changedTarget = true;
            }

            // Each window can tune how the marker sits on its own elements (rows, icons, buttons
            // all need different framing); otherwise the marker's own defaults are used.
            UIWindow window = target.GetComponentInParent<UIWindow>();
            bool useWindowSettings = window != null && window.OverrideMarkerSettings;
            Vector2 activePadding = useWindowSettings ? window.MarkerPadding : padding;
            Vector2 activeOffset = useWindowSettings ? window.MarkerOffset : offset;
            bool activeMatchSize = useWindowSettings ? window.MarkerMatchesSize : matchSize;

            if (activeMatchSize)
            {
                marker.sizeDelta = target.rect.size + activePadding;
            }

            Vector3 targetPosition = target.TransformPoint(target.rect.center) + (Vector3)activeOffset;
            // Time.unscaledDeltaTime: most of these windows open with the game paused.
            marker.position = (changedTarget || followSpeed <= 0f)
                ? targetPosition
                : Vector3.Lerp(marker.position, targetPosition, 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime));
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
