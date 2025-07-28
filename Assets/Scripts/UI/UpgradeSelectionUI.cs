using Game.Currency;
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
                WaveSpawner.Instance.ConfirmNextWave();
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
            if (!mainPanel.activeInHierarchy)
            {
                TogglePanel(true);
            }
            coinsGainedText.text = $"Coins Gained: {coinsGained}";
        }
    }


}