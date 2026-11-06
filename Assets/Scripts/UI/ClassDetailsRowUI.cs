using Game.Progression;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A row of the class select's detail tabs: an item (slot frame, icon, name, tooltip), a paragraph, or a
    /// recommended stat (star, text). Selectable, so the gamepad can reach it: selecting it shows the tooltip
    /// like hovering does, and asks the list to scroll it into view.
    /// </summary>
    public class ClassDetailsRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        [Tooltip("Optional. The item's slot frame, tinted with its quality.")]
        [SerializeField] private Image frame;
        [Tooltip("Optional. The item's icon.")]
        [SerializeField] private Image icon;
        [Tooltip("Optional. The stat's gold (Key) or silver (Complementary) star.")]
        [SerializeField] private Image star;

        private Action<RectTransform> showTooltip;
        private Action hideTooltip;
        private bool showingTooltip;

        public event Action<ClassDetailsRowUI> OnRowSelected;

        public string Text => text != null ? text.text : null;

        public void SetText(string value)
        {
            if (text != null) text.text = value;
        }

        public void SetTextColor(Color color)
        {
            if (text != null) text.color = color;
        }

        public void SetIcon(Sprite sprite, Color frameColor)
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }
            if (frame != null) frame.color = frameColor;
        }

        public void SetStar(StatRole role, Color keyColor, Color complementaryColor) =>
            StatRoleStar.Apply(star, role, keyColor, complementaryColor);

        public void SetTooltip(Action<RectTransform> show, Action hide)
        {
            showTooltip = show;
            hideTooltip = hide;
        }

        public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();
        public void OnPointerExit(PointerEventData eventData) => HideTooltip();

        public void OnSelect(BaseEventData eventData)
        {
            ShowTooltip();
            OnRowSelected?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData) => HideTooltip();

        // Rows are destroyed on every tab or class change, which sends no pointer exit or deselect
        private void OnDisable() => HideTooltip();

        private void ShowTooltip()
        {
            if (showTooltip == null) return;
            showingTooltip = true;
            showTooltip((RectTransform)transform);
        }

        private void HideTooltip()
        {
            if (!showingTooltip) return;
            showingTooltip = false;
            hideTooltip?.Invoke();
        }
    }
}
