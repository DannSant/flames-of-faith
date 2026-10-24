using Game.Effects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Loot
{
    /// <summary>The quality odds of one loot source: a base weight per quality, tilted by Luck.</summary>
    [Serializable]
    public class LootSourceSettings
    {
        public LootSource source;
        [Tooltip("Base weight of Common items, against the other qualities' weights (40 next to 20 = twice as likely).")]
        [Min(0f)] public float commonWeight = 25f;
        [Tooltip("Base weight of Uncommon items.")]
        [Min(0f)] public float uncommonWeight = 25f;
        [Tooltip("Base weight of Rare items.")]
        [Min(0f)] public float rareWeight = 25f;
        [Tooltip("Base weight of Heroic items.")]
        [Min(0f)] public float heroicWeight = 25f;
        [Tooltip("Base weight of Epic items.")]
        [Min(0f)] public float epicWeight;
        [Tooltip("Base weight of Legendary items.")]
        [Min(0f)] public float legendaryWeight;
        [Tooltip("Luck points per step of the Luck curve: each step multiplies a quality's weight by (1 + steps) per quality " +
                 "above Common, so lower = Luck matters more. 0 = Luck is ignored. Negative Luck mirrors it toward Common.")]
        [Min(0f)] public float luckScalePerStep = 2f;

        public float GetBaseWeight(EffectQuality quality) => quality switch
        {
            EffectQuality.Common => commonWeight,
            EffectQuality.Uncommon => uncommonWeight,
            EffectQuality.Rare => rareWeight,
            EffectQuality.Heroic => heroicWeight,
            EffectQuality.Epic => epicWeight,
            EffectQuality.Legendary => legendaryWeight,
            _ => 0f,
        };
    }

    /// <summary>
    /// Tunables of item drops: the quality odds of each source, the shop's size and the enemy drop chances. The items
    /// themselves and their rules are in <see cref="ItemDropTable"/>. Loaded from Resources/Loot/LootSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "LootSettings", menuName = "Loot/Loot Settings")]
    public class LootSettings : ScriptableObject
    {
        public const string ResourcePath = "Loot/LootSettings";

        [Tooltip("Quality odds per source; one entry per source.")]
        [SerializeField] private List<LootSourceSettings> sources = new();

        [Header("Shop")]
        [Tooltip("Items the shop offers with 0 Luck.")]
        [Min(0)]
        [SerializeField] private int shopItemCount = 10;
        [Tooltip("Items added (or removed, with negative Luck) per Luck point.")]
        [SerializeField] private int shopItemsPerLuck = 1;
        [Tooltip("The fewest items the shop offers, however low the player's Luck.")]
        [Min(0)]
        [SerializeField] private int shopMinItemCount = 3;
        [Tooltip("The most items the shop offers, however high the player's Luck. The shop window fits 20.")]
        [Min(0)]
        [SerializeField] private int shopMaxItemCount = 20;

        [Header("Enemy Drops")]
        [Tooltip("Chance that a killed enemy drops a pickup (an item or a health pickup).")]
        [Range(0f, 1f)]
        [SerializeField] private float enemyDropChance = 0.05f;
        [Tooltip("Share of those pickups that are a random item; the rest are health pickups (0.5 = half).")]
        [Range(0f, 1f)]
        [SerializeField] private float enemyItemShare = 0.5f;

        [Header("Treasure")]
        [Tooltip("Coins a treasure gives instead when no item can drop yet (the lowest amount).")]
        [Min(0)]
        [SerializeField] private int treasureFallbackMinCoins = 40;
        [Tooltip("Coins a treasure gives instead when no item can drop yet (the highest amount).")]
        [Min(0)]
        [SerializeField] private int treasureFallbackMaxCoins = 80;

        public List<LootSourceSettings> Sources => sources;
        public float EnemyDropChance => enemyDropChance;
        public float EnemyItemShare => enemyItemShare;
        public int TreasureFallbackMinCoins => treasureFallbackMinCoins;
        public int TreasureFallbackMaxCoins => Mathf.Max(treasureFallbackMinCoins, treasureFallbackMaxCoins);

        /// <summary>The source's quality odds; an empty entry (all 0) when the source has none, so nothing drops.</summary>
        public LootSourceSettings GetSource(LootSource source)
        {
            foreach (var entry in sources)
            {
                if (entry != null && entry.source == source) return entry;
            }
            Debug.LogWarning($"LootSettings has no entry for {source}: nothing can drop from it.");
            return new LootSourceSettings { source = source, commonWeight = 0f, uncommonWeight = 0f, rareWeight = 0f, heroicWeight = 0f };
        }

        /// <summary>How many items the shop offers at this Luck.</summary>
        public int GetShopItemCount(int luck) =>
            LootRoller.GetShopItemCount(shopItemCount, luck, shopItemsPerLuck, shopMinItemCount, shopMaxItemCount);

        private static LootSettings instance;

        public static LootSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<LootSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"LootSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<LootSettings>();
                    }
                }
                return instance;
            }
        }
    }
}
