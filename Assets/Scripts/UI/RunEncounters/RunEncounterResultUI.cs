using Game.RunEncounters;
using Game.Scene;
using Game.Waves;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    public class RunEncounterResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI resultsText;
        [SerializeField] private float displayDuration = 5f;

        [SerializeField] private Button okButton;

        private EventEncounterController eventController;
        private Coroutine autoHideRoutine;

        private void Awake()
        {
            mainPanel.SetActive(false);
            okButton.onClick.AddListener(OnOkButtonClicked);
        }


        private void Start()
        {
            MainSceneController.Instance.OnGameplayUISetupRequested += SubscribeToEvents;
        }

        private void OnDisable()
        {
            MainSceneController.Instance.OnGameplayUISetupRequested -= SubscribeToEvents;
            if (eventController != null)
            {
                eventController.OnEventResolved -= ShowResults;
            }
        }

        private void SubscribeToEvents()
        {
            eventController = FindAnyObjectByType<EventEncounterController>();
            if (eventController != null)
            {
                eventController.OnEventResolved += ShowResults;
            }
        }

        public void ShowResults(string results)
        {
            if (autoHideRoutine != null)
            {
                StopCoroutine(autoHideRoutine);
            }

            resultsText.text = results;
            mainPanel.SetActive(true);           
        }

        public void HideResults()
        {
            if (autoHideRoutine != null)
            {
                StopCoroutine(autoHideRoutine);
                autoHideRoutine = null;
            }

            mainPanel.SetActive(false);
        }

        private void OnOkButtonClicked()
        {
            HideResults();
            WaveSpawner.Instance.InvokeOnWaveComplete();
        }

        private IEnumerator AutoHideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            autoHideRoutine = null;
            mainPanel.SetActive(false);
        }
    }
}
