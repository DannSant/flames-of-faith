using Game.AI;
using Game.BalanceData;
using Game.BalanceData.Editor;
using Game.Enemies;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The pilot balance tables over the real assets: each exports and imports back as no change, and the CSV files
    /// committed in BalanceData/ match the assets (an asset edited in Unity without exporting, or a CSV edited without
    /// importing, fails here). The reports use the game's formulas.
    /// </summary>
    public class BalanceTablesTests
    {
        private static BalanceTable RoundTrip(BalanceTable table) => Csv.Read(table.Name, Csv.Write(table));

        [Test]
        public void EveryTable_ExportsAndImportsBackAsNoChange()
        {
            var tables = BalanceTables.All();
            Assert.That(tables.Count, Is.GreaterThan(10));
            CollectionAssert.AllItemsAreUnique(tables.Select(t => t.Name), "Two tables would write the same file");

            foreach (var definition in tables)
            {
                var exported = definition.Export();
                CollectionAssert.AreEqual(definition.Columns, exported.Columns, definition.Name);
                Assert.IsNotEmpty(exported.Rows, $"{definition.Name} exported no rows");

                var plan = new ImportPlan();
                definition.Plan(RoundTrip(exported), plan);
                Assert.IsEmpty(plan.Errors, $"{definition.Name}:\n" + string.Join("\n", plan.Errors));
                Assert.IsEmpty(plan.Changes, $"{definition.Name}:\n" + string.Join("\n", plan.Changes));
            }
        }

        [Test]
        public void CommittedCsvFiles_MatchTheAssets()
        {
            Assert.IsTrue(Directory.Exists(BalanceDataTools.Folder),
                "No BalanceData folder: run Tools/Flames of Faith/Balance Data/Export CSV and commit it");
            var plan = BalanceDataTools.PlanImport();
            Assert.IsEmpty(plan.Errors, string.Join("\n", plan.Errors));
            Assert.IsEmpty(plan.Changes, "The CSV files and the assets differ. If the assets are right, run Export CSV; " +
                "if the CSV files are, run Import CSV:\n" + string.Join("\n", plan.Changes));
        }

        [Test]
        public void EnemyRows_AreKeyedByEnemyType()
        {
            var enemies = BalanceTables.All().Single(t => t.Name == "Enemies").Export();
            var types = BalanceTables.LoadAll<EnemyData>(BalanceTables.EnemiesFolder).Select(e => e.enemyType.ToString());
            CollectionAssert.AreEquivalent(types, enemies.Rows.Select(r => r[0]), "Same names as the waves' enemy column");
        }

        [Test]
        public void Reports_UseTheGamesFormulas()
        {
            var reports = BalanceReports.All();
            CollectionAssert.AreEquivalent(new[] { "EnemyDamage", "EnemyHealth", "Experience", "LootOdds" }, reports.Select(r => r.Name));
            foreach (var report in reports) Assert.IsNotEmpty(report.Rows, report.Name);

            var slime = BalanceTables.LoadAll<EnemyData>(BalanceTables.EnemiesFolder).Single(e => e.enemyType == EnemyType.SlimeChaser);
            var health = reports.Single(r => r.Name == "EnemyHealth");
            var firstWave = health.Rows.First(r => r[0] == "SlimeChaser" && r[1] == "0" && r[2] == "1");
            Assert.AreEqual(slime.healthBase.ToString(), firstWave[3], "Level 0, wave 1 is the base health");

            var experience = reports.Single(r => r.Name == "Experience");
            Assert.AreEqual(new[] { "1", "10", "0" }, experience.Rows[0], "Level 1 needs 10 XP");

            // At Luck 0 the shop's odds are its base quality weights over the qualities it has
            var loot = reports.Single(r => r.Name == "LootOdds");
            var shopStart = loot.Rows.First(r => r[0] == "Shop" && r[1] == "0" && r[2] == "0");
            var odds = shopStart.Skip(4).Select(c => float.Parse(c, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            Assert.AreEqual(100f, odds.Sum(), 0.5f, "The quality chances add up to 100%");
            Assert.Greater(int.Parse(shopStart[3]), 0, "The shop has items at the start of a run");
        }
    }
}
