using Game.AI;
using Game.BalanceData;
using Game.BalanceData.Editor;
using Game.Enemies;
using Game.Progression;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The balance data importer's core (CSV, cells, the three table kinds), on in-memory assets: an export imports
    /// back as no change, a changed cell is one change, and any bad cell is an error that cancels the whole import.
    /// </summary>
    public class BalanceDataTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private T Make<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            created.Add(asset);
            return asset;
        }

        private List<EnemyData> enemies;

        private AssetTable<EnemyData> EnemyTable() =>
            new("Enemies", "Enemy", () => enemies, e => e.name, "enemyType", "healthBase", "contactDamageTier", "alwaysCorrupted");

        private void MakeEnemies()
        {
            var slime = Make<EnemyData>("Slime");
            slime.enemyType = EnemyType.SlimeChaser;
            slime.healthBase = 50;
            slime.contactDamageTier = 1f;
            var eye = Make<EnemyData>("Eye");
            eye.enemyType = EnemyType.DrekoEye;
            eye.healthBase = 35;
            eye.contactDamageTier = 1.5f;
            enemies = new List<EnemyData> { slime, eye };
        }

        private static ImportPlan PlanOf(BalanceTableDefinition definition, BalanceTable table)
        {
            var plan = new ImportPlan();
            definition.Plan(table, plan);
            return plan;
        }

        private static BalanceTable RoundTrip(BalanceTable table) => Csv.Read(table.Name, Csv.Write(table));

        private static void SetCell(BalanceTable table, string key, string column, string value)
        {
            int c = table.Columns.ToList().IndexOf(column);
            Assert.GreaterOrEqual(c, 0, $"no column {column}");
            table.Rows.Single(r => r[0] == key)[c] = value;
        }

        // --- CSV -------------------------------------------------------------------------------------------

        [Test]
        public void Csv_RoundTripsQuotesCommasAndLineBreaks()
        {
            var table = new BalanceTable("T", new[] { "Name", "Text" });
            table.AddRow("a", "plain");
            table.AddRow("b", "one, two");
            table.AddRow("c", "say \"hi\"");
            table.AddRow("d", "line one\nline two");
            table.AddRow("e", "");

            var back = RoundTrip(table);

            CollectionAssert.AreEqual(table.Columns, back.Columns);
            Assert.AreEqual(table.Rows.Count, back.Rows.Count);
            for (int i = 0; i < table.Rows.Count; i++) CollectionAssert.AreEqual(table.Rows[i], back.Rows[i]);
        }

        [Test]
        public void Csv_SkipsABomAndBlankLines_AndRejectsAnUnclosedQuote()
        {
            var table = Csv.Read("T", "﻿A,B\r\n1,2\r\n\r\n3,4\r\n");
            CollectionAssert.AreEqual(new[] { "A", "B" }, table.Columns);
            Assert.AreEqual(2, table.Rows.Count);

            Assert.Throws<System.FormatException>(() => Csv.Read("T", "A,B\n1,\"2\n"));
        }

        // --- Asset rows ------------------------------------------------------------------------------------

        [Test]
        public void Export_WritesEnumsByNameAndInvariantNumbers()
        {
            MakeEnemies();
            var table = EnemyTable().Export();

            CollectionAssert.AreEqual(new[] { "Enemy", "Enemy Type", "Health Base", "Contact Damage Tier", "Always Corrupted" }, table.Columns);
            CollectionAssert.AreEqual(new[] { "Eye", "DrekoEye", "35", "1.5", "FALSE" }, table.Rows[0], "Sorted by key, enum by name, dot decimals");
        }

        [Test]
        public void ExportThenImport_ChangesNothing()
        {
            MakeEnemies();
            var plan = PlanOf(EnemyTable(), RoundTrip(EnemyTable().Export()));

            Assert.IsEmpty(plan.Errors);
            Assert.IsEmpty(plan.Changes);
        }

        [Test]
        public void AChangedCell_IsOneChange_AndApplyWritesIt()
        {
            MakeEnemies();
            var table = RoundTrip(EnemyTable().Export());
            SetCell(table, "Slime", "Health Base", "60");
            SetCell(table, "Eye", "Enemy Type", "slimeroamer");

            var plan = PlanOf(EnemyTable(), table);
            Assert.IsEmpty(plan.Errors);
            CollectionAssert.AreEquivalent(new[] { "Slime.Health Base: 50 -> 60", "Eye.Enemy Type: DrekoEye -> slimeroamer" }, plan.Changes);

            plan.Apply();
            Assert.AreEqual(60, enemies[0].healthBase);
            Assert.AreEqual(EnemyType.SlimeRoamer, enemies[1].enemyType, "Enum names match without case");
        }

        [Test]
        public void ANearlyEqualFloat_IsNoChange()
        {
            MakeEnemies();
            enemies[1].contactDamageTier = 0.1f;
            var table = RoundTrip(EnemyTable().Export());
            SetCell(table, "Eye", "Contact Damage Tier", "0.100000001");

            Assert.IsEmpty(PlanOf(EnemyTable(), table).Changes);
        }

        [TestCase("Health Base", "fifty", "not a whole number")]
        [TestCase("Contact Damage Tier", "-1", "below the minimum")]
        [TestCase("Contact Damage Tier", "1,5", "not a number")]
        [TestCase("Enemy Type", "Dragon", "not one of")]
        [TestCase("Always Corrupted", "maybe", "not TRUE or FALSE")]
        public void ABadCell_IsAnError_AndNothingIsApplied(string column, string value, string reason)
        {
            MakeEnemies();
            var table = RoundTrip(EnemyTable().Export());
            SetCell(table, "Slime", "Health Base", "60");
            SetCell(table, "Eye", column, value);

            var plan = PlanOf(EnemyTable(), table);
            Assert.AreEqual(1, plan.Errors.Count, string.Join("\n", plan.Errors));
            StringAssert.Contains(reason, plan.Errors[0]);
            StringAssert.Contains("line 2", plan.Errors[0], "Eye sorts first, so it's on line 2");
            Assert.Throws<System.InvalidOperationException>(plan.Apply);
            Assert.AreEqual(50, enemies[0].healthBase, "The valid change isn't applied either");
        }

        [Test]
        public void UnknownMissingAndDuplicateRows_AreErrors()
        {
            MakeEnemies();
            var table = RoundTrip(EnemyTable().Export());
            table.Rows.Add(new[] { "Dragon", "Boss", "1", "1", "FALSE" });
            Assert.That(PlanOf(EnemyTable(), table).Errors, Has.Some.Contains("no Enemy \"Dragon\""));

            table = RoundTrip(EnemyTable().Export());
            table.Rows.RemoveAt(0);
            Assert.That(PlanOf(EnemyTable(), table).Errors, Has.Some.Contains("row for Eye is missing"));

            table = RoundTrip(EnemyTable().Export());
            table.Rows.Add(table.Rows[0].ToArray());
            Assert.That(PlanOf(EnemyTable(), table).Errors, Has.Some.Contains("Eye has two rows"));
        }

        [Test]
        public void WrongColumns_AreAnError()
        {
            MakeEnemies();
            var exported = EnemyTable().Export();
            var table = new BalanceTable("Enemies", exported.Columns.Take(3));
            foreach (var row in exported.Rows) table.Rows.Add(row.Take(3).ToArray());

            Assert.That(PlanOf(EnemyTable(), table).Errors, Has.Some.Contains("the columns must be"));
        }

        // --- Settings --------------------------------------------------------------------------------------

        [Test]
        public void Settings_ExportNotesFromTooltips_AndImportByName()
        {
            var settings = Make<ExperienceSettings>("Experience");
            var definition = new SettingsTable<ExperienceSettings>("Experience", () => settings, "xpRequiredBase", "dropChance");

            var table = RoundTrip(definition.Export());
            CollectionAssert.AreEqual(new[] { "Setting", "Value", "Notes" }, table.Columns);
            Assert.AreEqual("Xp Required Base", table.Rows[0][0]);
            StringAssert.Contains("level 1 to level 2", table.Rows[0][2], "Notes come from the field's tooltip");

            table.Rows[1][1] = "0.5";
            table.Rows[1][2] = "the designer's own note";
            var plan = PlanOf(definition, table);
            Assert.IsEmpty(plan.Errors);
            Assert.AreEqual(1, plan.Changes.Count, "Notes are ignored");
            plan.Apply();
            Assert.AreEqual(0.5f, settings.DropChance, 1e-6f);

            table.Rows[1][1] = "1.5";
            Assert.That(PlanOf(definition, table).Errors, Has.Some.Contains("outside 0..1"), "[Range] is checked");
        }

        // --- Child lists -----------------------------------------------------------------------------------

        private ChildListTable<EnemyDamageSettings> TierTable(EnemyDamageSettings settings, bool resizable) =>
            new("Tiers", "Settings", () => new[] { settings }, s => s.name, "tierHitsToKill", resizable, ("Hits To Kill", ""));

        [Test]
        public void ResizableList_FollowsTheRows()
        {
            var settings = Make<EnemyDamageSettings>("Damage");
            var definition = TierTable(settings, resizable: true);
            var table = RoundTrip(definition.Export());
            int before = table.Rows.Count;
            Assert.Greater(before, 1);

            table.Rows.Add(new[] { "Damage", "1" });
            var plan = PlanOf(definition, table);
            Assert.IsEmpty(plan.Errors);
            plan.Apply();
            var after = RoundTrip(definition.Export());
            Assert.AreEqual(before + 1, after.Rows.Count);
            Assert.AreEqual("1", after.Rows.Last()[1]);

            after.Rows.RemoveAt(0);
            plan = PlanOf(definition, after);
            plan.Apply();
            Assert.AreEqual(before, definition.Export().Rows.Count, "Removing a row shrinks the list");
        }

        [Test]
        public void FixedList_KeepsItsCount()
        {
            var settings = Make<EnemyDamageSettings>("Damage");
            var definition = TierTable(settings, resizable: false);
            var table = RoundTrip(definition.Export());
            table.Rows.Add(new[] { "Damage", "1" });

            Assert.That(PlanOf(definition, table).Errors, Has.Some.Contains("can't grow or shrink"));
        }
    }
}
