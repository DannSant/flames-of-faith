using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Database
{
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
