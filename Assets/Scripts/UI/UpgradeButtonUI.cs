using Game.Progression;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UpgradeButtonUI : MonoBehaviour
    {
        private int upgradeAmount;
        private StatType statToUpgrade;
        private Button upgradeButton;

        [SerializeField] private TextMeshProUGUI upgradeText;

        public event Action<StatType, int> OnUpgradeSelected;

        private void Awake()
        {
            upgradeButton = GetComponent<Button>();
        }

        public void Initialize(int amount, StatType stat)
        {
            this.upgradeAmount = amount;
            this.statToUpgrade = stat;
            SetupUI();
        }

        private void SetupUI() 
        {
            string displayStatName = StatDisplayNameHelper.GetDisplayName(statToUpgrade);
            upgradeText.SetText($"+{upgradeAmount} {displayStatName}");
            // Ensure previous listeners are cleared to avoid duplicates
            upgradeButton.onClick.RemoveAllListeners();

            upgradeButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnUpgradeSelected?.Invoke(statToUpgrade, upgradeAmount);
        }
    }

}