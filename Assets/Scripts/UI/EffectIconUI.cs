using Game.Effects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    // An item slot: frame tinted by the item's quality, the item icon, and a stack count badge.
    // Select/Deselect mirror pointer enter/exit so gamepad navigation shows the same tooltip.
    // Needs a Selectable on the same GameObject to be reachable with the gamepad.
    public class EffectIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image icon;
        [Tooltip("Optional. Slot frame, tinted with the item's quality color.")]
        [SerializeField] private Image frame;
        [SerializeField] private TextMeshProUGUI quantitytext;
        [Tooltip("Optional. Outline shown while this slot is the selected one (the item bag's Take/Sell target).")]
        [SerializeField] private GameObject selectedMark;
        [SerializeField] private float dimmedAlpha = 0.35f;

        private Effect effect;
        private Action<Effect> showTooltipAction;
        private Action hideTooltipAction;
        private CanvasGroup canvasGroup;
        private bool showingTooltip;

        public Effect Effect => effect;

        public void Setup(Effect effect, Sprite sprite, int quanity, Action<Effect> showTooltipAction, Action hideTooltipAction)
        {
            this.effect = effect;
            icon.sprite = sprite;
            quantitytext.text = FormatQuantity(quanity);

            if (frame != null && effect != null)
            {
                frame.color = EffectQualityDisplayHelper.GetQualityColor(effect.Quality);
            }

            this.showTooltipAction = showTooltipAction;
            this.hideTooltipAction = hideTooltipAction;
        }

        public void SetSelected(bool selected)
        {
            if (selectedMark != null) selectedMark.SetActive(selected);
        }

        /// <summary>The stack badge: empty for a single item, the count otherwise.</summary>
        public static string FormatQuantity(int quantity) => quantity > 1 ? quantity.ToString() : string.Empty;

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

        public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();
        public void OnPointerExit(PointerEventData eventData) => HideTooltip();
        public void OnSelect(BaseEventData eventData) => ShowTooltip();
        public void OnDeselect(BaseEventData eventData) => HideTooltip();

        // Icons are destroyed on every refresh and hidden with the window, neither of which sends
        // pointer exit or deselect: without this the shared tooltip would stay on screen
        private void OnDisable() => HideTooltip();

        private void ShowTooltip()
        {
            showingTooltip = true;
            showTooltipAction?.Invoke(effect);
        }

        private void HideTooltip()
        {
            if (!showingTooltip) return;
            showingTooltip = false;
            hideTooltipAction?.Invoke();
        }
    }
}
