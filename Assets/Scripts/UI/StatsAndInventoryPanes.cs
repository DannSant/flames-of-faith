using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Opens and closes the Stats pane and the Inventory beside a center window (death screen, end screen,
    /// campfire), so the player can look over the build. UI-to-UI lookup only: both panes are fed by gameplay
    /// events of their own. Panes left empty are found in the loaded scenes on first use.
    /// </summary>
    public class StatsAndInventoryPanes
    {
        private StatsPaneUI statsPane;
        private InventoryControllerUI inventoryPane;

        public StatsAndInventoryPanes(StatsPaneUI statsPane = null, InventoryControllerUI inventoryPane = null)
        {
            this.statsPane = statsPane;
            this.inventoryPane = inventoryPane;
        }

        /// <summary>Open them after the window that owns them: they're Secondary windows, so it keeps gamepad focus.</summary>
        public void Show() => Toggle(true);

        public void Hide() => Toggle(false);

        private void Toggle(bool show)
        {
            if (statsPane == null) statsPane = Object.FindAnyObjectByType<StatsPaneUI>(FindObjectsInactive.Include);
            if (inventoryPane == null) inventoryPane = Object.FindAnyObjectByType<InventoryControllerUI>(FindObjectsInactive.Include);

            if (statsPane != null)
            {
                if (show) statsPane.ShowStatsWindow(null);
                else statsPane.HideStatsWindow(0);
            }
            if (inventoryPane != null)
            {
                if (show) inventoryPane.ShowInventoryWindow();
                else inventoryPane.HideInventoryWindow(0);
            }
        }
    }
}
