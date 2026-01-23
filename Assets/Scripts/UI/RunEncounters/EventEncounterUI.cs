using Game.RunEncounters;
using Game.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    public class EventEncounterUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private EventEncounterOptionUI optionPrefab;


        private EventEncounterController eventController;

        private void Start()
        {
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
            backgroundImage.sprite = data.backgroundArt;
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var option in data.options)
            {
                var optionUI = Instantiate(optionPrefab, optionsContainer);
                optionUI.Initialize(option.OptionDescription);

                var optionButton = optionUI.GetComponent<Button>();
                optionButton.onClick.AddListener(() =>
                {
                    eventController.SelectOption(option);
                    HidePanel();
                    CleanupComponent();
                });
            }
        }

        private void CleanupComponent()
        {
            titleText.text ="";
            descriptionText.text = "";
            backgroundImage.sprite = null;

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