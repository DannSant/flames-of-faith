using Game.Control;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI.Navigation
{
    public enum UIWindowRole
    {
        // Main window of a situation (Pause, Upgrades, Shop...) - candidate for automatic focus.
        Primary,
        // Supporting window (Inventory, Stats) - only reached by cycling focus with LB/RB.
        Secondary
    }

    public enum UIWindowFocusMode
    {
        // Takes gamepad focus as soon as it opens.
        AutoFocus,
        // Only takes focus when the player presses Browse (e.g. Shop, which doesn't pause gameplay).
        OnDemand
    }

    public enum UIWindowOpenMode
    {
        // Open while the GameObject is active - fits panels a controller shows/hides.
        GameObjectActive,
        // Open only when a controller calls SetOpen - for panels that stay active and animate
        // off screen instead (the MainMenu panels).
        Manual
    }

    // Put this on the panel a window controller toggles on/off: OnEnable/OnDisable then match the
    // window opening/closing, so controllers need no code to take part in gamepad navigation.
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWindow : MonoBehaviour
    {
        [SerializeField] private UIWindowRole role = UIWindowRole.Primary;
        [Tooltip("GameObjectActive: open while this object is active. Manual: a controller calls SetOpen().")]
        [SerializeField] private UIWindowOpenMode openMode = UIWindowOpenMode.GameObjectActive;
        [Tooltip("When several Primary windows are open, the highest priority gets focus.")]
        [SerializeField] private int priority = 0;
        [SerializeField] private UIWindowFocusMode focusMode = UIWindowFocusMode.AutoFocus;
        [Tooltip("Disables gameplay input (movement, dash, attacks) while this window has gamepad focus.")]
        [SerializeField] private bool blocksGameplayInput = false;
        [Tooltip("Cancel (B) hands control back to gameplay while leaving this window open - for windows " +
                 "that open on proximity (Shop) so the player can walk away again.")]
        [SerializeField] private bool releaseFocusOnCancel = false;
        [Tooltip("Optional. Otherwise the first active, interactable Selectable in children is used.")]
        [SerializeField] private Selectable defaultSelectable;
        [Tooltip("Invoked when Cancel (B) is pressed while this window has gamepad focus.")]
        [SerializeField] private UnityEvent onCancel;

        [Header("Focus feedback")]
        [Tooltip("Optional frame/background recolored to show whether this window has gamepad focus.")]
        [SerializeField] private Graphic focusFrame;
        [SerializeField] private Color focusedFrameColor = Color.white;
        [SerializeField] private Color unfocusedFrameColor = new Color(1f, 1f, 1f, 0.3f);
        [Tooltip("Optional objects shown only while this window has gamepad focus (e.g. a highlight border).")]
        [SerializeField] private GameObject[] focusedOnlyObjects;

        [Header("Selection marker")]
        [Tooltip("Use this window's own marker padding/offset instead of the marker's defaults.")]
        [SerializeField] private bool overrideMarkerSettings = false;
        [SerializeField] private Vector2 markerPadding = new Vector2(8f, 8f);
        [SerializeField] private Vector2 markerOffset = Vector2.zero;
        [Tooltip("Resize the marker to the selected element. Turn off for a fixed-size marker such as an arrow.")]
        [SerializeField] private bool markerMatchesSize = true;

        public bool OverrideMarkerSettings => overrideMarkerSettings;
        public Vector2 MarkerPadding => markerPadding;
        public Vector2 MarkerOffset => markerOffset;
        public bool MarkerMatchesSize => markerMatchesSize;

        private CanvasGroup canvasGroup;
        private bool registered = false;
        private bool manuallyOpen = false;
        private bool subscribedToDeviceManager = false;

        public UIWindowRole Role => role;
        public int Priority => priority;
        public UIWindowFocusMode FocusMode => focusMode;
        public bool BlocksGameplayInput => blocksGameplayInput;
        public bool ReleaseFocusOnCancel => releaseFocusOnCancel;
        public bool IsFocused { get; private set; }
        public UnityEvent OnCancel => onCancel;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            TryRegister();
            TrySubscribe();
            ApplyFocusVisuals();
        }

        private void Start()
        {
            // Managers may not exist yet when this first enables, depending on scene load order.
            TryRegister();
            TrySubscribe();
            ApplyFocusVisuals();
        }

        private void OnDisable()
        {
            if (registered && UIFocusManager.Instance != null)
            {
                UIFocusManager.Instance.Unregister(this);
            }
            if (subscribedToDeviceManager && InputDeviceManager.Instance != null)
            {
                InputDeviceManager.Instance.OnInputSchemeChanged -= HandleInputSchemeChanged;
            }
            registered = false;
            subscribedToDeviceManager = false;
            IsFocused = false;
        }

        private void TryRegister()
        {
            if (openMode == UIWindowOpenMode.Manual && !manuallyOpen) return;
            if (registered || UIFocusManager.Instance == null) return;
            registered = true;
            UIFocusManager.Instance.Register(this);
        }

        // Manual mode only: tells the focus system this window is now showing (or no longer is),
        // for panels that stay active and slide off screen rather than being deactivated.
        public void SetOpen(bool open)
        {
            manuallyOpen = open;

            if (open)
            {
                TryRegister();
                return;
            }

            if (registered && UIFocusManager.Instance != null)
            {
                UIFocusManager.Instance.Unregister(this);
            }
            registered = false;
            IsFocused = false;
            ApplyFocusVisuals();
        }

        private void TrySubscribe()
        {
            if (subscribedToDeviceManager || InputDeviceManager.Instance == null) return;
            InputDeviceManager.Instance.OnInputSchemeChanged += HandleInputSchemeChanged;
            subscribedToDeviceManager = true;
        }

        private void HandleInputSchemeChanged(InputScheme scheme)
        {
            ApplyFocusVisuals();
        }

        internal void SetFocused(bool focused)
        {
            IsFocused = focused;
            ApplyFocusVisuals();
        }

        // On keyboard/mouse no window is focused, so every window is shown in its normal state.
        private void ApplyFocusVisuals()
        {
            bool gamepadActive = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            bool showAsFocused = IsFocused || !gamepadActive;

            if (focusFrame != null)
            {
                focusFrame.color = showAsFocused ? focusedFrameColor : unfocusedFrameColor;
            }

            if (focusedOnlyObjects == null) return;
            foreach (var focusedObject in focusedOnlyObjects)
            {
                if (focusedObject != null && focusedObject != gameObject)
                {
                    focusedObject.SetActive(IsFocused && gamepadActive);
                }
            }
        }

        internal void SetInteractable(bool interactable)
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = interactable;
        }

        public Selectable GetDefaultSelectable()
        {
            if (IsSelectableUsable(defaultSelectable)) return defaultSelectable;

            foreach (var selectable in GetComponentsInChildren<Selectable>())
            {
                if (IsSelectableUsable(selectable)) return selectable;
            }
            return null;
        }

        private static bool IsSelectableUsable(Selectable selectable)
        {
            return selectable != null
                && selectable.isActiveAndEnabled
                && selectable.IsInteractable()
                // Fully qualified: inside Game.UI.Navigation, "Navigation" resolves to this namespace.
                && selectable.navigation.mode != UnityEngine.UI.Navigation.Mode.None;
        }

        // Horizontal screen position, used to order LB/RB cycling left to right.
        public float GetScreenX()
        {
            var rectTransform = transform as RectTransform;
            if (rectTransform == null) return transform.position.x;

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return (corners[0].x + corners[2].x) * 0.5f;
        }
    }
}
