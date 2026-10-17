using Game.Progression;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    // Select/Deselect mirror pointer enter/exit so gamepad navigation shows the same tooltip.
    public class UpgradeButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private int upgradeAmount;
        private StatType statToUpgrade;
        // "Now / After" text pushed by the selection UI (computed by gameplay); null if none.
        private string benefitPreview;
        private Button upgradeButton;
        private GeneralTooltipPaneUI generalTooltipPaneUI;

        [SerializeField] private TextMeshProUGUI upgradeText;

        [Header("Class stat star")]
        [Tooltip("Shown for the class's Key (gold) and Complementary (silver) stats.")]
        [SerializeField] private Image roleStar;
        [SerializeField] private Color keyStatColor = new Color(0.95f, 0.76f, 0.31f);
        [SerializeField] private Color complementaryStatColor = new Color(0.78f, 0.82f, 0.86f);

        // "Key stat for the Warrior: ..." pushed by the selection UI; null for other stats.
        private string roleDescription;
        private StatRole role;

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

        public void Initialize(int amount, StatType stat, string benefitPreview = null,
            StatRole role = StatRole.None, string roleDescription = null)
        {
            this.upgradeAmount = amount;
            this.statToUpgrade = stat;
            this.benefitPreview = benefitPreview;
            this.roleDescription = roleDescription;
            this.role = role;
            SetupUI();
            StatRoleStar.Apply(roleStar, role, keyStatColor, complementaryStatColor);

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
            HideTooltip();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            ShowTooltip();
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (GeneralComponentsUI.Instance == null) {
                return;
            }

            string statName = StatDisplayNameHelper.GetDisplayName(statToUpgrade);
            string description = StatRoleStar.AppendRoleLine(
                StatUpgradeDatabase.Instance.GetStatDescription(statToUpgrade), roleDescription,
                role == StatRole.Key ? keyStatColor : complementaryStatColor);
            // The "Now / After" preview goes in the tooltip's second panel, like the stats pane's benefit line.
            string preview = string.IsNullOrEmpty(benefitPreview) ? null : $"<color=#F2C14E>{benefitPreview}</color>";
            generalTooltipPaneUI?.ShowTooltip(description, statName, preview);
        }

        private void HideTooltip()
        {
            if (GeneralComponentsUI.Instance == null)
            {
                return;
            }

            generalTooltipPaneUI?.HideTooltip();
        }
    }

}