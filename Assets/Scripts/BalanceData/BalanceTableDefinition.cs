using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Object = UnityEngine.Object;

namespace Game.BalanceData
{
    /// <summary>
    /// One balance table: its columns, how to export it from the assets, and how to plan an import of it. Rows are
    /// never deleted (saves store items by id and stats by name, so the safe way out is to retire), and an import
    /// with any error changes nothing. Runtime code (reflection, Resources), so a build can apply a table too.
    /// </summary>
    public abstract class BalanceTableDefinition
    {
        public abstract string Name { get; }
        public abstract IReadOnlyList<string> Columns { get; }
        public abstract BalanceTable Export();
        public abstract void Plan(BalanceTable table, ImportPlan plan);

        /// <summary>What the table is for, shown at the side of its sheet tab (see <see cref="BalanceDocs"/>).</summary>
        public virtual string Description => BalanceDocs.TableDescription(Name);

        /// <summary>What each column means, in the order of <see cref="Columns"/>; empty where nothing documents it.</summary>
        public virtual IReadOnlyList<string> ColumnNotes => Columns.Select(c => BalanceDocs.ColumnNote(Name, c, null)).ToList();

        /// <summary>The notes of columns that edit <paramref name="fields"/> of <paramref name="type"/> (tooltips unless documented).</summary>
        protected IEnumerable<string> FieldNotes(Type type, IEnumerable<string> fields) =>
            fields.Select(f =>
            {
                FieldAccess.TryDescribe(type, f, out _, out var info);
                return BalanceDocs.ColumnNote(Name, Header(f), info);
            });

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
                return Nicify(segments[segments.Length - 2]) + " " + last.ToUpperInvariant();
            }
            return Nicify(last);
        }

        /// <summary>
        /// A field name as the Inspector shows it (same as ObjectNames.NicifyVariableName, which is Editor-only):
        /// "m_" / "_" / "k" prefixes dropped, the first letter capitalised, a space before each word.
        /// </summary>
        public static string Nicify(string name)
        {
            if (name.StartsWith("m_")) name = name.Substring(2);
            else if (name.StartsWith("_")) name = name.Substring(1);
            if (name.Length > 1 && name[0] == 'k' && char.IsUpper(name[1])) name = name.Substring(1);
            if (name.Length == 0) return name;

            var text = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i == 0)
                {
                    text.Append(char.ToUpperInvariant(c));
                    continue;
                }
                char previous = name[i - 1];
                bool nextIsLower = i + 1 < name.Length && char.IsLower(name[i + 1]);
                bool startsWord =
                    (char.IsUpper(c) && (char.IsLower(previous) || char.IsDigit(previous) || (char.IsUpper(previous) && nextIsLower)))
                    || (char.IsDigit(c) && char.IsLetter(previous));
                if (startsWord) text.Append(' ');
                text.Append(c);
            }
            return text.ToString();
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

        /// <summary>The cell text of a field of an asset.</summary>
        protected static string FormatField(object root, string path)
        {
            if (!FieldAccess.TryDescribe(root.GetType(), path, out var type, out _))
            {
                throw new ArgumentException($"{root.GetType().Name} has no field {path}");
            }
            return CellCodec.Format(FieldAccess.Get(root, path), type);
        }

        /// <summary>
        /// Plans one cell: parses it against the field and, if it differs from the current value, adds the change.
        /// <paramref name="current"/> holds the current value (a scratch copy for rows that don't exist yet);
        /// <paramref name="target"/> resolves the asset to change, by default <paramref name="asset"/>.
        /// </summary>
        protected void PlanCell(ImportPlan plan, int line, string label, Object asset, object current,
            string path, string text, Func<Object> target = null, string column = null)
        {
            column ??= Header(path);
            if (!FieldAccess.TryDescribe(current.GetType(), path, out var type, out var field) || !CellCodec.IsSupported(type))
            {
                plan.Error(Name, line, $"{label} has no field {path} a table can set");
                return;
            }
            if (!CellCodec.TryParse(text, type, field, out object value, out string error))
            {
                plan.Error(Name, line, $"{label}, {column}: {error}");
                return;
            }
            object currentValue = FieldAccess.Get(current, path);
            if (CellCodec.Matches(currentValue, type, text)) return;

            plan.SetField($"{label}.{column}: {CellCodec.Format(currentValue, type)} -> {text.Trim()}", target ?? (() => asset), path, value);
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

        public override IReadOnlyList<string> ColumnNotes =>
            new[] { BalanceDocs.ColumnNote(Name, keyColumn, null) }
                .Concat(FieldNotes(assets().FirstOrDefault()?.GetType() ?? typeof(T), fields)).ToList();

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            foreach (var asset in assets().OrderBy(key, StringComparer.Ordinal))
            {
                table.AddRow(new[] { key(asset) }.Concat(fields.Select(f => FormatField(asset, f))).ToArray());
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
                for (int f = 0; f < fields.Length; f++)
                {
                    PlanCell(plan, line, rowKey, asset, asset, fields[f], row[f + 1]);
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
            var target = asset();
            foreach (string field in fields)
            {
                FieldAccess.TryDescribe(target.GetType(), field, out _, out var info);
                table.AddRow(Header(field), FormatField(target, field), CellCodec.TooltipOf(info));
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;
            var target = asset();
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
                PlanCell(plan, line, target.name, target, target, field, row[1]);
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
                int count = FieldAccess.Count(parent, listPath);
                for (int i = 0; i < count; i++)
                {
                    table.AddRow(new[] { key(parent) }.Concat(items.Select(item => FormatField(parent, ElementPath(i, item.path)))).ToArray());
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
            int count = FieldAccess.Count(current, listPath);

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

            // Rows past the list's end are checked against a scratch copy grown to the new size. New elements copy the
            // last one (FieldAccess.Resize), so the resize below does the same to the asset.
            Object scratch = null;
            object values = current;
            if (rows.Count != count)
            {
                plan.ResizeList($"{parentKey}.{Header(listPath)}: {count} rows -> {rows.Count} rows", target, listPath, rows.Count);
                scratch = Object.Instantiate(current);
                FieldAccess.Resize(scratch, listPath, rows.Count);
                values = scratch;
            }

            try
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var (line, cells) = rows[i];
                    for (int c = 0; c < items.Length; c++)
                    {
                        PlanCell(plan, line, $"{parentKey}[{i + 1}]", current, values, ElementPath(i, items[c].path), cells[c + 1], target, items[c].header);
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
