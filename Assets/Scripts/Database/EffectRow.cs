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

        // JSON with stat modifiers
        public string statModifiersJson { get; set; }

        // JSON with behaviors list (list of SO GUIDs)
        public string behaviorsJson { get; set; }
    }

}