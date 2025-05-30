using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class StatRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI statValueText;

        public void Initialize(string statName, int statValue, Color color)
        {
            if (statNameText != null)
            {
                statNameText.text = statName;
                statNameText.color = color;
            }
            if (statValueText != null)
            {
                statValueText.text = statValue.ToString();
                statValueText.color = color;
            }
        }
    }
}
