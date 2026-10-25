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
        [Tooltip("Title above the choices (\"Select an upgrade\"). Shown while choosing.")]
        [SerializeField] private TextMeshProUGUI titleText;
        [Tooltip("Line under the title while choosing: the level the current choice belongs to.")]
        [SerializeField] private TextMeshProUGUI levelText;
        [Tooltip("{0} is the level the current choice belongs to.")]
        [SerializeField] private string levelFormat = "You are now Level {0}";
        [Tooltip("{0} is the coins gained this wave.")]
        [SerializeField] private string coinsGainedFormat = "Coins gained: {0}";
        [Tooltip("Where the upgrade buttons are spawned. Only its children are cleared between choices.")]
        [SerializeField] private Transform optionsContainer;

        // Pushed by UpgradeManager: the choice groups, preview, first level and the class's stat guide.
        private UpgradeChoices choices;
        private int currentChoiceIndex = 0;

        private PlayerProgression playerProgression;

        // Coins given since the window last closed. Non-combat levels (events, treasures) give none,
        // and the row would show a lone coin icon with an empty text.
        private int coinsGained;

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
            if (!value)
            {
                // The next window only shows the coins given after this one
                coinsGained = 0;
                coinsGainedText.text = "";
            }
            coinsGainedObject.SetActive(value && ShowsCoinsGained(coinsGained));
            mainPanel.SetActive(value);
        }

        /// <summary>The "Coins gained" row shows only when coins were given (combat waves).</summary>
        public static bool ShowsCoinsGained(int coinsGained) => coinsGained > 0;

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
            SetHeader(FormatLevelLine(levelFormat, UpgradeLevels.LevelForChoice(choices.FirstLevel, currentChoiceIndex)));

            foreach (StatValuePair statPair in choices.Groups[currentChoiceIndex])
            {
                UpgradeButtonUI button = Instantiate(upgradeButtonPrefab, OptionsContainer);
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
            SetHeader(null);
        }

        /// <summary>The level line, e.g. "You are now Level 3".</summary>
        public static string FormatLevelLine(string format, int level) => string.Format(format, level);

        // Null hides the title and the level line (e.g. when only the Continue button is left).
        private void SetHeader(string levelLine)
        {
            bool show = !string.IsNullOrEmpty(levelLine);
            if (titleText != null) titleText.gameObject.SetActive(show);
            if (levelText != null)
            {
                levelText.gameObject.SetActive(show);
                if (show) levelText.text = levelLine;
            }
        }

        // Falls back to the main panel for prefabs without an options container
        private Transform OptionsContainer => optionsContainer != null ? optionsContainer : mainPanel.transform;

        private void ClearUpgradeOptions()
        {
            foreach (Transform child in OptionsContainer)
            {
                if (child.gameObject == continueButton.gameObject) continue;
                if (titleText != null && child.gameObject == titleText.gameObject) continue;
                if (levelText != null && child.gameObject == levelText.gameObject) continue;
                Destroy(child.gameObject);
            }
        }

        private void UpdateCoinsGainedText(int coinsGained)
        {
            this.coinsGained = coinsGained;
            coinsGainedText.text = string.Format(coinsGainedFormat, coinsGained);
            if (mainPanel.activeSelf)
            {
                coinsGainedObject.SetActive(ShowsCoinsGained(coinsGained));
            }
        }
    }


}