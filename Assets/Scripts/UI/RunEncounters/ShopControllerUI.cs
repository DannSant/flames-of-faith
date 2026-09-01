using Game.Effects;
using Game.Scene;
using Game.RunEncounters;
using Game.Waves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    public class ShopControllerUI : MonoBehaviour
    {
        [Header("Main UI Elements")]       
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject itemsContainer;

        [Header("Error UI Settings")]
        [SerializeField] private TextMeshProUGUI errorTextUI;     


       [Header("Effects Tooltip UI Elements")]
        [SerializeField] private TextMeshProUGUI effectNameText;
        [SerializeField] private TextMeshProUGUI effectDescriptionText;

        [Header("Prefab Settings")]
        [SerializeField] private ShopIconUI shopIconPrefab;

        private ShopKeeper shopKeeper;

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
                iconUI.Setup(effect, price, BuyItem, ShowEffectTooltip, HideEffectTooltip);
            }

            errorTextUI.gameObject.SetActive(false);

        }

        private void BuyItem(Effect effect)
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

        private void ShowEffectTooltip(Effect effect)
        {
            if (effect == null)
            {
                HideEffectTooltip();
                return;
            }
            effectNameText.text = effect.effectName;
            effectNameText.color = EffectQualityDisplayHelper.GetQualityColor(effect.Quality);
            effectDescriptionText.text = effect.description;
        }

        private void HideEffectTooltip()
        {
            effectNameText.text = string.Empty;
            effectDescriptionText.text = string.Empty;
        }



        private void TogglePanel(bool value)
        {            
            mainPanel.SetActive(value);
            
        }

        public void OnContinueButtonclicked()
        {
            WaveSpawner.Instance.GoToNextLevel();
        }
    }

}