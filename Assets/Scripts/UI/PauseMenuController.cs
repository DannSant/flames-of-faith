using Game.Control;
using Game.GameSettings;
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
        private InventoryControllerUI inventoryControllerUI;
        private LevelData levelData;


        private void Start()
        {
            pauseScreenPanel.SetActive(false);
            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            inputHandler.UI.Pause.performed += TogglePauseMenu;

            statsPaneUI = FindAnyObjectByType<StatsPaneUI>();
            inventoryControllerUI = FindAnyObjectByType<InventoryControllerUI>();
        }

        private void OnDestroy()
        {
            inputHandler.UI.Pause.performed -= TogglePauseMenu;
        }

        private LevelData GetLevelData()
        {
            if (levelData == null)
            {
                var levelSettings = FindAnyObjectByType<LevelSettings>();
                if (levelSettings != null)
                {
                    levelData = levelSettings.LevelData;
                }
            }
            return levelData;
        }

        private void TogglePauseMenu(CallbackContext _)
        {
            if (!isPaused)
            {
                var data = GetLevelData();
                if (data != null && !data.allowPause)
                {
                    return;
                }
            }

            isPaused = !isPaused;
            pauseScreenPanel.SetActive(isPaused);


            if (isPaused)
            {
                inputHandler.Player.Disable();
                PauseManager.Instance.SetPause(true); // Pause the game
                statsPaneUI?.ShowStatsWindow(null); // Show stats pane if available
                inventoryControllerUI?.ShowInventoryWindow();
            }
            else
            {
                inputHandler.Player.Enable();
                PauseManager.Instance.SetPause(false); // Resume the game
                statsPaneUI?.HideStatsWindow(-1); // Hide stats pane if available
                inventoryControllerUI?.HideInventoryWindow(0);

            }
        }

        public void ResumeGame()
        {
            TogglePauseMenu(default);
            //isPaused = false;
            //pauseScreenPanel.SetActive(isPaused);
            //inputHandler.Player.Enable();
            //Time.timeScale = 1f; // Resume the game
            //statsPaneUI?.HideStatsWindow(-1); // Hide stats pane if available
        }

        public void ExitGame()
        {
            PauseManager.Instance.SetPause(false);
            MainSceneController.Instance.LoadMainMenu();
        }

    }
}