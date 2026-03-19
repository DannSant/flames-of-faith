using Game.GameSettings;
using Game.Scene;
using UnityEngine;

namespace Game.UI.Overworld
{
    public class OverworldPlayerStatePresenter : MonoBehaviour
    {
        //[SerializeField] private CorruptionBar corruptionBar;
        //[SerializeField] private HealthBar healthBar;
        //[SerializeField] private GraceBar graceBar;
        [SerializeField] private OverworldCurrencyDisplay playerStatePanel;
        [SerializeField] private StatsPaneUI statsPaneUI;

        private const float MaxHealthDefault = 20f;
        private const float MaxGraceDefault = 10f;

        private void Start()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnLevelSelectorUISetupRequested += OnUISetup;
            }            
        }

        private void OnDisable()
        {
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnLevelSelectorUISetupRequested -= OnUISetup;
            }
        }

        private void OnUISetup()
        {
            Refresh();
        }

        public void Refresh()
        {
            var session = GameSession.Instance;            
            if (session == null)
            {
                return;
            }

            int currency = session.LoadCurrencyAmount();           
            playerStatePanel.SetCurrencyText(currency.ToString());
            /*
            var health = session.LoadCurrentHealth();
            var maxHealth = session.LoadMaxHealth();
            if (maxHealth == 0)
            {
                maxHealth = MaxHealthDefault;
            }
            healthBar.UpdateHealthBar(health, maxHealth);

            var grace = session.LoadCurrentGrace();
            var maxGrace = session.LoadMaxGrace();
            if(maxGrace == 0)
            {
                maxGrace = MaxGraceDefault;
            }
            graceBar.UpdateGrace(grace, maxGrace);

            var corruptionLevel =session.LoadCorruptionLevel();
            float actualCorruption = Mathf.Min(grace - corruptionLevel, 0);
            corruptionBar.UpdateCorruption(corruptionLevel, actualCorruption, maxGrace);
            */


            //statsPaneUI.ShowStatsWindow(null);
            /*var playerStats = session.PlayerData.savedStats;
            if (playerStats.Count > 0)
            {
                statsPaneUI.BuildStatRowsFromOverworld(playerStats);
                statsPaneUI.ShowStatsWindow(null);
            }*/



        }

        public void ExitGame()
        {
            PauseManager.Instance.SetPause(false);
            MainSceneController.Instance.LoadMainMenu();
        }
    }
}