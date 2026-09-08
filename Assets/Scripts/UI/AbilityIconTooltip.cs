using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class AbilityIconTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string title;
        private string description;
        private GeneralTooltipPaneUI generalTooltipPaneUI;

        private void Start()
        {
            generalTooltipPaneUI = FindAnyObjectByType<GeneralTooltipPaneUI>();
        }

        public void SetTooltip(string title, string description)
        {
            this.title = title;
            this.description = description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            generalTooltipPaneUI?.ShowTooltip(description, title);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            generalTooltipPaneUI?.HideTooltip();
        }
    }
}
