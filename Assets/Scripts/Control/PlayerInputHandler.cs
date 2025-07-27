using Game.Combat;
using Game.Common;
using Game.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Control
{

    public class PlayerInputHandler : MonoBehaviour
    {
       

        private InputSystem_Actions inputActions;

        public InputSystem_Actions.PlayerActions Player => inputActions.Player;
        public InputSystem_Actions.UIActions UI => inputActions.UI;

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
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
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
        }
    }
}
