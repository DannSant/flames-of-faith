using Game.Effects;
using Game.UI.Style;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>How a slot shows under the Inventory's Shift view (items granting the hovered stat).</summary>
    public enum StatFilter { None, Grants, Other }

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
            EnsureCanvasGroup().alpha = dimmed ? dimmedAlpha : 1f;
        }

        private CanvasGroup EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            return canvasGroup;
        }

        /// <summary>
        /// The Inventory's Shift view: the items granting the hovered stat get the selected ring, the others
        /// fade and darken (UISettings), much more than the shop's dimming. None restores the slot.
        /// </summary>
        public void SetStatFilter(StatFilter filter)
        {
            var settings = UISettings.Instance;
            SetSelected(filter == StatFilter.Grants);
            if (icon != null) icon.color = GetStatFilterColor(Color.white, filter, settings);
            if (frame != null && effect != null)
            {
                frame.color = GetStatFilterColor(EffectQualityDisplayHelper.GetQualityColor(effect.Quality), filter, settings);
            }
            EnsureCanvasGroup().alpha = GetStatFilterAlpha(filter, settings);
        }

        /// <summary>A slot color under the Shift view: darkened by the filter tint for the items that don't grant the stat.</summary>
        public static Color GetStatFilterColor(Color color, StatFilter filter, UISettings settings) =>
            filter == StatFilter.Other ? color * settings.StatFilterTint : color;

        /// <summary>A slot's opacity under the Shift view: faded for the items that don't grant the stat.</summary>
        public static float GetStatFilterAlpha(StatFilter filter, UISettings settings) =>
            filter == StatFilter.Other ? settings.StatFilterAlpha : 1f;

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
