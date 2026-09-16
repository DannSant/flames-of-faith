using Game.Combat;
using Game.Common;
using Game.Scene;
using Game.UI.Navigation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Control
{

    public class PlayerInputHandler : MonoBehaviour
    {
       

        private InputSystem_Actions inputActions;
        // Everything currently asking for gameplay (Player map) input to be off - pause menu,
        // gamepad shop browsing... Input only comes back once every source has released it,
        // so two overlapping blockers can't re-enable input early.
        private readonly HashSet<object> gameplayBlocks = new();
        private bool subscribedToFocusManager = false;

        public InputSystem_Actions.PlayerActions Player => inputActions.Player;
        public InputSystem_Actions.UIActions UI => inputActions.UI;
        public bool IsGameplayInputBlocked => gameplayBlocks.Count > 0;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        private void Start()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += DisableInput;
            }

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup += EnableInput;
            }

            TrySubscribeToFocusManager();
        }

        private void OnEnable()
        {
            inputActions.Enable();
            ApplyGameplayBlocks();
            TrySubscribeToFocusManager();
        }

        private void OnDisable()
        {
            inputActions.Disable();
            if (subscribedToFocusManager && UIFocusManager.Instance != null)
            {
                UIFocusManager.Instance.OnGameplayInputBlockChanged -= HandleUIGameplayBlockChanged;
            }
            subscribedToFocusManager = false;
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= DisableInput;
            }
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= EnableInput;
            }
        }

        public void DisableInput()
        {
            inputActions.Disable();
        }

        public void EnableInput()
        {
            inputActions.Enable();
            ApplyGameplayBlocks();
        }

        public void AddGameplayBlock(object source)
        {
            if (source == null || !gameplayBlocks.Add(source)) return;
            ApplyGameplayBlocks();
        }

        public void RemoveGameplayBlock(object source)
        {
            if (source == null || !gameplayBlocks.Remove(source)) return;
            ApplyGameplayBlocks();
        }

        private void ApplyGameplayBlocks()
        {
            if (inputActions == null) return;
            if (gameplayBlocks.Count > 0)
            {
                inputActions.Player.Disable();
            }
            else if (inputActions.UI.enabled)
            {
                // Only restore Player when input as a whole is on (DisableInput turns off both maps on death).
                inputActions.Player.Enable();
            }
        }

        private void TrySubscribeToFocusManager()
        {
            if (subscribedToFocusManager || UIFocusManager.Instance == null) return;
            UIFocusManager.Instance.OnGameplayInputBlockChanged += HandleUIGameplayBlockChanged;
            subscribedToFocusManager = true;
            HandleUIGameplayBlockChanged(UIFocusManager.Instance.IsGameplayInputBlocked);
        }

        private void HandleUIGameplayBlockChanged(bool blocked)
        {
            if (blocked)
            {
                AddGameplayBlock(UIFocusManager.Instance);
            }
            else
            {
                RemoveGameplayBlock(UIFocusManager.Instance);
            }
        }
    }
}
