using Game.RunEncounters;
using Game.Scene;
using Game.UI.Style;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    /// <summary>
    /// The campfire window: what resting gave (Grace and health, in their colors) and Continue. Opens the Stats
    /// pane and the Inventory beside it. Layout: RunEncounterRestyler.
    /// </summary>
    public class CampfireUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI infoTxt;
        [SerializeField] private Button continueButton;
        [Tooltip("{0} = the Grace gained, {1} = the health gained (both already colored, e.g. \"5 Grace\").")]
        [TextArea]
        [SerializeField] private string restedFormat = "You feel rested. You gained {0} and {1}.";

        [Header("Run review")]
        [Tooltip("Optional. Found in the loaded scenes when empty.")]
        [SerializeField] private StatsPaneUI statsPane;
        [Tooltip("Optional. Found in the loaded scenes when empty.")]
        [SerializeField] private InventoryControllerUI inventoryPane;

        private CampfireGraceGenerator campfireGraceGenerator;
        private StatsAndInventoryPanes panes;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonclicked);
        }

        private void OnDestroy()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueButtonclicked);
        }

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

                var settings = UISettings.Instance;
                infoTxt.text = FormatRestedText(restedFormat, campfireGraceGenerator.GraceAmount, campfireGraceGenerator.HealAmount,
                    settings != null ? settings.ToHex(UIColorRole.Grace) : "FFFFFF",
                    settings != null ? settings.ToHex(UIColorRole.Positive) : "FFFFFF");

                // After the campfire window, so it keeps gamepad focus
                Panes.Show();
            }
        }

        /// <summary>The rested line: "You feel rested. You gained [5 Grace] and [10 health]." with each gain colored.</summary>
        public static string FormatRestedText(string format, int grace, int heal, string graceHex, string healHex)
        {
            return string.Format(format, $"<color=#{graceHex}>{grace} Grace</color>", $"<color=#{healHex}>{heal} health</color>");
        }

        public void OnContinueButtonclicked()
        {
            Panes.Hide();
            WaveSpawner.Instance.GoToNextLevel();
        }

        private StatsAndInventoryPanes Panes => panes ??= new StatsAndInventoryPanes(statsPane, inventoryPane);
    }

}
