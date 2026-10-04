using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Level
{
    /// <summary>
    /// Briefly moves the gameplay camera to a target and back: pans and zooms in, holds, then returns to whatever the
    /// camera was following. Uses a proxy transform as the follow target so the pan timing doesn't depend on the
    /// Cinemachine damping settings.
    /// </summary>
    public class CameraFocusController : MonoBehaviour
    {
        private CinemachineCamera cinemachineCamera;
        private Transform originalFollow;
        private float originalOrthoSize;
        private Transform proxy;
        private bool focusing;

        public bool IsFocusing => focusing;

        /// <summary>
        /// Runs the whole focus. Yield on it from a coroutine. zoomMultiplier scales the current orthographic size
        /// (0.7 = 30% closer).
        /// </summary>
        public IEnumerator FocusRoutine(Transform target, float zoomMultiplier, float panInDuration, float holdDuration, float panOutDuration)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            if (cinemachineCamera == null || target == null)
            {
                yield break;
            }

            focusing = true;
            originalFollow = cinemachineCamera.Follow;
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
            float zoomedSize = originalOrthoSize * zoomMultiplier;

            proxy = new GameObject("CameraFocusProxy").transform;
            Vector3 startPosition = originalFollow != null ? originalFollow.position : cinemachineCamera.transform.position;
            proxy.position = startPosition;
            cinemachineCamera.Follow = proxy;

            yield return Move(() => startPosition, () => target != null ? target.position : proxy.position,
                originalOrthoSize, zoomedSize, panInDuration);

            float held = 0f;
            while (held < holdDuration)
            {
                held += Time.deltaTime;
                if (target != null) proxy.position = target.position;
                yield return null;
            }

            Vector3 focusPosition = proxy.position;
            yield return Move(() => focusPosition, () => originalFollow != null ? originalFollow.position : startPosition,
                zoomedSize, originalOrthoSize, panOutDuration);

            Restore();
        }

        private IEnumerator Move(System.Func<Vector3> from, System.Func<Vector3> to, float fromSize, float toSize, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                proxy.position = Vector3.Lerp(from(), to(), t);
                cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(fromSize, toSize, t);
                yield return null;
            }
            proxy.position = to();
            cinemachineCamera.Lens.OrthographicSize = toSize;
        }

        /// <summary>
        /// Puts the camera back on its original follow target and zoom. Safe to call at any time
        /// (also used when the focus is interrupted).
        /// </summary>
        public void Restore()
        {
            if (!focusing)
            {
                return;
            }

            focusing = false;
            if (cinemachineCamera != null)
            {
                cinemachineCamera.Follow = originalFollow;
                cinemachineCamera.Lens.OrthographicSize = originalOrthoSize;
            }

            if (proxy != null)
            {
                Destroy(proxy.gameObject);
                proxy = null;
            }
        }

        private void OnDisable()
        {
            Restore();
        }
    }
}
