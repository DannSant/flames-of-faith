using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// One event option button: its label, and its in-world hint in the shared tooltip beside it. No numbers
    /// before choosing; the result narrates them. Select/Deselect mirror pointer enter/exit so gamepad
    /// navigation shows the same tooltip.
    /// </summary>
    public class EventEncounterOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI optionText;

        private string hint;
        private GeneralTooltipPaneUI generalTooltipPaneUI;

        public void Initialize(string label, string hint, GeneralTooltipPaneUI generalTooltipPaneUI)
        {
            this.hint = hint;
            this.generalTooltipPaneUI = generalTooltipPaneUI;
            optionText.text = label;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        public void OnSelect(BaseEventData eventData)
        {
            ShowTooltip();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (string.IsNullOrWhiteSpace(hint))
            {
                return;
            }

            // Beside the option button, not at the pointer
            generalTooltipPaneUI?.ShowTooltip($"<i>{hint}</i>", anchor: (RectTransform)transform);
        }

        public void HideTooltip()
        {
            generalTooltipPaneUI?.HideTooltip();
        }
    }
}
