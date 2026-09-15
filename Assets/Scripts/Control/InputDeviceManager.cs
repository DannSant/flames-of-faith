using Game.Common;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Game.Control
{
    public enum InputScheme
    {
        KeyboardMouse,
        Gamepad
    }

    // Tracks which device the player is *currently using* (last device with meaningful
    // input), not merely whether a gamepad is plugged in - so moving the mouse flips back
    // to keyboard/mouse even with a gamepad connected, and touching the gamepad flips back.
    public class InputDeviceManager : Singleton<InputDeviceManager>
    {
        [Tooltip("Stick magnitude needed to count as gamepad use - keeps stick drift from switching schemes.")]
        [SerializeField] private float stickThreshold = 0.25f;
        [Tooltip("Mouse movement in pixels per frame needed to count as mouse use - keeps desk bumps from switching schemes.")]
        [SerializeField] private float mouseMoveThreshold = 3f;

        private InputScheme currentScheme = InputScheme.KeyboardMouse;

        public InputScheme CurrentScheme => currentScheme;
        public bool IsGamepadActive => currentScheme == InputScheme.Gamepad;

        public event Action<InputScheme> OnInputSchemeChanged;
        public event Action<Gamepad> OnGamepadConnected;
        public event Action<Gamepad> OnGamepadDisconnected;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            ApplyCursorVisibility();
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += HandleDeviceChange;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= HandleDeviceChange;
        }

        // Update still runs while Time.timeScale is 0, so scheme switching keeps working in menus/pause.
        private void Update()
        {
            if (currentScheme == InputScheme.Gamepad)
            {
                if (WasKeyboardMouseUsed()) SetScheme(InputScheme.KeyboardMouse);
            }
            else
            {
                if (WasGamepadUsed()) SetScheme(InputScheme.Gamepad);
            }
        }

        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not Gamepad gamepad) return;

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    OnGamepadConnected?.Invoke(gamepad);
                    break;
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    OnGamepadDisconnected?.Invoke(gamepad);
                    if (IsGamepadActive && !AnyGamepadConnected(gamepad))
                    {
                        SetScheme(InputScheme.KeyboardMouse);
                    }
                    break;
            }
        }

        private bool AnyGamepadConnected(Gamepad ignored)
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad != ignored && gamepad.added) return true;
            }
            return false;
        }

        private bool WasGamepadUsed()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return false;

            if (gamepad.leftStick.ReadValue().magnitude > stickThreshold) return true;
            if (gamepad.rightStick.ReadValue().magnitude > stickThreshold) return true;

            foreach (var control in gamepad.allControls)
            {
                if (control is ButtonControl button && !button.synthetic && button.wasPressedThisFrame)
                {
                    return true;
                }
            }
            return false;
        }

        private bool WasKeyboardMouseUsed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) return true;

            var mouse = Mouse.current;
            if (mouse == null) return false;

            if (mouse.leftButton.wasPressedThisFrame
                || mouse.rightButton.wasPressedThisFrame
                || mouse.middleButton.wasPressedThisFrame)
            {
                return true;
            }
            return mouse.delta.ReadValue().sqrMagnitude > mouseMoveThreshold * mouseMoveThreshold;
        }

        private void SetScheme(InputScheme scheme)
        {
            if (currentScheme == scheme) return;
            currentScheme = scheme;
            ApplyCursorVisibility();
            OnInputSchemeChanged?.Invoke(currentScheme);
        }

        private void ApplyCursorVisibility()
        {
            Cursor.visible = !IsGamepadActive;
        }
    }
}
