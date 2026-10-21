using TMPro;
using UnityEngine;

namespace Game.UI.Overworld
{
    /// <summary>
    /// The map window (top right): the act title, the name of the level the player stands on, and the coins.
    /// Only takes plain text, pushed by OverworldPlayerStatePresenter. Layout: LevelSelectorRestyler.
    /// </summary>
    public class OverworldMapInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI actTitleText;
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private TextMeshProUGUI coinsText;

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
    }
}
