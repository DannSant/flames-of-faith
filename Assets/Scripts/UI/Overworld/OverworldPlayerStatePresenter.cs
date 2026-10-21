using Game.GameSettings;
using Game.Overworld;
using Game.Saving;
using Game.Scene;
using UnityEngine;

namespace Game.UI.Overworld
{
    /// <summary>
    /// Feeds the map's UI: the map window (act title, the level under the player, coins) and the Stats pane.
    /// The Stats pane opens once the player's state is loaded (PlayerManager.OnPlayerDisabledOnMap fires on the
    /// map after that): the map's UI setup event comes earlier, when the stats are still empty.
    /// </summary>
    public class OverworldPlayerStatePresenter : MonoBehaviour
    {
        [SerializeField] private OverworldMapInfoUI mapInfo;
        [SerializeField] private StatsPaneUI statsPaneUI;
        [Tooltip("{0} = act number in Roman numerals, {1} = the act's name.")]
        [SerializeField] private string actTitleFormat = "Act {0} - {1}";

        // One-time lookups purely to attach/detach event subscriptions
        private MapRunController mapRunController;
        private PlayerManager playerManager;

        private void Start()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnLevelSelectorUISetupRequested += OnUISetup;
            }

            playerManager = PlayerManager.Instance;
            if (playerManager != null)
            {
                playerManager.OnPlayerDisabledOnMap += ShowStats;
            }

            mapRunController = MapRunController.Instance;
            if (mapRunController != null)
            {
                mapRunController.OnRunMapInitialized += RefreshLocation;
                mapRunController.OnCurrentNodeChanged += HandleCurrentNodeChanged;
                mapRunController.OnActChanged += HandleActChanged;
                mapRunController.OnMapStateChanged += RefreshLocation;
                if (mapRunController.IsInitialized) RefreshLocation();
            }
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnLevelSelectorUISetupRequested -= OnUISetup;
            }
            if (playerManager != null)
            {
                playerManager.OnPlayerDisabledOnMap -= ShowStats;
            }
            if (mapRunController != null)
            {
                mapRunController.OnRunMapInitialized -= RefreshLocation;
                mapRunController.OnCurrentNodeChanged -= HandleCurrentNodeChanged;
                mapRunController.OnActChanged -= HandleActChanged;
                mapRunController.OnMapStateChanged -= RefreshLocation;
            }
        }

        private void OnUISetup()
        {
            Refresh();
        }

        public void Refresh()
        {
            var session = GameSession.Instance;
            if (session != null && mapInfo != null)
            {
                mapInfo.SetCoins(session.LoadCurrencyAmount().ToString());
            }
            RefreshLocation();
        }

        private void HandleCurrentNodeChanged(RunNode node) => RefreshLocation();

        private void HandleActChanged(int act) => RefreshLocation();

        private void RefreshLocation()
        {
            if (mapInfo == null || mapRunController == null || !mapRunController.IsInitialized) return;

            var act = mapRunController.CurrentAct;
            if (act != null) mapInfo.SetActTitle(FormatActTitle(actTitleFormat, act.actNumber, act.displayName));

            var node = mapRunController.CurrentNode;
            mapInfo.SetLevelName(node != null && node.levelData != null ? node.levelData.DisplayName : string.Empty);
        }

        private void ShowStats()
        {
            if (statsPaneUI != null) statsPaneUI.ShowStatsWindow(null);
        }

        /// <summary>"Act I - Jesterwoods"; without a name, just "Act I".</summary>
        public static string FormatActTitle(string format, int actNumber, string actName)
        {
            string number = ToRoman(actNumber);
            return string.IsNullOrWhiteSpace(actName) ? $"Act {number}" : string.Format(format, number, actName);
        }

        public static string ToRoman(int number)
        {
            if (number <= 0) return number.ToString();
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] symbols = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                while (number >= values[i])
                {
                    number -= values[i];
                    result.Append(symbols[i]);
                }
            }
            return result.ToString();
        }

        // Save & Exit: the map autosaves already, saving again just makes sure the file is current
        public void ExitGame()
        {
            RunSaveService.SaveCurrentRun();
            PauseManager.Instance.SetPause(false);
            MainSceneController.Instance.LoadMainMenu();
        }
    }
}
