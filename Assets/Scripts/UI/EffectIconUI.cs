using Game.Effects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    // Select/Deselect mirror pointer enter/exit so gamepad navigation shows the same tooltip.
    // Needs a Selectable on the same GameObject to be reachable with the gamepad.
    public class EffectIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantitytext;
        [SerializeField] private float dimmedAlpha = 0.35f;

        private Effect effect;
        private Action<Effect> showTooltipAction;
        private Action hideTooltipAction;
        private CanvasGroup canvasGroup;

        public Effect Effect => effect;

        public void Setup(Effect effect, Sprite sprite, int quanity, Action<Effect> showTooltipAction, Action hideTooltipAction)
        {
            this.effect = effect;
            icon.sprite = sprite;
            quantitytext.text = quanity.ToString();

            this.showTooltipAction = showTooltipAction;
            this.hideTooltipAction = hideTooltipAction;
        }

        public void SetDimmed(bool dimmed)
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            canvasGroup.alpha = dimmed ? dimmedAlpha : 1f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           
            showTooltipAction?.Invoke(effect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hideTooltipAction?.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            showTooltipAction?.Invoke(effect);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            hideTooltipAction?.Invoke();
        }

    }
}