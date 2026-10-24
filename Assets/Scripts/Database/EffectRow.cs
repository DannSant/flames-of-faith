using SQLite4Unity3d;
using UnityEngine;

namespace Game.Database
{
    public class EffectRow 
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        public string effectID { get; set; }       // GUID
        public string name { get; set; }
        public string description { get; set; }
        public float scalingValue { get; set; }
        public int priceBuy { get; set; }
        public int priceSell { get; set; }
        public string iconKey { get; set; }

        public int unlockedByDefault { get; set; }  // 0 = false, 1 = true

        public int quality { get; set; }  // EffectQuality: 0=Common, 1=Uncommon, 2=Rare, 3=Heroic, 4=Epic, 5=Legendary

        // Retired (Oct 2026): whether an item is sold is the Shop column of the drop table (Resources/Loot/
        // ItemDropTable, the balance sheet's Items tab). The SQLite column stays, unread, because SQLite can't drop
        // columns safely; nothing writes it any more.
        public int? availableForShop { get; set; }

        // Upgrade tier of a class kit item (0 = base, 1 = "+1", ...). Picks the row of the item's
        // ability tier tables. null = column didn't exist yet = tier 0.
        public int? tier { get; set; }

        // 0 = false, 1 = true. Starting items (class kits) are never sold, dropped or rewarded.
        // null = column didn't exist yet = false.
        public int? startingOnly { get; set; }

        // JSON with stat modifiers
        public string statModifiersJson { get; set; }

        // JSON with behaviors list (list of SO GUIDs)
        public string behaviorsJson { get; set; }

        // JSON list of StatDependencyRule (stat bonuses that grow with another stat).
        // null/empty = no rules, which is also what rows read back before this column existed.
        public string statDependenciesJson { get; set; }
    }

}