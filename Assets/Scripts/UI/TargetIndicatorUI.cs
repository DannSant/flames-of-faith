using Game.Waves;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Arrow on the edge of the screen pointing at a target while it is off-screen (currently the Corruptor).
    /// Place it on a RectTransform inside a Screen Space - Overlay canvas.
    /// </summary>
    public class TargetIndicatorUI : MonoBehaviour
    {
        [Tooltip("Moved to the edge of the screen. Hidden while there is no target.")]
        [SerializeField] private RectTransform indicatorRoot;
        [Tooltip("Rotated to point at the target. Can be the root itself.")]
        [SerializeField] private RectTransform arrow;
        [Tooltip("Distance in pixels from the screen edge.")]
        [SerializeField] private float edgePadding = 48f;
        [Tooltip("Rotation offset for the arrow sprite (0 if the sprite points right, -90 if it points up).")]
        [SerializeField] private float arrowRotationOffset = 0f;
        [Tooltip("Hide the indicator while the target is visible on screen.")]
        [SerializeField] private bool hideWhenOnScreen = true;

        private Transform target;
        private Camera mainCamera;

        private void Start()
        {
            if (indicatorRoot != null && indicatorRoot.gameObject == gameObject)
            {
                Debug.LogError("TargetIndicatorUI: indicatorRoot must be a child object, otherwise hiding it also disables this script.");
            }
            SetVisible(false);
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnCorruptorSpawned += SetTarget;
                WaveSpawner.Instance.OnCorruptorPhaseEnded += ClearTarget;
            }
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnCorruptorSpawned -= SetTarget;
                WaveSpawner.Instance.OnCorruptorPhaseEnded -= ClearTarget;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void ClearTarget()
        {
            target = null;
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                SetVisible(false);
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.position);
            if (screenPoint.z < 0f)
            {
                screenPoint *= -1f;
            }

            bool onScreen = screenPoint.x >= 0f && screenPoint.x <= Screen.width
                && screenPoint.y >= 0f && screenPoint.y <= Screen.height;

            if (onScreen && hideWhenOnScreen)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 direction = (Vector2)screenPoint - center;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.up;
            }

            Vector2 position = (Vector2)screenPoint;
            if (!onScreen)
            {
                // Scale the direction so it touches the padded screen rectangle
                float halfWidth = Mathf.Max(1f, center.x - edgePadding);
                float halfHeight = Mathf.Max(1f, center.y - edgePadding);
                float scaleX = Mathf.Abs(direction.x) > 0.001f ? halfWidth / Mathf.Abs(direction.x) : float.MaxValue;
                float scaleY = Mathf.Abs(direction.y) > 0.001f ? halfHeight / Mathf.Abs(direction.y) : float.MaxValue;
                position = center + direction * Mathf.Min(scaleX, scaleY);
            }

            indicatorRoot.position = position;
            if (arrow != null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + arrowRotationOffset;
                arrow.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void SetVisible(bool visible)
        {
            if (indicatorRoot != null && indicatorRoot.gameObject.activeSelf != visible)
            {
                indicatorRoot.gameObject.SetActive(visible);
            }
        }
    }
}
