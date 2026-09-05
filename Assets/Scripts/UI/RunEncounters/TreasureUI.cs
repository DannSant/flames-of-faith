using Game.Effects;
using Game.RunEncounters;
using Game.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.RunEncounters
{
    public class TreasureUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private GameObject rejectButton;
        [SerializeField] private Image rewardImage;

        private TreasureEncounterController treasureController;
        private GeneralTooltipPaneUI generalTooltipPaneUI;
        private TreasureReward currentReward;

        private void Start()
        {
            mainPanel.SetActive(false);
            MainSceneController.Instance.OnGameplayUISetupRequested += SubscribeToEvents;

            generalTooltipPaneUI = FindAnyObjectByType<GeneralTooltipPaneUI>();

            var rewardIconTrigger = rewardImage.gameObject.AddComponent<TreasureRewardIconUI>();
            rewardIconTrigger.Setup(ShowRewardTooltip, HideRewardTooltip);
        }

        private void OnDisable()
        {
            MainSceneController.Instance.OnGameplayUISetupRequested -= SubscribeToEvents;

            if (treasureController != null)
            {
                treasureController.OnTreasurePresented -= ShowTreasure;
                treasureController.OnTreasureResolved -= Hide;
            }
        }

        private void SubscribeToEvents()
        {
            treasureController = FindAnyObjectByType<TreasureEncounterController>();

            if (treasureController == null)
                return;

            treasureController.OnTreasurePresented += ShowTreasure;
            treasureController.OnTreasureResolved += Hide;
        }

        private void ShowTreasure(TreasureReward reward)
        {
            currentReward = reward;
            mainPanel.SetActive(true);
            rewardText.text = DescribeReward(reward);
            rewardImage.sprite = reward.rewardSprite;
            rejectButton.SetActive(
                treasureController != null &&
                treasureController.Data.allowReject
            );
        }

        private void Hide()
        {
            mainPanel.SetActive(false);
            HideRewardTooltip();
        }

        private void ShowRewardTooltip()
        {
            if (currentReward == null || currentReward.type != TreasureRewardType.Item || currentReward.item == null)
                return;

            generalTooltipPaneUI?.ShowTooltip(currentReward.item.GetFormattedDescription(), ColorizeItemName(currentReward.item));
        }

        private void HideRewardTooltip()
        {
            generalTooltipPaneUI?.HideTooltip();
        }

        public void OnAcceptClicked()
        {
            treasureController?.Accept();
            Hide();
        }

        public void OnRejectClicked()
        {
            treasureController?.Reject();
            Hide();
        }

        private string DescribeReward(TreasureReward reward)
        {
            return reward.type switch
            {
                TreasureRewardType.Item =>
                    $"You found {reward.amount}x {ColorizeItemName(reward.item)}",

                TreasureRewardType.Currency =>
                    $"You found {reward.amount} golden chronos",

                TreasureRewardType.Experience =>
                    $"You gained {reward.amount} experience",

                _ => "You found something mysterious..."
            };
        }

        private string ColorizeItemName(Effect item)
        {
            string hex = ColorUtility.ToHtmlStringRGB(EffectQualityDisplayHelper.GetQualityColor(item.Quality));
            return $"<color=#{hex}>{item.EffectName}</color>";
        }
    }

}