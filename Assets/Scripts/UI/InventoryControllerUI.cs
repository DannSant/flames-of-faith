using Game.Currency;
using Game.Effects;
using Game.Progression;
using Game.Scene;
using Game.RunEncounters;
using Game.Waves;
using System.Collections.Generic;
using System.Linq;
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
        private StatsPaneUI statsPaneUI;
        private readonly List<EffectIconUI> spawnedIcons = new();

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

            statsPaneUI = FindAnyObjectByType<StatsPaneUI>();
            if (statsPaneUI != null)
            {
                statsPaneUI.OnStatHovered += HandleStatHovered;
                statsPaneUI.OnStatHoverEnded += HandleStatHoverEnded;
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

            if (statsPaneUI != null)
            {
                statsPaneUI.OnStatHovered -= HandleStatHovered;
                statsPaneUI.OnStatHoverEnded -= HandleStatHoverEnded;
            }
        }

        private void HandleStatHovered(StatType stat, bool shiftHeld)
        {
            if (!shiftHeld)
            {
                ClearIconDimming();
                return;
            }

            foreach (var icon in spawnedIcons)
            {
                bool grantsStat = icon.Effect.StatModifiers.Any(m => m.stat == stat);
                icon.SetDimmed(!grantsStat);
            }
        }

        private void HandleStatHoverEnded()
        {
            ClearIconDimming();
        }

        private void ClearIconDimming()
        {
            foreach (var icon in spawnedIcons)
            {
                icon.SetDimmed(false);
            }
        }

        private void SetupEvents()
        {
            WaveSpawner waveSpawner = WaveSpawner.Instance;
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveCompleteEnded += ShowInventoryWindow;
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
                waveSpawner.OnWaveCompleteEnded -= ShowInventoryWindow;
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

        //TODO: Optimize to only update changed effects and not destroy all UI elements
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
            spawnedIcons.Clear();
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
                spawnedIcons.Add(iconUI);
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
            effectNameText.color = EffectQualityDisplayHelper.GetQualityColor(effect.Quality);
            effectDescriptionText.text = effect.GetFormattedDescription();
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
