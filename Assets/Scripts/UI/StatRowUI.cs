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
        private GeneralTooltipPaneUI generalTooltipPaneUI;

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

            generalTooltipPaneUI = FindAnyObjectByType<GeneralTooltipPaneUI>();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }

            generalTooltipPaneUI?.HideTooltip();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }
           
            string statName = StatDisplayNameHelper.GetDisplayName(statType);
            string description = StatUpgradeDatabase.Instance.GetStatDescription(statType);
            generalTooltipPaneUI?.ShowTooltip(description, statName);
        }

        public void UpdateValue(int statValue, Color color)
        {
            statValueText.text = statValue.ToString();
            statValueText.color = color;
        }
    }
}
