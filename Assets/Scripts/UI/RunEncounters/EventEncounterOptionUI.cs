using System.Text;
using Game.Effects;
using Game.Progression;
using Game.RunEncounters;
using Game.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.RunEncounters
{
    public class EventEncounterOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI optionText;

        private EventOptionsList optionsList;
        private GeneralTooltipPaneUI generalTooltipPaneUI;

        public void Initialize(EventOptionsList optionsList, GeneralTooltipPaneUI generalTooltipPaneUI)
        {
            this.optionsList = optionsList;
            this.generalTooltipPaneUI = generalTooltipPaneUI;
            optionText.text = optionsList.OptionDescription;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            string tooltip = BuildTooltip();
            if (string.IsNullOrEmpty(tooltip))
            {
                return;
            }

            generalTooltipPaneUI?.ShowTooltip(tooltip);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        public void HideTooltip()
        {
            generalTooltipPaneUI?.HideTooltip();
        }

        private string BuildTooltip()
        {
            if (optionsList == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var action in optionsList.OptionActionsList)
            {
                if (action == null)
                {
                    continue;
                }

                sb.AppendLine(BuildActionLine(action));
            }

            return sb.ToString();
        }

        private string BuildActionLine(EventOptionActionBase action)
        {
            if(action is GiveRandomItemOptionAction giveRandomItemAction)
            {
                var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
                EffectQuality expectedQuality = giveRandomItemAction.GetExpectedQuality(playerProgression);

                return $"Might receive an item of any quality, averaging around {expectedQuality} (higher Luck favors rarer items)";
            }
            
            if (action.IsPlainReward)
            {
                bool isGain = action.Amount >= 0;
                return $"{(isGain ? "Gain" : "Lose")} {Mathf.Abs(action.Amount)} {action.ResourceName}";
            }

            // Assumes a range that is entirely positive or entirely negative.
            // Ranges that straddle zero (hybrid gain/loss) aren't handled precisely yet.
            bool rangeIsGain = action.AmountRange.y > 0;
            string statName = StatDisplayNameHelper.GetDisplayName(action.BiasStat);

            float magnitudeX = Mathf.Abs(action.AmountRange.x);
            float magnitudeY = Mathf.Abs(action.AmountRange.y);
            float lowMagnitude = Mathf.Min(magnitudeX, magnitudeY);
            float highMagnitude = Mathf.Max(magnitudeX, magnitudeY);

            return $"Might {(rangeIsGain ? "get" : "lose")} {action.ResourceName} {lowMagnitude} to {highMagnitude} based on {statName}";
        }
    }
}
