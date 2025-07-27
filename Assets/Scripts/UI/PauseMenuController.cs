using Game.Control;
using Game.Scene;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Game.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pauseScreenPanel;
        private PlayerInputHandler inputHandler;
        private bool isPaused = false;
        private StatsPaneUI statsPaneUI;

        private void Start()
        {
            pauseScreenPanel.SetActive(false);
            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            inputHandler.UI.Pause.performed += TogglePauseMenu;

            statsPaneUI = FindAnyObjectByType<StatsPaneUI>();
        }

        private void OnDestroy()
        {
            inputHandler.UI.Pause.performed -= TogglePauseMenu;
        }

        private void TogglePauseMenu(CallbackContext _)
        {
            isPaused = !isPaused;
            pauseScreenPanel.SetActive(isPaused);

            if (isPaused)
            {
                inputHandler.Player.Disable();
                Time.timeScale = 0f; // Pause the game
                statsPaneUI?.ShowStatsWindow(null); // Show stats pane if available
            }
            else
            {
                inputHandler.Player.Enable();
               Time.timeScale = 1f; // Resume the game
                statsPaneUI?.HideStatsWindow(-1); // Hide stats pane if available

            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            pauseScreenPanel.SetActive(isPaused);
            inputHandler.Player.Enable();
            Time.timeScale = 1f; // Resume the game
            statsPaneUI?.HideStatsWindow(-1); // Hide stats pane if available
        }

        public void ExitGame()
        {
            Time.timeScale = 1f; // Resume the game
            MainSceneController.Instance.LoadMainMenu();
        }

    }
}