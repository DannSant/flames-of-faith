using Game.Combat;
using Game.Common;
using Game.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Control
{

    public class PlayerInputHandler : Singleton<PlayerInputHandler>
    {
       

        private InputSystem_Actions inputActions;

        public InputSystem_Actions.PlayerActions Player => inputActions.Player;

        protected override void Awake()
        {        
            base.Awake();
            inputActions = new InputSystem_Actions();
        }

        private void Start()
        {
            if(PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.onDeath += DisableInput;
            }

            if(MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += EnableInput;
            }
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.onDeath -= DisableInput;
            }
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= EnableInput;
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
