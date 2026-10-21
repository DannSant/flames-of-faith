using Game.Control;
using Game.UI.Navigation;
using UnityEngine;

namespace Game.UI.InputPrompts
{
    public enum WindowFocusCondition
    {
        Any,
        Focused,
        NotFocused
    }

    // Conditions that don't belong to one specific window - for prompts shown next to the UI as a
    // whole, such as the "LB/RB switch window" hint.
    public enum GlobalFocusRequirement
    {
        Ignore,
        AnyWindowFocused,
        MultipleWindowsOpen,
        // Only while no window has gamepad focus, e.g. the map's movement prompts (appended: stored by index)
        NoWindowFocused
    }

    // Shows/hides input prompts depending on the active input scheme and, optionally, whether a
    // window has gamepad focus (e.g. "Y Browse" before browsing the shop, "A Buy" while browsing).
    // Never deactivates its own GameObject so it keeps receiving events - it toggles `targets`,
    // or a CanvasGroup on itself when no targets are assigned.
    public class InputPromptVisibilityUI : MonoBehaviour
    {
        [SerializeField] private bool showOnKeyboardMouse = true;
        [SerializeField] private bool showOnGamepad = true;
        [Tooltip("Objects to toggle. If empty, a CanvasGroup on this object is used instead.")]
        [SerializeField] private GameObject[] targets;

        [Header("Optional window focus condition")]
        [Tooltip("Leave empty for prompts that aren't tied to one window - use the requirement below instead.")]
        [SerializeField] private UIWindow window;
        [SerializeField] private WindowFocusCondition focusCondition = WindowFocusCondition.Any;
        [Tooltip("AnyWindowFocused: only while the gamepad is in the UI. MultipleWindowsOpen: only when LB/RB have somewhere to go. " +
                 "NoWindowFocused: only while the gamepad is out of the UI (e.g. moving on the map).")]
        [SerializeField] private GlobalFocusRequirement globalRequirement = GlobalFocusRequirement.Ignore;

        private CanvasGroup canvasGroup;
        private bool subscribedToDevices = false;
        private bool subscribedToFocus = false;

        private void Awake()
        {
            if (targets == null || targets.Length == 0)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void Start()
        {
            // Managers may not exist yet when this first enables, depending on scene load order.
            TrySubscribe();
            Refresh();
        }

        private void OnDisable()
        {
            if (subscribedToDevices && InputDeviceManager.Instance != null)
            {
                InputDeviceManager.Instance.OnInputSchemeChanged -= HandleInputSchemeChanged;
            }
            if (subscribedToFocus && UIFocusManager.Instance != null)
            {
                UIFocusManager.Instance.OnFocusChanged -= HandleFocusChanged;
                UIFocusManager.Instance.OnOpenWindowsChanged -= Refresh;
            }
            subscribedToDevices = false;
            subscribedToFocus = false;
        }

        private void TrySubscribe()
        {
            if (!subscribedToDevices && InputDeviceManager.Instance != null)
            {
                InputDeviceManager.Instance.OnInputSchemeChanged += HandleInputSchemeChanged;
                subscribedToDevices = true;
            }
            if (!subscribedToFocus && UIFocusManager.Instance != null)
            {
                UIFocusManager.Instance.OnFocusChanged += HandleFocusChanged;
                UIFocusManager.Instance.OnOpenWindowsChanged += Refresh;
                subscribedToFocus = true;
            }
        }

        private void HandleInputSchemeChanged(InputScheme scheme)
        {
            Refresh();
        }

        private void HandleFocusChanged(UIWindow focusedWindow)
        {
            Refresh();
        }

        public void Refresh()
        {
            bool gamepad = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            bool visible = gamepad ? showOnGamepad : showOnKeyboardMouse;

            if (visible && window != null && focusCondition != WindowFocusCondition.Any)
            {
                visible = focusCondition == WindowFocusCondition.Focused ? window.IsFocused : !window.IsFocused;
            }

            if (visible && globalRequirement != GlobalFocusRequirement.Ignore)
            {
                var focusManager = UIFocusManager.Instance;
                visible = MeetsGlobalRequirement(globalRequirement,
                    focusManager != null && focusManager.FocusedWindow != null,
                    focusManager != null ? focusManager.CountCyclableWindows() : 0);
            }

            if (targets != null && targets.Length > 0)
            {
                foreach (var target in targets)
                {
                    if (target != null && target != gameObject) target.SetActive(visible);
                }
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        /// <summary>Whether a prompt with this requirement shows, given whether a window has gamepad focus and how many LB/RB can reach.</summary>
        public static bool MeetsGlobalRequirement(GlobalFocusRequirement requirement, bool anyWindowFocused, int cyclableWindows)
        {
            switch (requirement)
            {
                case GlobalFocusRequirement.AnyWindowFocused: return anyWindowFocused;
                case GlobalFocusRequirement.MultipleWindowsOpen: return anyWindowFocused && cyclableWindows > 1;
                case GlobalFocusRequirement.NoWindowFocused: return !anyWindowFocused;
                default: return true;
            }
        }
    }
}
