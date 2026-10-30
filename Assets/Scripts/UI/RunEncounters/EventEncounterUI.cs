using Game.RunEncounters;
using Game.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// The event window: title, text and one button per option (a column at the bottom), plus a panel beside
    /// it for the event's art when it has any. Layout: RunEncounterRestyler.
    /// </summary>
    public class EventEncounterUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [Tooltip("Panel beside the window that shows the event's art. Hidden for events without art.")]
        [SerializeField] private GameObject artPanel;
        [SerializeField] private Image artImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private EventEncounterOptionUI optionPrefab;

        private GeneralTooltipPaneUI generalTooltipPaneUI;


        private EventEncounterController eventController;

        private void Start()
        {
            generalTooltipPaneUI = FindAnyObjectByType<GeneralTooltipPaneUI>();
            HidePanel();
            MainSceneController.Instance.OnGameplayUISetupRequested += SubscribeToEvents;
        }

        private void OnDisable()
        {
            MainSceneController.Instance.OnGameplayUISetupRequested -= SubscribeToEvents;
            if (eventController != null)
            {
                eventController.OnEventPresented -= UpdateEventEncounterData;
            }

            CleanupComponent();
        }

        private void SubscribeToEvents()
        {
            eventController = FindAnyObjectByType<EventEncounterController>();
            if (eventController != null)
            {
                eventController.OnEventPresented += UpdateEventEncounterData;
            }
            

        }

        private void UpdateEventEncounterData(EventEncounterData data)
        {
            ShowPanel();
            titleText.text = data.title;
            descriptionText.text = data.fullText;
            if (artPanel != null) artPanel.SetActive(data.art != null);
            if (artImage != null) artImage.sprite = data.art;
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var option in data.legacyOptions)
            {
                var optionUI = Instantiate(optionPrefab, optionsContainer);
                optionUI.Initialize(option, generalTooltipPaneUI);

                var optionButton = optionUI.GetComponent<Button>();
                optionButton.onClick.AddListener(() =>
                {
                    eventController.SelectOption(option);
                    HidePanel();
                    CleanupComponent();
                    generalTooltipPaneUI?.HideTooltip();
                });
            }
        }

        private void CleanupComponent()
        {
            titleText.text ="";
            descriptionText.text = "";

            if(optionsContainer == null) return;

            foreach (Transform child in optionsContainer)
            {
                var optionButton = child.GetComponent<Button>();
                optionButton.onClick.RemoveAllListeners();
                Destroy(child.gameObject);
            }
        }

        private void ShowPanel()
        {
            mainPanel.SetActive(true);
        }

        private void HidePanel()
        {
            mainPanel.SetActive(false);
        }
    }

}