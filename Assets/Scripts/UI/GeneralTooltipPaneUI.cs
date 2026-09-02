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

        [Header("Expanded Mode (e.g. Shift+Hover sources)")]
        [SerializeField] private GameObject sourcesSection;
        [SerializeField] private TextMeshProUGUI sourcesText;
        [SerializeField] private Vector2 expandedSize;

        private RectTransform panelRectTransform;
        private Vector2 normalSize;

        private void Awake()
        {
            panelRectTransform = mainPanel.GetComponent<RectTransform>();
            if (panelRectTransform != null)
            {
                normalSize = panelRectTransform.sizeDelta;
            }
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

        public void ShowTooltip(string description, string title = "", string sourcesText = null)
        {
            mainPanel.SetActive(true);
            titleText.SetText(title);
            descriptionText.SetText(description);

            bool showSources = !string.IsNullOrEmpty(sourcesText);
            if (sourcesSection != null)
            {
                sourcesSection.SetActive(showSources);
            }
            if (this.sourcesText != null)
            {
                this.sourcesText.SetText(showSources ? sourcesText : "");
            }
            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = showSources ? expandedSize : normalSize;
            }
        }

        public void HideTooltip()
        {
            mainPanel.SetActive(false);
            titleText.SetText("");
            descriptionText.SetText("");

            if (sourcesSection != null)
            {
                sourcesSection.SetActive(false);
            }
            if (sourcesText != null)
            {
                sourcesText.SetText("");
            }
            if (panelRectTransform != null)
            {
                panelRectTransform.sizeDelta = normalSize;
            }
        }
    }

}