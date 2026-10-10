using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The triangle above the player's current target. Lives on a child of the enemy's
    /// world-space canvas; gameplay (WeaponManager) shows and hides it when the target changes.
    /// </summary>
    public class TargetMarkerUI : MonoBehaviour
    {
        [Tooltip("The object to show/hide. Must be a child, so this component keeps running while hidden.")]
        [SerializeField] private GameObject markerRoot;
        [Tooltip("How far the marker bobs up and down, in canvas units.")]
        [SerializeField] private float bobAmplitude = 0.05f;
        [Tooltip("Bob cycles per second.")]
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 restPosition;

        private void Awake()
        {
            if (markerRoot == null)
            {
                Debug.LogWarning($"{nameof(TargetMarkerUI)} on {name} has no marker root assigned.");
                return;
            }
            restPosition = markerRoot.transform.localPosition;
            markerRoot.SetActive(false);
        }

        private void Update()
        {
            if (markerRoot == null || !markerRoot.activeSelf) return;

            float offset = Mathf.Sin(Time.time * bobSpeed * 2f * Mathf.PI) * bobAmplitude;
            markerRoot.transform.localPosition = restPosition + Vector3.up * offset;
        }

        public void Show()
        {
            if (markerRoot != null) markerRoot.SetActive(true);
        }

        public void Hide()
        {
            if (markerRoot != null) markerRoot.SetActive(false);
        }
    }
}
