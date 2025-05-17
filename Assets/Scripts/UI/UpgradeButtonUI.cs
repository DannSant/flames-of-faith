using Game.Progression;
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

            upgradeButton.onClick.AddListener(() =>
            {
                UpgradeManager upgradeManager = UpgradeManager.Instance;
                if (upgradeManager != null)
                {
                    upgradeManager.ApplyUpgrade(statToUpgrade, upgradeAmount);
                }

                // Optionally, hide the UI immediately
                gameObject.transform.parent.gameObject.SetActive(false);
            });
        }
    }

}