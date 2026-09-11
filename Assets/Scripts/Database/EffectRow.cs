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

        // 0 = false, 1 = true, null = column didn't exist yet on this row (legacy data,
        // treated as true/available so pre-existing effects don't vanish from the shop).
        // Nullable so SQLite migration adding this column to an existing table doesn't
        // try to write a null into a non-nullable int and throw on load.
        public int? availableForShop { get; set; }

        // JSON with stat modifiers
        public string statModifiersJson { get; set; }

        // JSON with behaviors list (list of SO GUIDs)
        public string behaviorsJson { get; set; }
    }

}