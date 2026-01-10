using Game.RunEncounters;
using Game.Scene;
using TMPro;
using UnityEngine;

namespace Game.UI.RunEncounters
{
    public class TreasureUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private GameObject rejectButton;

        private TreasureEncounterController treasureController;

        private void Start()
        {
            mainPanel.SetActive(false);
            MainSceneController.Instance.OnGameplayUISetupRequested += SubscribeToEvents;
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
            mainPanel.SetActive(true);
            rewardText.text = DescribeReward(reward);

            rejectButton.SetActive(
                treasureController != null &&
                treasureController.Data.allowReject
            );
        }

        private void Hide()
        {
            mainPanel.SetActive(false);
        }

        public void OnAcceptClicked()
        {
            treasureController?.Accept();
        }

        public void OnRejectClicked()
        {
            treasureController?.Reject();
        }

        private string DescribeReward(TreasureReward reward)
        {
            return reward.type switch
            {
                TreasureRewardType.Item =>
                    $"You found {reward.amount}x {reward.item.EffectName}",

                TreasureRewardType.Currency =>
                    $"You found {reward.amount} golden chronos",

                TreasureRewardType.Experience =>
                    $"You gained {reward.amount} experience",

                _ => "You found something mysterious..."
            };
        }
    }

}