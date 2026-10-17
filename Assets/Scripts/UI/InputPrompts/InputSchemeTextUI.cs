using Game.Control;
using TMPro;
using UnityEngine;

namespace Game.UI.InputPrompts
{
    // Swaps a label's text with the active input scheme, e.g. "Click" on keyboard/mouse and "Press"
    // on gamepad next to an InputPromptIconUI / InputPromptVisibilityUI icon.
    [RequireComponent(typeof(TMP_Text))]
    public class InputSchemeTextUI : MonoBehaviour
    {
        [SerializeField] private string keyboardMouseText = "Click ";
        [SerializeField] private string gamepadText = "Press ";

        private TMP_Text label;
        private bool subscribed = false;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void Start()
        {
            // InputDeviceManager may not exist yet when this first enables, depending on scene load order.
            TrySubscribe();
            Refresh();
        }

        private void OnDisable()
        {
            if (subscribed && InputDeviceManager.Instance != null)
            {
                InputDeviceManager.Instance.OnInputSchemeChanged -= HandleInputSchemeChanged;
            }
            subscribed = false;
        }

        private void TrySubscribe()
        {
            if (subscribed || InputDeviceManager.Instance == null) return;
            InputDeviceManager.Instance.OnInputSchemeChanged += HandleInputSchemeChanged;
            subscribed = true;
        }

        private void HandleInputSchemeChanged(InputScheme scheme)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (label == null) return;
            bool gamepad = InputDeviceManager.Instance != null && InputDeviceManager.Instance.IsGamepadActive;
            label.text = gamepad ? gamepadText : keyboardMouseText;
        }
    }
}
