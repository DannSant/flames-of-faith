using Game.Progression;
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

        private void Start()
        {
            UpgradeManager upgradeManager = UpgradeManager.Instance;

            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeOptionsAvailable += ShowUpgradeOptions;
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
                upgradeManager.OnUpgradeOptionsAvailable -= ShowUpgradeOptions;
                upgradeManager.OnNoUpgradeAvailable -= ShowContinueButton;
            }
        }

        private void ShowUpgradeOptions(List<StatValuePair> options)
        {
            ClearUpgradeOptions();
            mainPanel.SetActive(true);
            continueButton.gameObject.SetActive(false);

            foreach (StatValuePair statPair in options)
            {
                UpgradeButtonUI button = Instantiate(upgradeButtonPrefab, mainPanel.transform);
                button.Initialize(statPair.value, statPair.statType);
            }
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