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
        [Tooltip("Where the item slots are spawned (the grid). Falls back to the main panel.")]
        [SerializeField] private Transform iconContainer;

        [Header("Currency UI Elements")]
        [SerializeField] private TextMeshProUGUI currencyText;

        [Header("Prefab Settings")]
        [SerializeField] private EffectIconUI effectIconPrefab;

        private EffectStore effectStore;
        private CurrencyWallet currencyWallet;
        private StatsPaneUI statsPaneUI;
        private readonly List<EffectIconUI> spawnedIcons = new();
        // Item tooltips use the scene's shared tooltip pane, like the other windows
        private GeneralTooltipPaneUI tooltipPane;
        private bool showingTooltip;

        private Transform IconContainer => iconContainer != null ? iconContainer : mainPanel.transform;

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

            // Same rule as the Stats pane's Shift list, stat bonus rules included (e.g. the Emblems)
            foreach (var icon in spawnedIcons)
            {
                icon.SetStatFilter(icon.Effect.GrantsStat(stat) ? StatFilter.Grants : StatFilter.Other);
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
                icon.SetStatFilter(StatFilter.None);
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
            foreach (Transform child in IconContainer)
            {
                var layoutElement = child.GetComponent<LayoutElement>();
                if(layoutElement!=null && layoutElement.ignoreLayout)
                {
                    continue;
                }
                Destroy(child.gameObject);
            }
            spawnedIcons.Clear();

            var effectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>();
            if (effectStore == null)
            {
                Debug.LogWarning("EffectStore component not found on player.");
                return;
            }

            var effects = effectStore.ActiveEffects;

            foreach(var effectInstance in effects)
            {
                EffectIconUI iconUI = Instantiate(effectIconPrefab, IconContainer);
                iconUI.Setup(effectInstance.effect, effectInstance.effect.EffectIcon, effectInstance.count, ShowEffectTooltip, HideEffectTooltip);
                spawnedIcons.Add(iconUI);
            }

        }

        private void ShowEffectTooltip(Effect effect)
        {
            if (effect == null)
            {
                HideEffectTooltip();
                return;
            }

            // UI to UI lookup (the pane lives in the same scene), cached after the first time
            if (tooltipPane == null)
            {
                tooltipPane = FindAnyObjectByType<GeneralTooltipPaneUI>();
                if (tooltipPane == null) return;
            }

            tooltipPane.ShowTooltip(effect.GetDescriptionText(), EffectQualityDisplayHelper.ColorizeName(effect), null, effect.GetStatLines());
            showingTooltip = true;
        }

        // Only closes the tooltip if the inventory opened it, so a stat tooltip shown meanwhile stays
        private void HideEffectTooltip()
        {
            if (!showingTooltip || tooltipPane == null) return;
            tooltipPane.HideTooltip();
            showingTooltip = false;
        }

        public void HideInventoryWindow(int _)
        {
            // Hide the inventory panel
            HideEffectTooltip();
            TogglePanel(false);
        }
    }
}
