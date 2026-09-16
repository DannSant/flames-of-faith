using Game.Control;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI.InputPrompts
{
    // Shows the glyph for whatever control is bound to an action on the active device family.
    // Reads the binding instead of a hardcoded sprite, so it follows future rebinding.
    [RequireComponent(typeof(Image))]
    public class InputPromptIconUI : MonoBehaviour
    {
        private const string GamepadGroup = "Gamepad";
        private const string KeyboardMouseGroup = "Keyboard&Mouse";

        [SerializeField] private InputActionReference action;
        [Tooltip("Optional composite part (e.g. \"up\" for a 2D vector Move composite). Leave empty for regular bindings.")]
        [SerializeField] private string compositePartName;
        [Tooltip("Optional. Shows this control name (e.g. leftStick) instead of resolving the action's binding - " +
                 "for prompts like \"stick to move\" where the action is a composite with no single control.")]
        [SerializeField] private string overrideControlName;
        [Tooltip("Optional. Falls back to Resources/" + InputIconDatabase.DefaultResourcePath + ".")]
        [SerializeField] private InputIconDatabase database;

        private Image image;
        private bool subscribed = false;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (database == null)
            {
                database = InputIconDatabase.LoadDefault();
            }
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
                InputDeviceManager.Instance.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
            }
            subscribed = false;
        }

        private void TrySubscribe()
        {
            if (subscribed || InputDeviceManager.Instance == null) return;
            InputDeviceManager.Instance.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;
            subscribed = true;
        }

        private void HandleDeviceFamilyChanged(InputDeviceFamily family)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (image == null) return;

            InputDeviceFamily family = InputDeviceManager.Instance != null
                ? InputDeviceManager.Instance.CurrentDeviceFamily
                : InputDeviceFamily.KeyboardMouse;

            bool forGamepad = family != InputDeviceFamily.KeyboardMouse;
            string bindingPath = string.IsNullOrEmpty(overrideControlName)
                ? FindBindingPath(forGamepad)
                : overrideControlName;
            // Submit/Cancel bind to usages ("*/{Submit}") rather than a concrete control, so also
            // try the control the action actually resolved to on this device.
            string resolvedControl = string.IsNullOrEmpty(overrideControlName)
                ? FindResolvedControlName(forGamepad)
                : null;

            if (database != null
                && (database.TryGetSprite(family, bindingPath, out Sprite sprite)
                    || database.TryGetSprite(family, resolvedControl, out sprite)))
            {
                image.sprite = sprite;
                image.enabled = true;
                return;
            }

            image.enabled = false;
            if (action != null || !string.IsNullOrEmpty(overrideControlName))
            {
                string source = action != null ? $"action '{action.name}'" : $"control '{overrideControlName}'";
                Debug.LogWarning($"[InputPromptIconUI] No {family} glyph on {name} for {source}. " +
                    $"Add an entry named '{InputIconSet.Normalize(bindingPath)}'" +
                    (string.IsNullOrEmpty(resolvedControl) || resolvedControl == InputIconSet.Normalize(bindingPath)
                        ? "" : $" or '{resolvedControl}'") +
                    $" (full path: '{bindingPath}') to the icon set.", this);
            }
        }

        private string FindResolvedControlName(bool forGamepad)
        {
            if (action == null || action.action == null) return null;

            foreach (var control in action.action.controls)
            {
                bool isGamepadControl = control.device is Gamepad;
                if (isGamepadControl == forGamepad) return control.name;
            }
            return null;
        }

        private string FindBindingPath(bool forGamepad)
        {
            if (action == null || action.action == null) return null;

            string group = forGamepad ? GamepadGroup : KeyboardMouseGroup;
            string fallbackPath = null;

            foreach (var binding in action.action.bindings)
            {
                if (binding.isComposite) continue;
                if (!string.IsNullOrEmpty(compositePartName))
                {
                    if (!binding.isPartOfComposite || binding.name != compositePartName) continue;
                }
                else if (binding.isPartOfComposite)
                {
                    continue;
                }

                string path = binding.effectivePath;
                if (BindingHasGroup(binding, group)) return path;

                // Some bindings (e.g. Pause) have no control-scheme group - match them by device instead.
                if (fallbackPath == null && string.IsNullOrEmpty(binding.groups) && PathMatchesDevice(path, forGamepad))
                {
                    fallbackPath = path;
                }
            }
            return fallbackPath;
        }

        private static bool BindingHasGroup(InputBinding binding, string group)
        {
            if (string.IsNullOrEmpty(binding.groups)) return false;
            foreach (var bindingGroup in binding.groups.Split(InputBinding.Separator))
            {
                if (bindingGroup == group) return true;
            }
            return false;
        }

        private static bool PathMatchesDevice(string path, bool forGamepad)
        {
            string layout = InputControlPath.TryGetDeviceLayout(path);
            if (string.IsNullOrEmpty(layout)) return false;

            if (forGamepad)
            {
                return InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad");
            }
            return InputSystem.IsFirstLayoutBasedOnSecond(layout, "Keyboard")
                || InputSystem.IsFirstLayoutBasedOnSecond(layout, "Pointer");
        }
    }
}
