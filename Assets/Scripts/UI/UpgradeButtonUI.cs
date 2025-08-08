using Game.Progression;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

        private void OnDisable()
        {
            upgradeButton.onClick.RemoveListener(OnButtonClicked);
            OnUpgradeSelected = null;
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

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }
            var tooltipPanel = GeneralComponentsUI.Instance.GeneralTooltipPaneUI;
            tooltipPanel.HideTooltip();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null) {
                return;
            }
            var tooltipPanel = GeneralComponentsUI.Instance.GeneralTooltipPaneUI;
            string statName = StatDisplayNameHelper.GetDisplayName(statToUpgrade);
            string description = StatUpgradeDatabase.Instance.GetStatDescription(statToUpgrade);
            tooltipPanel.ShowTooltip(description, statName);
        }
    }

}