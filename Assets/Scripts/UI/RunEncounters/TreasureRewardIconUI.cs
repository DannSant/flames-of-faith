using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.RunEncounters
{
    // Select/Deselect mirror pointer enter/exit so gamepad navigation shows the same tooltip.
    public class TreasureRewardIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private Action showTooltipAction;
        private Action hideTooltipAction;

        public void Setup(Action showTooltipAction, Action hideTooltipAction)
        {
            this.showTooltipAction = showTooltipAction;
            this.hideTooltipAction = hideTooltipAction;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            showTooltipAction?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hideTooltipAction?.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            showTooltipAction?.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            hideTooltipAction?.Invoke();
        }
    }
}
