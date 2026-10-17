using Game.Audio;
using Game.Currency;
using Game.GameSettings;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System;
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
        [Tooltip("Title above the choices. Kept when the choices are cleared.")]
        [SerializeField] private TextMeshProUGUI titleText;
        [Tooltip("Title while choosing; {0} is the level the current choice belongs to.")]
        [SerializeField] private string titleFormat = "You are now Level {0}, pick an upgrade";

        // Pushed by UpgradeManager: the choice groups, preview, first level and the class's stat guide.
        private UpgradeChoices choices;
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
            coinsGainedText.text = "";

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

        private void ShowUpgradeChoices(UpgradeChoices upgradeChoices)
        {
            choices = upgradeChoices;
            currentChoiceIndex = 0;
            TogglePanel(true);
            ShowCurrentChoice();
            PauseManager.Instance.SetPause(true); // Pause the game when showing upgrade choices
            AudioManager.Instance.SetLowVolume(); // Lower music volume during upgrade selection
        }

        private void ShowCurrentChoice()
        {
            ClearUpgradeOptions();

            if (choices?.Groups == null || currentChoiceIndex >= choices.Groups.Count)
            {
                ShowContinueButton();
                return;
            }

            continueButton.gameObject.SetActive(false);
            SetTitle(string.Format(titleFormat, UpgradeLevels.LevelForChoice(choices.FirstLevel, currentChoiceIndex)));

            foreach (StatValuePair statPair in choices.Groups[currentChoiceIndex])
            {
                UpgradeButtonUI button = Instantiate(upgradeButtonPrefab, mainPanel.transform);
                string preview = choices.Preview?.Invoke(statPair.statType, statPair.value);
                var classStat = ClassStatGuide.Find(choices.ClassStats, statPair.statType);
                button.Initialize(statPair.value, statPair.statType, preview,
                    classStat?.role ?? StatRole.None, ClassStatGuide.DescribeRole(classStat, choices.ClassName));
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
            SetTitle(null);
        }

        // Null hides the title (e.g. when only the Continue button is left).
        private void SetTitle(string text)
        {
            if (titleText == null) return;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            if (!string.IsNullOrEmpty(text)) titleText.text = text;
        }

        private void ClearUpgradeOptions()
        {
            foreach (Transform child in mainPanel.transform)
            {
                if (child.gameObject != continueButton.gameObject
                    && (titleText == null || child.gameObject != titleText.gameObject))
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