using TMPro;
using UnityEngine;

namespace Game.UI.Overworld
{
    /// <summary>
    /// The map window (top right): the act title, the name of the level the player stands on, the Favors and the
    /// coins, each with a tooltip. Only takes plain values, pushed by OverworldPlayerStatePresenter. Layout:
    /// LevelSelectorRestyler.
    /// </summary>
    public class OverworldMapInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI actTitleText;
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TooltipTriggerUI coinsTooltip;
        [Tooltip("Banked (+earned this Pilgrimage).")]
        [SerializeField] private TextMeshProUGUI favorsText;
        [SerializeField] private TooltipTriggerUI favorsTooltip;
        [SerializeField] private CurrencyTooltipTexts currencyTooltips = new();

        private int bankedFavors;
        private int runFavors;

        private void Start()
        {
            if (coinsTooltip != null) coinsTooltip.Setup(currencyTooltips.CoinsTitle, () => currencyTooltips.CoinsText);
            if (favorsTooltip != null) favorsTooltip.Setup(currencyTooltips.FavorsTitle, () => currencyTooltips.FormatFavors(bankedFavors, runFavors));
        }

        public void SetActTitle(string text)
        {
            if (actTitleText != null) actTitleText.text = text;
        }

        public void SetLevelName(string text)
        {
            if (levelNameText != null) levelNameText.text = text;
        }

        public void SetCoins(string text)
        {
            if (coinsText != null) coinsText.text = text;
        }

        public void SetFavors(int banked, int earnedThisRun)
        {
            bankedFavors = banked;
            runFavors = earnedThisRun;
            if (favorsText != null) favorsText.text = CurrencyTooltipTexts.FormatFavorsAmount(banked, earnedThisRun);
        }
    }
}
