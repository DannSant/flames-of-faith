using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shows the general tooltip while this element is hovered (mouse) or selected (gamepad). The text comes from a
    /// provider set by the owner, so it is built when shown and always reflects current values.
    /// Adds a non-visual Selectable when missing so gamepad navigation can reach the element.
    /// </summary>
    public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private string title;
        private Func<string> descriptionProvider;
        private GeneralTooltipPaneUI tooltipPane;
        private bool isShowing;

        public void Setup(string title, Func<string> descriptionProvider)
        {
            this.title = title;
            this.descriptionProvider = descriptionProvider;

            if (!TryGetComponent<Selectable>(out _))
            {
                var selectable = gameObject.AddComponent<Selectable>();
                selectable.transition = Selectable.Transition.None;
            }

            // Hover needs the graphic to receive raycasts
            if (TryGetComponent(out Graphic graphic))
            {
                graphic.raycastTarget = true;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();
        public void OnPointerExit(PointerEventData eventData) => HideTooltip();
        public void OnSelect(BaseEventData eventData) => ShowTooltip();
        public void OnDeselect(BaseEventData eventData) => HideTooltip();

        private void OnDisable() => HideTooltip();

        // Public for owners that decide themselves when the tooltip shows (e.g. the gamepad on the overworld map)
        public void ShowTooltip()
        {
            if (descriptionProvider == null) return;

            if (tooltipPane == null)
            {
                tooltipPane = FindAnyObjectByType<GeneralTooltipPaneUI>();
                if (tooltipPane == null) return;
            }

            tooltipPane.ShowTooltip(descriptionProvider(), title);
            isShowing = true;
        }

        public void HideTooltip()
        {
            if (!isShowing || tooltipPane == null) return;
            tooltipPane.HideTooltip();
            isShowing = false;
        }
    }
}
