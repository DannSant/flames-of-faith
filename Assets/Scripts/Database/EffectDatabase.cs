using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Database
{
    [System.Obsolete("Retired Oct 2026 (data-driven step 5): items are edited through the balance sheet tables (Items, ItemStats, ItemStatBonuses, ItemBehaviors) and Tools/Flames of Faith/Items/New Item. Kept until a play-test round confirms the sheet path, then deleted with the SQLite library.")]
    public static class EffectDatabase 
    {
        private static SQLiteConnection _db;

        public static void Initialize(string dbPath)
        {
            _db = new SQLiteConnection(dbPath);

            _db.CreateTable<EffectRow>();
        }

        public static List<EffectRow> GetAllEffects()
        {
            return _db.Table<EffectRow>().ToList();
        }

        public static EffectRow GetEffectByID(string effectID)
        {
            return _db.Table<EffectRow>().FirstOrDefault(e => e.effectID == effectID);
        }

        public static void InsertEffect(EffectRow row)
        {
            _db.Insert(row);
        }

        public static void UpdateEffect(EffectRow row)
        {
            _db.Update(row);
        }

        public static void DeleteEffect(int id)
        {
            _db.Delete<EffectRow>(id);
        }
    }
}
