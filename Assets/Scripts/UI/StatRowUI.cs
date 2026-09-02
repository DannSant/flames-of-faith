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
        private StatsPaneUI owner;

        public void Initialize(string statName, int statValue, Color color, StatType statType, StatsPaneUI owner)
        {
            this.statType = statType;
            this.owner = owner;
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

            owner?.NotifyHoverEnded();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }

            owner?.NotifyHover(statType);
        }

        public void UpdateValue(int statValue, Color color)
        {
            statValueText.text = statValue.ToString();
            statValueText.color = color;
        }
    }
}
