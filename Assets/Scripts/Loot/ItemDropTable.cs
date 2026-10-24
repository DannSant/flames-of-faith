using Game.Effects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Loot
{
    /// <summary>Where an item can come from.</summary>
    public enum LootSource
    {
        EnemyPickup,
        Shop,
        Treasure,
        Event,
    }

    /// <summary>One lootable item: where it can drop, from how far into the run, and its odds within its quality.</summary>
    [Serializable]
    public class ItemDropEntry
    {
        public Effect item;
        [Tooltip("Can come from the random item pickup enemies sometimes drop.")]
        public bool enemyPickup;
        [Tooltip("Can be offered in the shop.")]
        public bool shop;
        [Tooltip("Can come from a treasure chest.")]
        public bool treasure;
        [Tooltip("Can come from an event's random item reward.")]
        public bool events;
        [Tooltip("Combat and boss levels the player must have beaten in the run before this item can drop anywhere. 0 = from the start.")]
        [Min(0)]
        public int minLevelsBeaten;
        [Tooltip("How likely this item is compared to the other eligible items of the same quality (2 = twice as likely). 0 = never.")]
        [Min(0f)]
        public float weight = 1f;

        public bool DropsFrom(LootSource source) => source switch
        {
            LootSource.EnemyPickup => enemyPickup,
            LootSource.Shop => shop,
            LootSource.Treasure => treasure,
            LootSource.Event => events,
            _ => false,
        };
    }

    /// <summary>
    /// Every lootable item and its drop rules: the one place the enemy pickup, the shop, treasures and events draw
    /// items from (through <see cref="ItemProvider"/>). What an item is (stats, price, quality) stays in the Effects
    /// database; where and when it drops lives here and in the balance sheet's Items tab. Rows are kept in step with
    /// the lootable Effects by Tools/Flames of Faith/Loot/Sync Item Drop Table.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDropTable", menuName = "Loot/Item Drop Table")]
    public class ItemDropTable : ScriptableObject
    {
        public const string ResourcePath = "Loot/ItemDropTable";

        [SerializeField] private List<ItemDropEntry> entries = new();

        public List<ItemDropEntry> Entries => entries;

        private static ItemDropTable instance;

        public static ItemDropTable Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ItemDropTable>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"ItemDropTable not found at Resources/{ResourcePath}. No item can drop.");
                        instance = CreateInstance<ItemDropTable>();
                    }
                }
                return instance;
            }
        }
    }
}
