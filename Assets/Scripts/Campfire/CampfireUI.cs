using Game.Scene;
using Game.Waves;
using TMPro;
using UnityEngine;

namespace Game.Campfire.UI
{
    public class CampfireUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI infoTxt;

        private CampfireGraceGenerator campfireGraceGenerator;

        private void Start()
        {
            TogglePanel(false);
            MainSceneController.Instance.OnGameplayUISetupRequested += SubscribeToEvents;
        }

        private void OnDisable()
        {
            MainSceneController.Instance.OnGameplayUISetupRequested -= SubscribeToEvents;
            if (campfireGraceGenerator != null)
            {
                campfireGraceGenerator.onPlayerEntersCampfire -= TogglePanel;
            }
        }

        private void SubscribeToEvents()
        {
            campfireGraceGenerator = FindAnyObjectByType<CampfireGraceGenerator>();
            if (campfireGraceGenerator != null)
            {
                campfireGraceGenerator.onPlayerEntersCampfire += TogglePanel;
            }
        }

        private void TogglePanel(bool value)
        {
            mainPanel.SetActive(value);

            if (value && campfireGraceGenerator != null)
            {
                // Unsubscribe after showing the panel once
                campfireGraceGenerator.onPlayerEntersCampfire -= TogglePanel;

                // Update info text
                infoTxt.text = $"You gained {campfireGraceGenerator.GraceAmount} Grace.\r\nGrace improves all damage done.";
            }

        }
        public void OnContinueButtonclicked()
        {
            WaveSpawner.Instance.GoToNextLevel();
        }
    }

}