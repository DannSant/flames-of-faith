using Game.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    // Select/Deselect mirror pointer enter/exit so gamepad navigation shows the same tooltip.
    // Needs a Selectable on the same GameObject to be reachable with the gamepad.
    public class StatRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
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

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }

            owner?.NotifyHover(statType);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }

            owner?.NotifyHoverEnded();
        }

        public void UpdateValue(int statValue, Color color)
        {
            statValueText.text = statValue.ToString();
            statValueText.color = color;
        }
    }
}
