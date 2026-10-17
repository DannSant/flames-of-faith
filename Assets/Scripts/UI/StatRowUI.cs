using Game.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    // Select/Deselect mirror pointer enter/exit so gamepad navigation shows the same tooltip.
    // Needs a Selectable on the same GameObject to be reachable with the gamepad.
    public class StatRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI statValueText;

        [Header("Class stat star")]
        [Tooltip("Shown for the class's Key (gold) and Complementary (silver) stats.")]
        [SerializeField] private Image roleStar;
        [SerializeField] private Color keyStatColor = new Color(0.95f, 0.76f, 0.31f);
        [SerializeField] private Color complementaryStatColor = new Color(0.78f, 0.82f, 0.86f);

        public Color KeyStatColor => keyStatColor;
        public Color ComplementaryStatColor => complementaryStatColor;

        public void SetRole(StatRole role) => StatRoleStar.Apply(roleStar, role, keyStatColor, complementaryStatColor);

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
