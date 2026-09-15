using Game.Common;
using UnityEngine;

namespace Game.Control
{
    // In-world aim indicator shown only while the gamepad is the active input scheme.
    // Expected hierarchy (this component must sit on an object that is never deactivated,
    // otherwise OnDisable would unsubscribe it and it could never turn itself back on):
    //   AimIndicator (this)
    //    └ IsoPlane   (visualRoot)  - flattened by isoYScale, never rotated
    //       └ Pivot   (pivot)       - rotates on local Z only; circle sprite
    //          └ Triangle           - points up at rotation 0
    // Splitting "flatten" and "spin" across two transforms keeps the circle a constant
    // isometric ellipse while the triangle slides around its rim.
    public class PlayerAimIndicator : MonoBehaviour, IMapComponentDisabler
    {
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Transform pivot;
        [Tooltip("Vertical flatten of the ground plane. 0.5 matches the 1 x 0.5 isometric tilemap cell.")]
        [SerializeField] private float isoYScale = 0.5f;

        private CharacterVisual characterVisual;
        private bool hiddenOnMap = false;
        private bool subscribed = false;

        private void Awake()
        {
            characterVisual = transform.parent != null
                ? transform.parent.GetComponentInChildren<CharacterVisual>()
                : GetComponentInParent<CharacterVisual>();

            if (visualRoot != null)
            {
                Vector3 scale = visualRoot.transform.localScale;
                visualRoot.transform.localScale = new Vector3(scale.x, isoYScale, scale.z);
            }
        }

        private void OnEnable()
        {
            TrySubscribe();
            RefreshVisibility();
        }

        private void Start()
        {
            // InputDeviceManager may not exist yet when this first enables, depending on scene load order.
            TrySubscribe();
            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (subscribed && InputDeviceManager.Instance != null)
            {
                InputDeviceManager.Instance.OnInputSchemeChanged -= HandleInputSchemeChanged;
            }
            subscribed = false;
        }

        private void LateUpdate()
        {
            if (visualRoot == null || !visualRoot.activeSelf) return;
            if (pivot == null || characterVisual == null) return;

            Vector2 direction = characterVisual.FacingDirection;
            // Un-flatten the y component so the triangle's on-screen direction matches the aim
            // exactly after the parent squashes it; -90 because the sprite points up at rotation 0.
            float angle = Mathf.Atan2(direction.y / isoYScale, direction.x) * Mathf.Rad2Deg - 90f;
            pivot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void TrySubscribe()
        {
            if (subscribed || InputDeviceManager.Instance == null) return;
            InputDeviceManager.Instance.OnInputSchemeChanged += HandleInputSchemeChanged;
            subscribed = true;
        }

        private void HandleInputSchemeChanged(InputScheme scheme)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            if (visualRoot == null) return;
            bool gamepadActive = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            visualRoot.SetActive(gamepadActive && !hiddenOnMap);
        }

        public void DisableComponentsOnMap()
        {
            hiddenOnMap = true;
            RefreshVisibility();
        }
    }
}
