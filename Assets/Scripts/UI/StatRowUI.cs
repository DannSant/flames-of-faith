using Game.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class StatRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI statValueText;

        private StatType statType;

        public void Initialize(string statName, int statValue, Color color, StatType statType)
        {
            this.statType = statType;
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

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }
            var tooltipPanel = GeneralComponentsUI.Instance.GeneralTooltipPaneUI;
            tooltipPanel.HideTooltip();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }
            var tooltipPanel = GeneralComponentsUI.Instance.GeneralTooltipPaneUI;
            string statName = StatDisplayNameHelper.GetDisplayName(statType);
            string description = StatUpgradeDatabase.Instance.GetStatDescription(statType);
            tooltipPanel.ShowTooltip(description, statName);
        }
    }
}
