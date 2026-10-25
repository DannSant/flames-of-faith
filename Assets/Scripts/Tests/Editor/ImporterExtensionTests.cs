using Game.BalanceData;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// What items and abilities need from the balance importer (data-driven step 5): nested records in
    /// DesignFields (damage profiles), ChildListTable info columns (an ability's tier) and empty lists (an item
    /// without stat modifiers), notes from element tooltips, and multiline text (item descriptions).
    /// </summary>
    public class ImporterExtensionTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private ImporterFixture Make(string name, params int[] amounts)
        {
            var asset = ScriptableObject.CreateInstance<ImporterFixture>();
            asset.name = name;
            asset.rows = amounts.Select(a => new ImporterFixture.Row { amount = a }).ToList();
            created.Add(asset);
            return asset;
        }

        private static ImportPlan PlanOf(BalanceTableDefinition definition, BalanceTable table)
        {
            var plan = new ImportPlan();
            definition.Plan(table, plan);
            return plan;
        }

        private static BalanceTable RoundTrip(BalanceTable table) => Csv.Read(table.Name, Csv.Write(table));

        [Test]
        public void DesignFields_ReadsNestedRecordsOnlyWhenAsked()
        {
            CollectionAssert.AreEqual(new[] { "top" }, BalanceTables.DesignFields(typeof(ImporterFixture)));
            CollectionAssert.AreEqual(new[] { "top", "profile.baseValue", "profile.scale" },
                BalanceTables.DesignFields(typeof(ImporterFixture), nested: true));
            Assert.AreEqual("Base Value", BalanceTableDefinition.Header("profile.baseValue"));
        }

        [Test]
        public void NestedRecordFields_RoundTripAndImport()
        {
            var fixtures = new List<ImporterFixture> { Make("A") };
            var paths = BalanceTables.DesignFields(typeof(ImporterFixture), nested: true).ToArray();
            var table = new AssetTable<ImporterFixture>("Fixtures", "Name", () => fixtures, f => f.name, paths);

            var exported = RoundTrip(table.Export());
            Assert.IsEmpty(PlanOf(table, exported).Changes);

            exported.Rows[0][exported.Columns.ToList().IndexOf("Scale")] = "1.5";
            var plan = PlanOf(table, exported);
            Assert.IsEmpty(plan.Errors);
            plan.Apply();
            Assert.AreEqual(1.5f, fixtures[0].profile.scale, 0.0001f, "The struct inside the asset was written back");
        }

        private ChildListTable<ImporterFixture> RowTable(List<ImporterFixture> fixtures, bool allowEmpty)
        {
            var table = new ChildListTable<ImporterFixture>("FixtureRows", "Name", () => fixtures, f => f.name, "rows", true, ("Amount", "amount"))
                .WithInfoColumns(("Index", (element, index) => (index + 1).ToString()));
            return allowEmpty ? table.AllowingEmpty() : table;
        }

        [Test]
        public void ChildListInfoColumns_AreExportedAndIgnoredOnImport()
        {
            var fixtures = new List<ImporterFixture> { Make("A", 5, 7) };
            var table = RowTable(fixtures, false);
            CollectionAssert.AreEqual(new[] { "Name", "Index", "Amount" }, table.Columns);

            var exported = RoundTrip(table.Export());
            CollectionAssert.AreEqual(new[] { "A", "1", "5" }, exported.Rows[0]);
            CollectionAssert.AreEqual(new[] { "A", "2", "7" }, exported.Rows[1]);

            exported.Rows[1][1] = "99";
            Assert.IsEmpty(PlanOf(table, exported).Changes, "Editing an info cell changes nothing");

            exported.Rows[1][2] = "8";
            var plan = PlanOf(table, exported);
            Assert.IsEmpty(plan.Errors);
            plan.Apply();
            Assert.AreEqual(8, fixtures[0].rows[1].amount);
        }

        [Test]
        public void AnEmptyList_IsAllowedOnlyWhenTheTableSaysSo()
        {
            var fixtures = new List<ImporterFixture> { Make("A", 5), Make("B") };
            var strict = RowTable(fixtures, false);
            var noRowsForA = new BalanceTable("FixtureRows", strict.Columns);
            Assert.IsNotEmpty(PlanOf(strict, noRowsForA).Errors, "A strict list needs a row");

            var lenient = RowTable(fixtures, true);
            Assert.IsEmpty(PlanOf(lenient, RoundTrip(lenient.Export())).Changes, "B has no rows and stays empty");

            var plan = PlanOf(lenient, noRowsForA);
            Assert.IsEmpty(plan.Errors);
            plan.Apply();
            Assert.AreEqual(0, fixtures[0].rows.Count, "No rows empties the list");
        }

        [Test]
        public void ChildListNotes_FallBackToTheElementTooltips()
        {
            var fixtures = new List<ImporterFixture> { Make("A", 5) };
            Assert.AreEqual("A row's amount.", RowTable(fixtures, false).ColumnNotes[2]);
        }

        [Test]
        public void MultilineText_RoundTripsAsUnchanged()
        {
            var fixture = Make("A");
            fixture.text = "First line, with a comma\r\nSecond \"quoted\" line\n";
            var fixtures = new List<ImporterFixture> { fixture };
            var table = new AssetTable<ImporterFixture>("Texts", "Name", () => fixtures, f => f.name, "text");

            var exported = RoundTrip(table.Export());
            Assert.IsEmpty(PlanOf(table, exported).Changes, "Line ends and trailing whitespace are not a change");

            exported.Rows[0][1] = "One line\nTwo lines";
            var plan = PlanOf(table, exported);
            plan.Apply();
            Assert.AreEqual("One line\nTwo lines", fixture.text);
        }
    }
}
