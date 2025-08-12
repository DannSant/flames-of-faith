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
        private GeneralTooltipPaneUI generalTooltipPaneUI;

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

            generalTooltipPaneUI = FindAnyObjectByType<GeneralTooltipPaneUI>();
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

            generalTooltipPaneUI?.HideTooltip();
            OnUpgradeSelected?.Invoke(statToUpgrade, upgradeAmount);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }

            generalTooltipPaneUI?.HideTooltip();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (GeneralComponentsUI.Instance == null) {
                return;
            }
           
            string statName = StatDisplayNameHelper.GetDisplayName(statToUpgrade);
            string description = StatUpgradeDatabase.Instance.GetStatDescription(statToUpgrade);
            generalTooltipPaneUI?.ShowTooltip(description, statName);
        }
    }

}