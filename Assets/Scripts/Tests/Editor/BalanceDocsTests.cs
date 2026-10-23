using Game.BalanceData;
using NUnit.Framework;
using System.Linq;

namespace Game.Tests
{
    /// <summary>
    /// The explanation panel at the side of every sheet tab: a new table or column can't be added without saying what
    /// it means (a tooltip on the field, or an entry in BalanceDocs).
    /// </summary>
    public class BalanceDocsTests
    {
        [Test]
        public void EveryTable_HasADescription()
        {
            foreach (var definition in BalanceTables.All())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Description),
                    $"{definition.Name} has no description: add it to BalanceDocs.Tables");
            }
        }

        [Test]
        public void EveryColumn_HasANote()
        {
            foreach (var definition in BalanceTables.All())
            {
                var notes = definition.ColumnNotes;
                Assert.AreEqual(definition.Columns.Count, notes.Count, definition.Name);
                for (int i = 0; i < notes.Count; i++)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(notes[i]),
                        $"{definition.Name}.{definition.Columns[i]} has no note: add a [Tooltip] to the field, or an entry in BalanceDocs.Columns");
                }
            }
        }

        [Test]
        public void NoDescription_IsLeftOverFromARemovedTable()
        {
            var names = BalanceTables.All().Select(t => t.Name).ToList();
            foreach (string described in BalanceDocs.DescribedTables)
            {
                Assert.Contains(described, names, $"BalanceDocs describes {described}, which is no table any more");
            }
        }

        [Test]
        public void NoColumnNote_IsLeftOverFromARemovedColumn()
        {
            var columns = BalanceTables.All()
                .SelectMany(t => t.Columns.Select(c => t.Name + "." + c))
                .ToList();
            foreach (string key in BalanceDocs.DocumentedColumns)
            {
                bool all = key.Contains("_*.");
                bool found = all
                    ? columns.Any(c => c.StartsWith(key.Substring(0, key.IndexOf('*')) ) && c.EndsWith(key.Substring(key.IndexOf('*') + 1)))
                    : columns.Contains(key);
                Assert.IsTrue(found, $"BalanceDocs has a note for {key}, which is no column any more");
            }
        }
    }
}
