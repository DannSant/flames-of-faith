using Game.Combat;
using Game.Scene;
using UnityEngine;

namespace Game.UI
{
    public class DeathscreenController : MonoBehaviour
    {
        [SerializeField] private GameObject deathScreenPanel;

        [Header("Run review")]
        [Tooltip("Also open the Stats pane and the Inventory with the death screen, so the player can look over the build that died.")]
        [SerializeField] private bool showStatsAndInventoryOnDeath = true;
        [Tooltip("Optional. Found in the loaded scenes when empty.")]
        [SerializeField] private StatsPaneUI statsPane;
        [Tooltip("Optional. Found in the loaded scenes when empty.")]
        [SerializeField] private InventoryControllerUI inventoryPane;

        private StatsAndInventoryPanes runReview;

        private void Start()
        {
            deathScreenPanel.SetActive(false);
            /*if (PlayerHealth.Instance != null) {
                PlayerHealth.Instance.onDeath += ShowDeathScreen;
            }*/
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += ShowDeathScreen;
            }

        }

        private void OnDisable()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= ShowDeathScreen;
            }
        }

        private void ShowDeathScreen()
        {
            deathScreenPanel.SetActive(true);
            // Opened after the death screen: they're Secondary windows, so the death screen keeps
            // gamepad focus and LB/RB reach them.
            if (showStatsAndInventoryOnDeath)
            {
                ToggleRunReview(true);
            }
        }

        private void ToggleRunReview(bool show)
        {
            runReview ??= new StatsAndInventoryPanes(statsPane, inventoryPane);
            if (show) runReview.Show();
            else runReview.Hide();
        }

        public void OnRetryButtonClicked()
        {
            // Retry goes back to the map, where these panes would otherwise stay open.
            ToggleRunReview(false);
            MainSceneController.Instance.RetryCurrentRun();
        }

        public void OnMainMenuButtonClicked()
        {
            ToggleRunReview(false);
            MainSceneController.Instance.LoadMainMenu();
        }
    }

}
