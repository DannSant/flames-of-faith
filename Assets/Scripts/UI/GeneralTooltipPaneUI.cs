using Game.Common;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class GeneralTooltipPaneUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private void Awake()
        {
            
            mainPanel.SetActive(false);
        }

        public void ShowTooltip(string description,string title="")
        {
            mainPanel.SetActive(true);
            titleText.SetText(title);
            descriptionText.SetText(description);
        }

        public void HideTooltip()
        {
            mainPanel.SetActive(false);
            titleText.SetText("");
            descriptionText.SetText("");
        }
    }

}