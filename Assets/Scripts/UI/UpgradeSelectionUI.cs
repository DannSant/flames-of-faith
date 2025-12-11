using Game.Audio;
using Game.Currency;
using Game.GameSettings;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UpgradeSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject coinsGainedObject;
        [SerializeField] private UpgradeButtonUI upgradeButtonPrefab;
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI coinsGainedText;

        private List<List<StatValuePair>> upgradeChoiceGroups;
        private int currentChoiceIndex = 0;

        private PlayerProgression playerProgression;

        private void Start()
        {
            TogglePanel(false);
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable += ShowUpgradeChoices;
                upgradeManager.OnNoUpgradeAvailable += ShowContinueButton;
            }

            continueButton.onClick.AddListener(() =>
            {
                TogglePanel(false);
                PauseManager.Instance.SetPause(false);
                WaveSpawner.Instance.ConfirmNextWave();
                AudioManager.Instance.ResetMusicVolume(); // Reset music volume when continuing
                var generalTooltipPaneUI = FindAnyObjectByType<GeneralTooltipPaneUI>();
                generalTooltipPaneUI?.HideTooltip();
            });

            CurrencyGenerator currencyGenerator = CurrencyGenerator.Instance;
            if (currencyGenerator != null)
            {
                currencyGenerator.OnCurrencyGenerated += UpdateCoinsGainedText;
            }
        }

        private void OnDisable()
        {
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable -= ShowUpgradeChoices;
                upgradeManager.OnNoUpgradeAvailable -= ShowContinueButton;
            }
            CurrencyGenerator currencyGenerator = CurrencyGenerator.Instance;
            if (currencyGenerator != null)
            {
                currencyGenerator.OnCurrencyGenerated -= UpdateCoinsGainedText;
            }
        }

        private void TogglePanel(bool value)
        {
            coinsGainedObject.SetActive(value);
            mainPanel.SetActive(value);
        }

        private void ShowUpgradeChoices(List<List<StatValuePair>> choices)
        {
          
            upgradeChoiceGroups = choices;
            currentChoiceIndex = 0;
            TogglePanel(true);
            ShowCurrentChoice();
            PauseManager.Instance.SetPause(true); // Pause the game when showing upgrade choices
            AudioManager.Instance.SetMusicVolume(0.2f); // Lower music volume during upgrade selection
        }

        private void ShowCurrentChoice()
        {
            ClearUpgradeOptions();

            if (upgradeChoiceGroups == null || currentChoiceIndex >= upgradeChoiceGroups.Count)
            {
                ShowContinueButton();
                return;
            }

            continueButton.gameObject.SetActive(false);

            foreach (StatValuePair statPair in upgradeChoiceGroups[currentChoiceIndex])
            {
                UpgradeButtonUI button = Instantiate(upgradeButtonPrefab, mainPanel.transform);
                button.Initialize(statPair.value, statPair.statType);
                button.OnUpgradeSelected += HandleUpgradeSelected;
            }
        }

        private void HandleUpgradeSelected(StatType statType, int amount)
        {
            playerProgression.UpdateStat(statType, amount);
            currentChoiceIndex++;
            ShowCurrentChoice();
        }

        private void ShowContinueButton()
        {
            ClearUpgradeOptions();
            PauseManager.Instance.SetPause(true); // Pause the game when showing the continue panel
            TogglePanel(true);
            continueButton.gameObject.SetActive(true);
        }

        private void ClearUpgradeOptions()
        {
            foreach (Transform child in mainPanel.transform)
            {
                if (child.gameObject != continueButton.gameObject)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void UpdateCoinsGainedText(int coinsGained)
        {
            /*if (!mainPanel.activeInHierarchy)
            {
                TogglePanel(true);
            }*/
            coinsGainedText.text = $"Coins Gained: {coinsGained}";
        }
    }


}