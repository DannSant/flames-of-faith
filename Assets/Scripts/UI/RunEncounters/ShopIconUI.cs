using Game.Effects;
using Game.UI.Style;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// A shop cell: the Inventory's item slot (quality frame, icon; it shows and hides the tooltip on hover and
    /// gamepad select) with the price under it. Items the player can't afford are dimmed with a red price; they
    /// can still be clicked, which shows the shop's error. Layout: ShopRestyler.
    /// </summary>
    public class ShopIconUI : MonoBehaviour
    {
        [SerializeField] private EffectIconUI slot;
        [SerializeField] private Button slotButton;
        [SerializeField] private TextMeshProUGUI costText;

        public void Setup(Effect effect, int displayPrice, bool affordable, Action<Effect> clickAction,
            Action<Effect, RectTransform> showTooltipAction, Action hideTooltipAction)
        {
            var slotRect = (RectTransform)slot.transform;
            slot.Setup(effect, effect.EffectIcon, 1, e => showTooltipAction?.Invoke(e, slotRect), hideTooltipAction);
            slot.SetDimmed(!affordable);

            costText.text = displayPrice.ToString();
            var settings = UISettings.Instance;
            if (settings != null) costText.color = settings.GetColor(affordable ? UIColorRole.Text : UIColorRole.Negative);

            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => clickAction?.Invoke(effect));
        }
    }
}
