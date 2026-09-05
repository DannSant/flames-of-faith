using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.RunEncounters
{
    public class TreasureRewardIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    }
}
