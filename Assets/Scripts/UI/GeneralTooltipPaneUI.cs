using Game.Common;
using Game.GameSettings;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class GeneralTooltipPaneUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private void Awake()
        {
            mainPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.onPauseToggled += HandlePauseToggled;
            }
        }

        private void OnDisable()
        {
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.onPauseToggled -= HandlePauseToggled;
            }
        }

        private void HandlePauseToggled(bool isPaused)
        {
            // The panels that trigger tooltips (stats, inventory, etc.) get hidden on
            // pause/unpause without firing OnPointerExit, so the tooltip must be
            // force-closed on every pause transition to avoid getting stuck on screen.
            HideTooltip();
        }

        public void ShowTooltip(string description,string title="")
        {
            mainPanel.SetActive(true);
            titleText.SetText(title);
            descriptionText.SetText(description);
        }

        public void HideTooltip()
        {
            mainPanel.SetActive(false);
            titleText.SetText("");
            descriptionText.SetText("");
        }
    }

}