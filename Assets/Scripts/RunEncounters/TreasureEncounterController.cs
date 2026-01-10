using Game.Effects;
using Game.Overworld;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using NUnit.Framework.Interfaces;
using System;
using UnityEngine;

namespace Game.RunEncounters
{
    public class TreasureReward
    {
        public TreasureRewardType type;

        public Effect item;
        public int amount;
    }
    public class TreasureEncounterController : MonoBehaviour
    {
        [SerializeField] private TreasureEncounterData data;

        private TreasureReward resolvedReward;
        private bool resolved = false;

        public event Action<TreasureReward> OnTreasurePresented;
        public event Action OnTreasureResolved;

        public TreasureEncounterData Data => data;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            TriggerEncounter();
        }

        public void TriggerEncounter()
        {
            if (resolved)
                return;

            resolvedReward = RollReward();
            PresentReward(resolvedReward);
        }

        private TreasureReward RollReward()
        {
            if (data.rewardTable == null || data.rewardTable.Count == 0)
            {
                Debug.LogError("[TreasureEncounter] Reward table is empty.");
                return null;
            }

            // Use a local RNG (for demo this is OK)
            System.Random rng = new System.Random();

            // 1️⃣ Pick reward entry by weight
            TreasureRewardEntry entry = WeightedRandom.Roll(
                data.rewardTable,
                e => e.weight,
                rng
            );

            if (entry == null)
            {
                Debug.LogError("[TreasureEncounter] Failed to roll reward entry.");
                return null;
            }

            // 2️⃣ Resolve reward type
            TreasureRewardType resolvedType = entry.rewardType;

            if (resolvedType == TreasureRewardType.Random)
            {
                resolvedType = RollRandomRewardType(rng);
            }

            // 3️⃣ Resolve final reward
            TreasureReward reward = new TreasureReward
            {
                type = resolvedType
            };

            switch (resolvedType)
            {
                case TreasureRewardType.Item:
                    reward.item = entry.item;
                    reward.amount = Mathf.Max(1, entry.itemCount);
                    break;

                case TreasureRewardType.Currency:
                    reward.amount = rng.Next(
                        entry.minCurrency,
                        entry.maxCurrency + 1
                    );
                    break;

                case TreasureRewardType.Experience:
                    reward.amount = rng.Next(
                        entry.minExperience,
                        entry.maxExperience + 1
                    );
                    break;
            }

            return reward;
        }

        private TreasureRewardType RollRandomRewardType(System.Random rng)
        {
            TreasureRewardType[] types =
            {
                TreasureRewardType.Item,
                TreasureRewardType.Currency,
                TreasureRewardType.Experience
            };

            int index = rng.Next(types.Length);
            return types[index];
        }

        private void PresentReward(TreasureReward reward)
        {
            // UI hook later
            OnTreasurePresented?.Invoke(reward);
        }

        private string DescribeReward(TreasureReward reward)
        {
            return reward.type switch
            {
                TreasureRewardType.Item =>
                    $"{reward.amount}x {reward.item.name}",

                TreasureRewardType.Currency =>
                    $"{reward.amount} Gold",

                TreasureRewardType.Experience =>
                    $"{reward.amount} XP",

                _ => "Unknown Reward"
            };
        }

        public void Accept()
        {
            if (resolved)
                return;

            ApplyReward(resolvedReward);
            resolved = true;
            ExitEncounter();
        }

        public void Reject()
        {
            if (resolved)
                return;

            resolved = true;
            ExitEncounter();
        }

        private void ApplyReward(TreasureReward reward)
        {
            switch (reward.type)
            {
                case TreasureRewardType.Item:
                    //PlayerInventory.Instance.AddItem(reward.item, reward.amount);
                    break;

                case TreasureRewardType.Currency:
                    //PlayerWallet.Instance.Add(reward.amount);
                    break;

                case TreasureRewardType.Experience:
                    //PlayerProgression.Instance.AddExperience(reward.amount);
                    break;
            }
        }

        private void Resolve()
        {
            OnTreasureResolved?.Invoke();
            //MapRunController.Instance.OnLevelCleared();
            //MainSceneController.Instance.LoadLevelSelector();
        }


        private void ExitEncounter()
        {
            
        }
    }

}