using Game.RunEncounters;
using Game.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// The event window: title, text and one button per option (a column at the bottom), plus a panel beside
    /// it for the event's art when it has any. It shows the screens EventEncounterController pushes (the intro,
    /// then each follow-up step) and reports the picked choice through the screen's callback. An option's
    /// tooltip is its in-world hint, never its numbers. Layout: RunEncounterRestyler.
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
                eventController.OnEventPresented -= ShowScreen;
            }

            CleanupComponent();
        }

        private void SubscribeToEvents()
        {
            eventController = FindAnyObjectByType<EventEncounterController>();
            if (eventController != null)
            {
                eventController.OnEventPresented += ShowScreen;
            }
            

        }

        /// <summary>
        /// Shows one screen of the event: the intro or a step's prompt, and its choices. Picking one hides the
        /// window; the controller then pushes the next step's screen, or the result to the Results window.
        /// </summary>
        private void ShowScreen(EventScreen screen)
        {
            CleanupComponent();
            ShowPanel();
            titleText.text = screen.title;
            descriptionText.text = screen.text;
            if (artPanel != null) artPanel.SetActive(screen.art != null);
            if (artImage != null) artImage.sprite = screen.art;

            for (int i = 0; i < screen.choices.Count; i++)
            {
                int index = i;
                var choice = screen.choices[i];
                var optionUI = Instantiate(optionPrefab, optionsContainer);
                optionUI.Initialize(choice.label, choice.hint, generalTooltipPaneUI);

                var optionButton = optionUI.GetComponent<Button>();
                optionButton.onClick.AddListener(() =>
                {
                    generalTooltipPaneUI?.HideTooltip();
                    HidePanel();
                    CleanupComponent();
                    screen.choose?.Invoke(index);
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
                // Hidden at once: Destroy waits for the end of the frame, and a step's options are spawned
                // in the same frame, so the old buttons would still take layout room and gamepad focus.
                child.gameObject.SetActive(false);
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