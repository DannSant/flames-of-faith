using TMPro;
using UnityEngine;

namespace Game.UI.Overworld
{
    public class OverworldCurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currencyText;

        public void SetCurrencyText(string text)
        {
            currencyText.text = text;
        }
    }
}