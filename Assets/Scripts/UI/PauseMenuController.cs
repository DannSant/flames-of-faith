using Game.Combat;
using Game.Control;
using Game.GameSettings;
using Game.Scene;
using Game.UI.Navigation;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace Game.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pauseScreenPanel;
        [SerializeField] private ConfirmDialogUI confirmDialog;
        [TextArea]
        [SerializeField] private string levelProgressLostWarning = "Your run is saved at the start of this level. Progress made in this level will be lost. Exit?";
        [Tooltip("Optional. The scene's Settings panel window, opened from the pause menu's Settings button.")]
        [SerializeField] private UIWindow settingsWindow;
        private SettingsPanelController settingsPanel;
        private bool settingsOpen;
        private PlayerInputHandler inputHandler;
        private bool isPaused = false;
        private StatsPaneUI statsPaneUI;
        private InventoryControllerUI inventoryControllerUI;
        private EndscreenControllerUI endscreen;
        private WaveCorruptionSummaryUI waveSummary;
        private LevelData levelData;


        private void Start()
        {
            pauseScreenPanel.SetActive(false);
            inputHandler = PlayerManager.Instance.GetPlayerComponent<PlayerInputHandler>();
            inputHandler.UI.Pause.performed += TogglePauseMenu;

            statsPaneUI = FindAnyObjectByType<StatsPaneUI>();
            inventoryControllerUI = FindAnyObjectByType<InventoryControllerUI>();
            endscreen = FindAnyObjectByType<EndscreenControllerUI>();
            waveSummary = FindAnyObjectByType<WaveCorruptionSummaryUI>();

            if (settingsWindow != null)
            {
                settingsWindow.gameObject.SetActive(false);
                settingsPanel = settingsWindow.GetComponentInChildren<SettingsPanelController>(true);
                if (settingsPanel != null) settingsPanel.OnCloseRequested += CloseSettings;
            }
        }

        private void OnDestroy()
        {
            inputHandler.UI.Pause.performed -= TogglePauseMenu;
            if (settingsPanel != null) settingsPanel.OnCloseRequested -= CloseSettings;
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
                var playerHealth = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerComponent<PlayerHealth>() : null;
                var data = GetLevelData();
                bool canOpen = CanOpenPauseMenu(
                    playerHealth != null && playerHealth.IsDead(),
                    (endscreen != null && endscreen.IsShown) || (waveSummary != null && waveSummary.IsShown),
                    PauseManager.Instance != null && PauseManager.Instance.IsPaused,
                    data == null || data.allowPause);
                if (!canOpen)
                {
                    return;
                }
            }

            // Pause closes the Save & Exit dialog first instead of resuming behind it
            if (confirmDialog != null && confirmDialog.IsOpen)
            {
                confirmDialog.Cancel();
                return;
            }

            // ...and Settings, back to the pause menu
            if (settingsOpen)
            {
                CloseSettings();
                return;
            }

            isPaused = !isPaused;
            pauseScreenPanel.SetActive(isPaused);


            if (isPaused)
            {
                inputHandler.AddGameplayBlock(this);
                PauseManager.Instance.SetPause(true); // Pause the game
                statsPaneUI?.ShowStatsWindow(null); // Show stats pane if available
                inventoryControllerUI?.ShowInventoryWindow();
            }
            else
            {
                inputHandler.RemoveGameplayBlock(this);
                PauseManager.Instance.SetPause(false); // Resume the game
                statsPaneUI?.HideStatsWindow(-1); // Hide stats pane if available
                inventoryControllerUI?.HideInventoryWindow(0);

            }
        }

        /// <summary>
        /// Whether Pause (Esc / Start) may open the pause menu. Not after death: the UI map stays on for the
        /// death screen, but there's nothing to pause. Not while another window already paused the game (the
        /// item bag, the level-up window, the end screen): Resume would unpause the game behind that window
        /// and hide its Stats and Inventory panes. Not over a window that ends a wave or the run without pausing
        /// (the wave results, the end screen before it pauses): the pause would freeze its count, and its
        /// Continue stays clickable behind the menu, so the item bag or level-up window could open under the
        /// pause menu and Resume would unpause them. Nor in a level that doesn't allow pausing.
        /// </summary>
        public static bool CanOpenPauseMenu(bool playerDead, bool blockingWindowShown, bool gameAlreadyPaused, bool levelAllowsPause) =>
            !playerDead && !blockingWindowShown && !gameAlreadyPaused && levelAllowsPause;

        public void ResumeGame()
        {
            TogglePauseMenu(default);
        }

        /// <summary>
        /// Opens the Settings panel in place of the pause menu (Stats and Inventory hide too, the panel takes
        /// their room). Shown with SetActive/SetOpen only: the game is paused, so nothing may wait on scaled time.
        /// </summary>
        public void OpenSettings()
        {
            if (settingsWindow == null || settingsOpen) return;

            settingsOpen = true;
            pauseScreenPanel.SetActive(false);
            statsPaneUI?.HideStatsWindow(-1);
            inventoryControllerUI?.HideInventoryWindow(0);
            settingsWindow.gameObject.SetActive(true);
            settingsWindow.SetOpen(true);
        }

        /// <summary>Back, Cancel (B) or Pause (Esc / Start) while Settings is open: back to the pause menu.</summary>
        public void CloseSettings()
        {
            if (!settingsOpen) return;

            settingsOpen = false;
            settingsWindow.SetOpen(false);
            settingsWindow.gameObject.SetActive(false);
            pauseScreenPanel.SetActive(true);
            statsPaneUI?.ShowStatsWindow(null);
            inventoryControllerUI?.ShowInventoryWindow();
        }

        public void ExitGame()
        {
            PauseManager.Instance.SetPause(false);
            MainSceneController.Instance.LoadMainMenu();
        }

        /// <summary>
        /// The run is autosaved on the map, so leaving a level only needs the player to accept losing this level's progress.
        /// </summary>
        public void SaveAndExit()
        {
            if (confirmDialog != null)
            {
                confirmDialog.Show(levelProgressLostWarning, ExitGame);
                return;
            }

            ExitGame();
        }

    }
}