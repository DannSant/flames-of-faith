using Game.Currency;
using Game.Effects;
using Game.Items;
using Game.Progression;
using Game.Scene;
using Game.Utils;
using Game.Waves;
using System;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>
    /// A treasure chest: walking into it rolls one reward from the table. An item goes to the player's item bag
    /// and the encounter ends, so the item bag window opens (take or sell), like items from events.
    /// Currency and experience are given at once and reported through OnTreasureResolved (the Results window,
    /// whose Continue ends the encounter).
    /// </summary>
    public class TreasureEncounterController : MonoBehaviour
    {
        private const string CurrencyName = "Golden Chronos";
        private const string ExperienceName = "Experience";

        [SerializeField] private TreasureEncounterData data;

        private bool resolved = false;

        /// <summary>The result line of a currency or experience treasure, e.g. "Gained 75 Golden Chronos".</summary>
        public event Action<string> OnTreasureResolved;

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

            resolved = true;
            if (!RollReward(out var type, out var item, out int amount))
            {
                ExitEncounter();
                return;
            }

            switch (type)
            {
                case TreasureRewardType.Item:
                    var itemBag = PlayerManager.Instance.GetPlayerComponent<ItemBag>();
                    for (int i = 0; i < amount; i++)
                    {
                        itemBag.AddEffectItem(item);
                    }
                    ExitEncounter();
                    break;

                case TreasureRewardType.Currency:
                    PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>().AddCurrency(amount);
                    OnTreasureResolved?.Invoke(DescribeReward(type, amount));
                    break;

                case TreasureRewardType.Experience:
                    PlayerManager.Instance.GetPlayerComponent<PlayerExperience>().AddExperience(amount);
                    OnTreasureResolved?.Invoke(DescribeReward(type, amount));
                    break;
            }
        }

        /// <summary>The Results line of a currency or experience reward.</summary>
        public static string DescribeReward(TreasureRewardType type, int amount)
        {
            switch (type)
            {
                case TreasureRewardType.Currency: return $"Gained {amount} {CurrencyName}";
                case TreasureRewardType.Experience: return $"Gained {amount} {ExperienceName}";
                default: return string.Empty;
            }
        }

        private bool RollReward(out TreasureRewardType type, out Effect item, out int amount)
        {
            type = TreasureRewardType.Item;
            item = null;
            amount = 0;

            if (data.rewardTable == null || data.rewardTable.Count == 0)
            {
                Debug.LogError("[TreasureEncounter] Reward table is empty.");
                return false;
            }

            // Use a local RNG (for demo this is OK)
            System.Random rng = new System.Random();

            // Pick the reward entry by weight
            TreasureRewardEntry entry = WeightedRandom.Roll(
                data.rewardTable,
                e => e.weight,
                rng
            );

            if (entry == null)
            {
                Debug.LogError("[TreasureEncounter] Failed to roll reward entry.");
                return false;
            }

            type = entry.rewardType;
            if (type == TreasureRewardType.Random)
            {
                type = RollRandomRewardType(rng);
            }

            switch (type)
            {
                case TreasureRewardType.Item:
                    item = entry.item;
                    amount = Mathf.Max(1, entry.itemCount);
                    return item != null;

                case TreasureRewardType.Currency:
                    amount = rng.Next(entry.minCurrency, entry.maxCurrency + 1);
                    return true;

                case TreasureRewardType.Experience:
                    amount = rng.Next(entry.minExperience, entry.maxExperience + 1);
                    return true;
            }

            return false;
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

        private void ExitEncounter()
        {
            WaveSpawner.Instance.InvokeOnWaveComplete();
        }
    }

}
