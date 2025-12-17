using Game.Currency;
using Game.Effects;
using Game.Scene;
using Game.Shop;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class InventoryControllerUI : MonoBehaviour
    {
        [Header("Main UI Elements")]
        [SerializeField] private GameObject titleObject;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private Image backgroundImage;

        [Header("Currency UI Elements")]
        [SerializeField] private TextMeshProUGUI currencyText;

        [Header("Effects Tooltip UI Elements")]
        [SerializeField] private TextMeshProUGUI effectNameText;
        [SerializeField] private TextMeshProUGUI effectDescriptionText;

        [Header("Prefab Settings")]
        [SerializeField] private EffectIconUI effectIconPrefab;

        private EffectStore effectStore;
        private CurrencyWallet currencyWallet;

        private void Start()
        {
            TogglePanel(false);

        }

        private void OnEnable()
        {
            var mainSceneController = MainSceneController.Instance;
            if (mainSceneController != null)
            {
                mainSceneController.OnGameplayUISetupRequested += SetupEvents;
            }

            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            if (effectStore != null)
            {
                effectStore.OnEffectAdded += OnEffectAdded;
                
            }
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            if (currencyWallet != null)
            {
                currencyWallet.OnCurrencyChanged += OnUpdatedCurrency;
            }
        }

        private void OnDisable()
        {
            var mainSceneController = MainSceneController.Instance;
            if (mainSceneController != null)
            {
                mainSceneController.OnGameplayUISetupRequested -= SetupEvents;
            }

            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            if (effectStore != null)
            {
                effectStore.OnEffectAdded -= OnEffectAdded;

            }
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            if (currencyWallet != null)
            {
                currencyWallet.OnCurrencyChanged -= OnUpdatedCurrency;
            }
        }

        private void SetupEvents()
        {
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveComplete += ShowInventoryWindow;
                waveSpawner.OnWaveStarted += HideInventoryWindow;
            }

            var shopKeeper = FindAnyObjectByType<ShopKeeper>();
            if (shopKeeper != null)
            {
                shopKeeper.onShopWindowOpened += ShowInventoryWindow;
                shopKeeper.onInventoryWindowRefresh += RefreshInventoryWindow;
            }

        }

        private void OnDestroy()
        {
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveComplete -= ShowInventoryWindow;
                waveSpawner.OnWaveStarted -= HideInventoryWindow;
            }

            var shopKeeper = FindAnyObjectByType<ShopKeeper>();
            if (shopKeeper != null)
            {
                shopKeeper.onShopWindowOpened -= ShowInventoryWindow;
                shopKeeper.onInventoryWindowRefresh -= RefreshInventoryWindow;
            }
            effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            if (effectStore != null)
            {
                effectStore.OnEffectAdded -= OnEffectAdded;

            }
            currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            if (currencyWallet != null)
            {
                currencyWallet.OnCurrencyChanged -= OnUpdatedCurrency;
            }
        }

        private void TogglePanel(bool value)
        {
            titleObject.SetActive(value);
            mainPanel.SetActive(value);
            backgroundImage.enabled = value;
        }

        public void ShowInventoryWindow()
        {
            // Show the inventory panel
            TogglePanel(true);
            RefreshInventoryWindow();
        }

        private void OnEffectAdded(Effect effect)
        {
            RefreshInventoryWindow();
        }
        private void OnUpdatedCurrency(int newAmount)
        {
            UpdateCurrencyText();
        }

        private void RefreshInventoryWindow()
        {
            UpdateCurrencyText();
            UpdateEffectsDisplay();
        }

        private void UpdateCurrencyText() 
        {
            CurrencyWallet currencyWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>();
            currencyText.text = currencyWallet != null ? currencyWallet.CurrencyAmount.ToString() : "0";
        }

        private void UpdateEffectsDisplay()
        {
            //Clear previous effect display
            foreach (Transform child in mainPanel.transform)
            {
                var layoutElement = child.GetComponent<LayoutElement>();
                if(layoutElement!=null && layoutElement.ignoreLayout)
                {
                    continue;
                }
                Destroy(child.gameObject);
            }
            effectNameText.text = string.Empty;
            effectDescriptionText.text = string.Empty;

            var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            if (effectStore == null)
            {
                Debug.LogWarning("EffectStore component not found on player.");
                return;
            }

            var effects = effectStore.ActiveEffects;

            foreach(var effectInstance in effects)
            {
                EffectIconUI iconUI = Instantiate(effectIconPrefab, mainPanel.transform);
                iconUI.Setup(effectInstance.effect, effectInstance.effect.EffectIcon, effectInstance.count, ShowEffectTooltip, HideEffectTooltip);
            }

        }

        private void ShowEffectTooltip(Effect effect)
        {          
            if (effect == null)
            {
                effectNameText.text = string.Empty;
                effectDescriptionText.text = string.Empty;
                return;
            }
            effectNameText.text = effect.effectName;
            effectDescriptionText.text = effect.description;
        }

        private void HideEffectTooltip()
        {           
            effectNameText.text = string.Empty;
            effectDescriptionText.text = string.Empty;
        }

        public void HideInventoryWindow(int _)
        {
            // Hide the inventory panel
            TogglePanel(false);
        }
    }
}
