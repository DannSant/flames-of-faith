using Game.Scene;
using UnityEngine;

namespace Game.UI.Overworld
{
    public class OverworldPlayerStatePresenter : MonoBehaviour
    {
        [SerializeField] private CorruptionBar corruptionBar;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private GraceBar graceBar;
        [SerializeField] private OverworldCurrencyDisplay playerStatePanel;
        [SerializeField] private StatsPaneUI statsPaneUI;

        private const float MaxHealthDefault = 20f;
        private const float MaxGraceDefault = 10f;

        private void Start()
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

            int currency = session.LoadCurrencyAmount();
            playerStatePanel.SetCurrencyText(currency.ToString());

            var playerStats = session.PlayerData.savedStats;
            if (playerStats.Count > 0)
            {
                statsPaneUI.BuildStatRowsFromOverworld(playerStats);
                statsPaneUI.ShowStatsWindow(null);
            }
            


        }
    }
}