using UnityEngine;
using SQLite4Unity3d;

public class SQLiteTest : MonoBehaviour
{
    void Start()
    {
        string dbPath = System.IO.Path.Combine(Application.persistentDataPath, "test.db");

        var db = new SQLiteConnection(dbPath);

        db.CreateTable<TestEffectRow>();
        db.Insert(new TestEffectRow { Name = "Sword of Fire", Power = 99 });

        var row = db.Table<TestEffectRow>().FirstOrDefault();

        Debug.Log($"DB WORKS → Id={row.Id}, Name={row.Name}, Power={row.Power}");

        db.Close();
    }
}

public class TestEffectRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public int Power { get; set; }
}