using Game.RunEncounters;
using Game.Scene;
using Game.UI.Style;
using Game.Waves;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// The Results window: what an event option (or a currency / experience treasure) gave and took, one line
    /// each, gains in green and losses in red. Continue ends the encounter. Layout: RunEncounterRestyler.
    /// </summary>
    public class RunEncounterResultUI : MonoBehaviour
    {
        private const string GainedPrefix = "Gained";
        private const string LostPrefix = "Lost";

        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI resultsText;

        [SerializeField] private Button okButton;

        // One-time lookups purely to attach/detach event subscriptions
        private EventEncounterController eventController;
        private TreasureEncounterController treasureController;

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
            if (treasureController != null)
            {
                treasureController.OnTreasureResolved -= ShowResults;
            }
        }

        private void SubscribeToEvents()
        {
            eventController = FindAnyObjectByType<EventEncounterController>();
            if (eventController != null)
            {
                eventController.OnEventResolved += ShowResults;
            }

            treasureController = FindAnyObjectByType<TreasureEncounterController>();
            if (treasureController != null)
            {
                treasureController.OnTreasureResolved += ShowResults;
            }
        }

        public void ShowResults(string results)
        {
            var settings = UISettings.Instance;
            resultsText.text = ColorizeResultLines(results,
                settings != null ? settings.ToHex(UIColorRole.Positive) : "FFFFFF",
                settings != null ? settings.ToHex(UIColorRole.Negative) : "FFFFFF");
            mainPanel.SetActive(true);
        }

        public void HideResults()
        {
            mainPanel.SetActive(false);
        }

        /// <summary>
        /// One result per line ("Gained 10 health", "Lost 5 Grace", "Found item: X"): lines starting with Gained
        /// take the gained color, lines starting with Lost the lost color, others stay as they are.
        /// </summary>
        public static string ColorizeResultLines(string results, string gainedHex, string lostHex)
        {
            if (string.IsNullOrEmpty(results)) return string.Empty;

            var lines = results.TrimEnd('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');
                if (line.StartsWith(GainedPrefix, StringComparison.Ordinal)) line = $"<color=#{gainedHex}>{line}</color>";
                else if (line.StartsWith(LostPrefix, StringComparison.Ordinal)) line = $"<color=#{lostHex}>{line}</color>";
                lines[i] = line;
            }
            return string.Join("\n", lines);
        }

        private void OnOkButtonClicked()
        {
            HideResults();
            WaveSpawner.Instance.InvokeOnWaveComplete();
        }
    }
}
