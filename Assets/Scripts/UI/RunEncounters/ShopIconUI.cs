using Game.Effects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    public class ShopIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button button;

        private Effect effect;
        private Action<Effect> showTooltipAction;
        private Action hideTooltipAction;

        public void Setup(Effect effect, Action<Effect> clickAction, Action<Effect> showTooltipAction, Action hideTooltipAction)
        {
            this.effect = effect;
            icon.sprite = effect.EffectIcon;
            costText.text = effect.BuyPrice.ToString();

            this.showTooltipAction = showTooltipAction;
            this.hideTooltipAction = hideTooltipAction;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => clickAction?.Invoke(effect));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            showTooltipAction?.Invoke(effect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hideTooltipAction?.Invoke();
        }
    }
}
