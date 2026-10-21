using Game.Effects;
using Game.Scene;
using Game.RunEncounters;
using Game.UI.Style;
using Game.Waves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// The shop window: a grid of item cells (the Inventory slot plus the price), Continue to leave. Hovering or
    /// selecting an item shows the shared tooltip beside it, with the price; items the player can't afford are
    /// dimmed (ShopKeeper decides). Opens beside the Inventory and the Stats pane. Layout: ShopRestyler.
    /// </summary>
    public class ShopControllerUI : MonoBehaviour
    {
        [Header("Main UI Elements")]       
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject itemsContainer;

        [Header("Error UI Settings")]
        [SerializeField] private TextMeshProUGUI errorTextUI;     


        [Header("Prefab Settings")]
        [SerializeField] private ShopIconUI shopIconPrefab;

        [Header("Continue")]
        [SerializeField] private Button continueButton;

        [Header("Tooltip")]
        [Tooltip("The tooltip's price line. {0} = price.")]
        [SerializeField] private string priceFormat = "Price: {0} coins";
        [Tooltip("Added to the price line (in red) when the player can't afford the item.")]
        [SerializeField] private string notEnoughText = "Not enough coins";

        [Header("Leave Confirmation")]
        [Tooltip("Asks before leaving, since the shop can't be revisited. Without one, Continue leaves directly.")]
        [SerializeField] private ConfirmDialogUI confirmDialog;
        [TextArea]
        [SerializeField] private string leaveShopWarning = "Are you sure you want to leave? You cannot go back to this shop";

        [Header("Buy Confirmation")]
        [Tooltip("Asks before buying, on the same dialog. {0} = item name (in its quality color), {1} = price. " +
            "Items the player can't afford skip the question and show the error right away.")]
        [TextArea]
        [SerializeField] private string buyItemQuestion = "Buy {0} for {1} coins?";

        private ShopKeeper shopKeeper;
        private GeneralTooltipPaneUI tooltipPane;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonclicked);
        }

        private void OnDestroy()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueButtonclicked);
        }

        private void Start()
        {
            TogglePanel(false);
            MainSceneController.Instance.OnGameplayUISetupRequested += SubscribeToEvents;
        }

        private void SubscribeToEvents()
        {
            var foundShopKeeper = FindAnyObjectByType<ShopKeeper>();
            if (foundShopKeeper != null)
            {
                foundShopKeeper.onShopWindowToggle += ToggleShopUI;
            }
        }

        private void OnDisable()
        {
            MainSceneController.Instance.OnGameplayUISetupRequested -= SubscribeToEvents;
            var foundShopKeeper = FindAnyObjectByType<ShopKeeper>();
            if (foundShopKeeper != null)
            {
                foundShopKeeper.onShopWindowToggle -= ToggleShopUI;
            }
        }

        private void ToggleShopUI(bool value, List<Effect> items, ShopKeeper shopKeeper)
        {
            this.shopKeeper = shopKeeper;
            TogglePanel(value);
            PopulateShopGrid(items);
            HideEffectTooltip();

        }

        private void PopulateShopGrid(List<Effect> items)
        {
            foreach (Transform child in itemsContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var effect in items)
            {
                ShopIconUI iconUI = Instantiate(shopIconPrefab, itemsContainer.transform);
                int price = shopKeeper != null ? shopKeeper.GetDiscountedPrice(effect) : effect.BuyPrice;
                bool affordable = shopKeeper == null || shopKeeper.CanAfford(effect);
                iconUI.Setup(effect, price, affordable, BuyItem, ShowEffectTooltip, HideEffectTooltip);
            }

            errorTextUI.gameObject.SetActive(false);

        }

        // Clicking an item asks first; only an affordable item gets the question, so a "Yes" can't fail.
        private void BuyItem(Effect effect)
        {
            if (shopKeeper == null || effect == null)
            {
                return;
            }

            if (confirmDialog == null)
            {
                ConfirmBuyItem(effect);
                return;
            }
            if (confirmDialog.IsOpen) return;

            if (!shopKeeper.CanAfford(effect))
            {
                errorTextUI.gameObject.SetActive(true);
                return;
            }

            string itemName = $"<color=#{ColorUtility.ToHtmlStringRGB(EffectQualityDisplayHelper.GetQualityColor(effect.Quality))}>{effect.effectName}</color>";
            confirmDialog.Show(string.Format(buyItemQuestion, itemName, shopKeeper.GetDiscountedPrice(effect)),
                () => ConfirmBuyItem(effect));
        }

        private void ConfirmBuyItem(Effect effect)
        {
            if (shopKeeper == null)
            {
                return;
            }

            bool result = shopKeeper.BuyItem(effect);
            if (result)
            {
                //refresh items the item
                PopulateShopGrid(shopKeeper.GetAvailableItems());
            }
            else
            {
                //show error message
                errorTextUI.gameObject.SetActive(true);
            }
        }

        // The shared tooltip, beside the item's slot. UI-to-UI lookup only.
        private void ShowEffectTooltip(Effect effect, RectTransform slot)
        {
            if (effect == null)
            {
                HideEffectTooltip();
                return;
            }
            if (tooltipPane == null) tooltipPane = FindAnyObjectByType<GeneralTooltipPaneUI>();
            if (tooltipPane == null) return;

            int price = shopKeeper != null ? shopKeeper.GetDiscountedPrice(effect) : effect.BuyPrice;
            bool affordable = shopKeeper == null || shopKeeper.CanAfford(effect);
            var settings = UISettings.Instance;
            string negativeHex = settings != null ? settings.ToHex(UIColorRole.Negative) : "FF4545";
            tooltipPane.ShowTooltip(effect.GetDescriptionText(), EffectQualityDisplayHelper.ColorizeName(effect),
                FormatPriceLine(price, affordable, priceFormat, notEnoughText, negativeHex), effect.GetStatLines(), slot);
        }

        private void HideEffectTooltip()
        {
            if (tooltipPane != null) tooltipPane.HideTooltip();
        }

        /// <summary>The tooltip's price line: "Price: 25 coins", in red with "Not enough coins" when unaffordable.</summary>
        public static string FormatPriceLine(int price, bool affordable, string priceFormat, string notEnoughText, string negativeHex)
        {
            string line = string.Format(priceFormat, price);
            return affordable ? line : $"<color=#{negativeHex}>{line} - {notEnoughText}</color>";
        }



        private void TogglePanel(bool value)
        {            
            mainPanel.SetActive(value);
            
        }

        public void OnContinueButtonclicked()
        {
            if (confirmDialog == null)
            {
                LeaveShop();
                return;
            }
            if (confirmDialog.IsOpen) return;

            confirmDialog.Show(leaveShopWarning, LeaveShop);
        }

        private void LeaveShop()
        {
            HideEffectTooltip();
            WaveSpawner.Instance.GoToNextLevel();
        }
    }

}