using Game.Progression;
using Game.Scene;
using Game.Waves;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UpgradeSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private UpgradeButtonUI upgradeButtonPrefab;
        [SerializeField] private Button continueButton;

        private List<List<StatValuePair>> upgradeChoiceGroups;
        private int currentChoiceIndex = 0;

        private PlayerProgression playerProgression;

        private void Start()
        {
            playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable += ShowUpgradeChoices;
                upgradeManager.OnNoUpgradeAvailable += ShowContinueButton;
            }

            continueButton.onClick.AddListener(() =>
            {
                mainPanel.SetActive(false);
                WaveSpawner.Instance.ConfirmNextWave();
            });
        }

        private void OnDisable()
        {
            UpgradeManager upgradeManager = UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable -= ShowUpgradeChoices;
                upgradeManager.OnNoUpgradeAvailable -= ShowContinueButton;
            }
        }

        private void ShowUpgradeChoices(List<List<StatValuePair>> choices)
        {
            upgradeChoiceGroups = choices;
            currentChoiceIndex = 0;
            mainPanel.SetActive(true);
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
            mainPanel.SetActive(true);
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
    }


}