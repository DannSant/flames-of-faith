using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// One balance table: its columns, how to export it from the assets, and how to plan an import of it. Rows are
    /// never deleted (saves store items by id and stats by name, so the safe way out is to retire), and an import
    /// with any error changes nothing.
    /// </summary>
    public abstract class BalanceTableDefinition
    {
        public abstract string Name { get; }
        public abstract IReadOnlyList<string> Columns { get; }
        public abstract BalanceTable Export();
        public abstract void Plan(BalanceTable table, ImportPlan plan);

        /// <summary>
        /// A field path's column header: its name made readable ("healthPerWave" -> "Health Per Wave"; a vector's
        /// component keeps its field name: "strafeFlipInterval.x" -> "Strafe Flip Interval X").
        /// </summary>
        public static string Header(string path)
        {
            var segments = path.Split('.');
            string last = segments[segments.Length - 1];
            if (segments.Length >= 2 && (last == "x" || last == "y" || last == "z" || last == "w"))
            {
                return ObjectNames.NicifyVariableName(segments[segments.Length - 2]) + " " + last.ToUpperInvariant();
            }
            return ObjectNames.NicifyVariableName(last);
        }

        protected bool CheckColumns(BalanceTable table, ImportPlan plan)
        {
            bool ok = table.Columns.Count == Columns.Count
                && table.Columns.Zip(Columns, (a, b) => string.Equals(a.Trim(), b, StringComparison.OrdinalIgnoreCase)).All(x => x);
            if (!ok)
            {
                plan.Error(Name, 1, $"the columns must be: {string.Join(", ", Columns)}");
                return false;
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i].Length != Columns.Count)
                {
                    plan.Error(Name, BalanceTable.LineOf(i), $"{table.Rows[i].Length} cells, the table has {Columns.Count} columns");
                    ok = false;
                }
            }
            return ok;
        }

        /// <summary>
        /// Plans one cell: parses it against the field and, if it differs from the asset's value, adds the change.
        /// <paramref name="serialized"/> reads the current value (a scratch copy for rows that don't exist yet);
        /// <paramref name="target"/> resolves the asset to change, by default <paramref name="asset"/>.
        /// </summary>
        protected void PlanCell(ImportPlan plan, int line, string label, Object asset, SerializedObject serialized,
            string path, string text, Func<Object> target = null)
        {
            var property = serialized.FindProperty(path);
            if (property == null)
            {
                plan.Error(Name, line, $"{label} has no field {path}");
                return;
            }
            if (!CellCodec.TryParse(property, text, out var setter, out string error))
            {
                plan.Error(Name, line, $"{label}, {Header(path)}: {error}");
                return;
            }
            if (CellCodec.Matches(property, text)) return;

            plan.SetField($"{label}.{Header(path)}: {CellCodec.Format(property)} -> {text.Trim()}", target ?? (() => asset), path, setter);
        }
    }

    /// <summary>One row per asset; the first column is the asset's key, the others are its fields.</summary>
    public class AssetTable<T> : BalanceTableDefinition where T : Object
    {
        private readonly string name;
        private readonly string keyColumn;
        private readonly Func<IEnumerable<T>> assets;
        private readonly Func<T, string> key;
        private readonly string[] fields;

        public AssetTable(string name, string keyColumn, Func<IEnumerable<T>> assets, Func<T, string> key, params string[] fields)
        {
            this.name = name;
            this.keyColumn = keyColumn;
            this.assets = assets;
            this.key = key;
            this.fields = fields;
        }

        public override string Name => name;
        public override IReadOnlyList<string> Columns => new[] { keyColumn }.Concat(fields.Select(Header)).ToList();

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            foreach (var asset in assets().OrderBy(key, StringComparer.Ordinal))
            {
                var serialized = new SerializedObject(asset);
                table.AddRow(new[] { key(asset) }.Concat(fields.Select(f => CellCodec.Format(serialized.FindProperty(f)))).ToArray());
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;

            var byKey = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var asset in assets())
            {
                if (!byKey.TryAdd(key(asset), asset)) plan.Error(Name, $"two assets have the key {key(asset)}");
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                int line = BalanceTable.LineOf(i);
                string rowKey = row[0].Trim();
                if (!byKey.TryGetValue(rowKey, out var asset))
                {
                    plan.Error(Name, line, $"no {keyColumn} \"{rowKey}\" (rows can't be added to this table; add the asset in Unity)");
                    continue;
                }
                if (!seen.Add(rowKey))
                {
                    plan.Error(Name, line, $"{rowKey} has two rows");
                    continue;
                }
                var serialized = new SerializedObject(asset);
                for (int f = 0; f < fields.Length; f++)
                {
                    PlanCell(plan, line, rowKey, asset, serialized, fields[f], row[f + 1]);
                }
            }

            foreach (string missing in byKey.Keys.Where(k => !seen.Contains(k)))
            {
                plan.Error(Name, $"the row for {missing} is missing (rows can't be removed)");
            }
        }
    }

    /// <summary>
    /// The settings of one asset as Setting / Value rows. The Notes column is the field's tooltip: written on export
    /// for the designer, ignored on import.
    /// </summary>
    public class SettingsTable<T> : BalanceTableDefinition where T : Object
    {
        private readonly string name;
        private readonly Func<T> asset;
        private readonly string[] fields;

        public SettingsTable(string name, Func<T> asset, params string[] fields)
        {
            this.name = name;
            this.asset = asset;
            this.fields = fields;
        }

        public override string Name => name;
        public override IReadOnlyList<string> Columns => new[] { "Setting", "Value", "Notes" };

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            var serialized = new SerializedObject(asset());
            foreach (string field in fields)
            {
                var property = serialized.FindProperty(field);
                table.AddRow(Header(field), CellCodec.Format(property), CellCodec.TooltipOf(property));
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;
            var target = asset();
            var serialized = new SerializedObject(target);
            var byHeader = fields.ToDictionary(Header, f => f, StringComparer.OrdinalIgnoreCase);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                int line = BalanceTable.LineOf(i);
                string setting = row[0].Trim();
                if (!byHeader.TryGetValue(setting, out string field))
                {
                    plan.Error(Name, line, $"unknown setting \"{setting}\"");
                    continue;
                }
                if (!seen.Add(setting))
                {
                    plan.Error(Name, line, $"{setting} has two rows");
                    continue;
                }
                PlanCell(plan, line, target.name, target, serialized, field, row[1]);
            }

            foreach (string missing in byHeader.Keys.Where(k => !seen.Contains(k)))
            {
                plan.Error(Name, $"the row for {missing} is missing");
            }
        }
    }

    /// <summary>
    /// The elements of a list field, one row each, under their asset's key (e.g. the enemy damage curve's rows). Row
    /// order is list order. A resizable list grows or shrinks to the number of rows; a fixed one (its elements hold
    /// things only Unity sets, like sprites) must keep its count.
    /// </summary>
    public class ChildListTable<T> : BalanceTableDefinition where T : Object
    {
        private readonly string name;
        private readonly string keyColumn;
        private readonly Func<IEnumerable<T>> parents;
        private readonly Func<T, string> key;
        private readonly string listPath;
        private readonly bool resizable;
        private readonly (string header, string path)[] items;

        /// <param name="items">Column header and path inside one element ("" for a list of plain values).</param>
        public ChildListTable(string name, string keyColumn, Func<IEnumerable<T>> parents, Func<T, string> key,
            string listPath, bool resizable, params (string header, string path)[] items)
        {
            this.name = name;
            this.keyColumn = keyColumn;
            this.parents = parents;
            this.key = key;
            this.listPath = listPath;
            this.resizable = resizable;
            this.items = items;
        }

        public override string Name => name;
        public override IReadOnlyList<string> Columns => new[] { keyColumn }.Concat(items.Select(i => i.header)).ToList();

        private string ElementPath(int index, string item) =>
            $"{listPath}.Array.data[{index}]" + (string.IsNullOrEmpty(item) ? string.Empty : "." + item);

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            foreach (var parent in parents().OrderBy(key, StringComparer.Ordinal))
            {
                var serialized = new SerializedObject(parent);
                int count = serialized.FindProperty(listPath).arraySize;
                for (int i = 0; i < count; i++)
                {
                    table.AddRow(new[] { key(parent) }
                        .Concat(items.Select(item => CellCodec.Format(serialized.FindProperty(ElementPath(i, item.path)))))
                        .ToArray());
                }
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;

            var byKey = parents().ToDictionary(key, p => p, StringComparer.OrdinalIgnoreCase);
            var rowsByParent = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string rowKey = table.Rows[i][0].Trim();
                if (!byKey.ContainsKey(rowKey))
                {
                    plan.Error(Name, BalanceTable.LineOf(i), $"no {keyColumn} \"{rowKey}\"");
                    continue;
                }
                if (!rowsByParent.TryGetValue(rowKey, out var rows)) rowsByParent[rowKey] = rows = new List<int>();
                rows.Add(i);
            }

            foreach (var (parentKey, parent) in byKey)
            {
                var rows = rowsByParent.TryGetValue(parentKey, out var r) ? r : new List<int>();
                PlanList(plan, parentKey, parent, rows.Select(i => (BalanceTable.LineOf(i), table.Rows[i])).ToList(), () => parent);
            }
        }

        /// <summary>Plans one parent's list from its rows (line, cells); also used for parents created by the import.</summary>
        public void PlanList(ImportPlan plan, string parentKey, Object current, List<(int line, string[] cells)> rows, Func<Object> target)
        {
            var serialized = new SerializedObject(current);
            int count = serialized.FindProperty(listPath).arraySize;

            if (rows.Count == 0 && count > 0)
            {
                plan.Error(Name, $"{parentKey} has no rows (it needs at least one)");
                return;
            }
            if (!resizable && rows.Count != count)
            {
                plan.Error(Name, $"{parentKey} must have {count} rows, it has {rows.Count} (this list can't grow or shrink here)");
                return;
            }

            // Rows past the list's end are checked against a scratch copy grown to the new size. Unity fills new
            // elements with a copy of the last one, so the resize below does the same to the asset.
            Object scratch = null;
            var grown = serialized;
            if (rows.Count != count)
            {
                plan.SetField($"{parentKey}.{Header(listPath)}: {count} rows -> {rows.Count} rows", target,
                    $"{listPath}.Array.size", p => p.intValue = rows.Count);
                scratch = Object.Instantiate(current);
                grown = new SerializedObject(scratch);
                grown.FindProperty(listPath).arraySize = rows.Count;
                grown.ApplyModifiedPropertiesWithoutUndo();
            }

            try
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var (line, cells) = rows[i];
                    for (int c = 0; c < items.Length; c++)
                    {
                        PlanCell(plan, line, $"{parentKey}[{i + 1}]", current, grown, ElementPath(i, items[c].path), cells[c + 1], target);
                    }
                }
            }
            finally
            {
                if (scratch != null) Object.DestroyImmediate(scratch);
            }
        }
    }
}
