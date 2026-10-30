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
    /// The Results window. An event shows its narrated result, then a compact summary line of what changed
    /// (EventResultFormatter); a currency / experience treasure shows one line per reward, gains in green and
    /// losses in red. Continue ends the encounter. Layout: RunEncounterRestyler.
    /// </summary>
    public class RunEncounterResultUI : MonoBehaviour
    {
        private const string GainedPrefix = "Gained";
        private const string LostPrefix = "Lost";

        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI resultsText;
        [Tooltip("An event's summary line, centered above Continue. Hidden for a treasure. Without it the summary " +
            "is appended to the results text.")]
        [SerializeField] private TextMeshProUGUI summaryText;

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
                eventController.OnEventResolved -= ShowEventResults;
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
                eventController.OnEventResolved += ShowEventResults;
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
            if (summaryText != null) summaryText.gameObject.SetActive(false);
            mainPanel.SetActive(true);
        }

        /// <summary>An event's narrated result, then the compact summary line of what changed.</summary>
        public void ShowEventResults(EventResolution resolution)
        {
            var colors = GetSummaryColors(UISettings.Instance);
            if (summaryText == null)
            {
                resultsText.text = BuildEventResultText(resolution, colors);
            }
            else
            {
                string summary = resolution != null ? EventResultFormatter.BuildSummary(resolution.AllResults, colors) : string.Empty;
                resultsText.text = EventResultFormatter.FormatResolution(resolution);
                summaryText.text = summary;
                summaryText.gameObject.SetActive(!string.IsNullOrEmpty(summary));
            }
            mainPanel.SetActive(true);
        }

        public static string BuildEventResultText(EventResolution resolution, EventSummaryColors colors)
        {
            string narration = EventResultFormatter.FormatResolution(resolution);
            string summary = resolution != null ? EventResultFormatter.BuildSummary(resolution.AllResults, colors) : string.Empty;
            if (string.IsNullOrEmpty(narration)) return summary;
            if (string.IsNullOrEmpty(summary)) return narration;
            return $"{narration}\n\n{summary}";
        }

        public static EventSummaryColors GetSummaryColors(UISettings settings)
        {
            if (settings == null) return default;
            return new EventSummaryColors
            {
                positive = settings.ToHex(UIColorRole.Positive),
                negative = settings.ToHex(UIColorRole.Negative),
                grace = settings.ToHex(UIColorRole.Grace),
                favors = settings.ToHex(UIColorRole.Favors)
            };
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
