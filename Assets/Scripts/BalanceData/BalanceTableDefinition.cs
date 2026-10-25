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
            FieldNotes(type, fields.Select(f => (Header(f), f)));

        /// <summary>The notes of columns (header, field path) of <paramref name="type"/> (tooltips unless documented).</summary>
        protected IEnumerable<string> FieldNotes(Type type, IEnumerable<(string header, string path)> columns) =>
            columns.Select(column =>
            {
                FieldAccess.TryDescribe(type, column.path, out _, out var info);
                // A vector component ("spawnOffset.x") ends at Unity's own x field: the tooltip is on the vector
                if (info != null && (info.DeclaringType == typeof(UnityEngine.Vector2) || info.DeclaringType == typeof(UnityEngine.Vector3)))
                {
                    FieldAccess.TryDescribe(type, column.path.Substring(0, column.path.LastIndexOf('.')), out _, out info);
                }
                return BalanceDocs.ColumnNote(Name, column.header, info);
            });

        /// <summary>Field paths as columns named after the field (see <see cref="Header"/>).</summary>
        protected static (string header, string path)[] Named(string[] fields) => fields.Select(f => (Header(f), f)).ToArray();

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
        private readonly (string header, string path)[] fields;

        public AssetTable(string name, string keyColumn, Func<IEnumerable<T>> assets, Func<T, string> key, params string[] fields)
            : this(name, keyColumn, assets, key, Named(fields))
        {
        }

        /// <param name="fields">Column header and field path, for nested fields whose own names would clash or read badly.</param>
        public AssetTable(string name, string keyColumn, Func<IEnumerable<T>> assets, Func<T, string> key, params (string header, string path)[] fields)
        {
            this.name = name;
            this.keyColumn = keyColumn;
            this.assets = assets;
            this.key = key;
            this.fields = fields;
        }

        public override string Name => name;
        public override IReadOnlyList<string> Columns => new[] { keyColumn }.Concat(fields.Select(f => f.header)).ToList();

        public override IReadOnlyList<string> ColumnNotes =>
            new[] { BalanceDocs.ColumnNote(Name, keyColumn, null) }
                .Concat(FieldNotes(assets().FirstOrDefault()?.GetType() ?? typeof(T), fields)).ToList();

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            foreach (var asset in assets().OrderBy(key, StringComparer.Ordinal))
            {
                table.AddRow(new[] { key(asset) }.Concat(fields.Select(f => FormatField(asset, f.path))).ToArray());
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
                    PlanCell(plan, line, rowKey, asset, asset, fields[f].path, row[f + 1], column: fields[f].header);
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
        private readonly (string header, string path)[] fields;

        public SettingsTable(string name, Func<T> asset, params string[] fields) : this(name, asset, Named(fields))
        {
        }

        /// <param name="fields">Setting name and field path, for nested or array fields whose own names would read badly.</param>
        public SettingsTable(string name, Func<T> asset, params (string header, string path)[] fields)
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
            foreach (var (header, path) in fields)
            {
                FieldAccess.TryDescribe(target.GetType(), path, out _, out var info);
                table.AddRow(header, FormatField(target, path), BalanceDocs.ColumnNote(Name, header, info));
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;
            var target = asset();
            var byHeader = fields.ToDictionary(f => f.header, f => f.path, StringComparer.OrdinalIgnoreCase);

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
                PlanCell(plan, line, target.name, target, target, field, row[1], column: setting);
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
    /// things only Unity sets, like sprites) must keep its count. A list that may be empty (<see cref="AllowingEmpty"/>,
    /// an item's stat modifiers) has no rows when empty, and an asset without rows gets an empty list. Info columns
    /// (<see cref="WithInfoColumns"/>) come right after the key and show read-only context (an ability's tier).
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
        private (string header, Func<object, int, string> value)[] info = Array.Empty<(string, Func<object, int, string>)>();
        private bool allowEmpty;

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

        /// <summary>Read-only columns after the key, each computed from the list element and its index.</summary>
        public ChildListTable<T> WithInfoColumns(params (string header, Func<object, int, string> value)[] columns)
        {
            info = columns;
            return this;
        }

        /// <summary>The list may be empty: an asset with no rows gets an empty list instead of an error.</summary>
        public ChildListTable<T> AllowingEmpty()
        {
            allowEmpty = true;
            return this;
        }

        public override string Name => name;
        public override IReadOnlyList<string> Columns =>
            new[] { keyColumn }.Concat(info.Select(i => i.header)).Concat(items.Select(i => i.header)).ToList();

        // Documented columns first, else the element field's tooltip
        public override IReadOnlyList<string> ColumnNotes =>
            new[] { BalanceDocs.ColumnNote(Name, keyColumn, null) }
                .Concat(info.Select(i => BalanceDocs.ColumnNote(Name, i.header, null)))
                .Concat(FieldNotes(ParentType(), items.Select(i => (i.header, ElementPath(0, i.path))))).ToList();

        private Type ParentType() => parents().FirstOrDefault()?.GetType() ?? typeof(T);

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
                    var element = FieldAccess.Get(parent, $"{listPath}.Array.data[{i}]");
                    int index = i;
                    table.AddRow(new[] { key(parent) }
                        .Concat(info.Select(column => column.value(element, index) ?? string.Empty))
                        .Concat(items.Select(item => FormatField(parent, ElementPath(index, item.path)))).ToArray());
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
                // PlanList reads the key then the item cells: the info columns are left out
                PlanList(plan, parentKey, parent, rows.Select(i => (BalanceTable.LineOf(i), WithoutInfo(table.Rows[i]))).ToList(), () => parent);
            }
        }

        private string[] WithoutInfo(string[] cells) =>
            info.Length == 0 ? cells : new[] { cells[0] }.Concat(cells.Skip(1 + info.Length)).ToArray();

        /// <summary>Plans one parent's list from its rows (line, cells); also used for parents created by the import.</summary>
        public void PlanList(ImportPlan plan, string parentKey, Object current, List<(int line, string[] cells)> rows, Func<Object> target)
        {
            int count = FieldAccess.Count(current, listPath);

            if (rows.Count == 0 && count > 0 && !allowEmpty)
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

    /// <summary>
    /// The elements of a list field, one row each, keyed by one of their own fields (e.g. the Stat Database's entries
    /// by stat). Rows can't be added or removed, and the key can't change: it says which element a row is.
    /// <c>include</c> leaves elements out of the table (retired stats). Info columns (<see cref="WithInfoColumns"/>)
    /// come right after the key and show read-only context (an item's quality): exported, ignored on import.
    /// </summary>
    public class KeyedListTable<T> : BalanceTableDefinition where T : Object
    {
        private readonly string name;
        private readonly string keyColumn;
        private readonly Func<T> asset;
        private readonly string listPath;
        private readonly string keyPath;
        private readonly Func<object, bool> include;
        private readonly (string header, string path)[] items;
        private (string header, Func<object, string> value)[] info = Array.Empty<(string, Func<object, string>)>();

        public KeyedListTable(string name, string keyColumn, Func<T> asset, string listPath, string keyPath,
            Func<object, bool> include, params (string header, string path)[] items)
        {
            this.name = name;
            this.keyColumn = keyColumn;
            this.asset = asset;
            this.listPath = listPath;
            this.keyPath = keyPath;
            this.include = include ?? (_ => true);
            this.items = items;
        }

        /// <summary>Read-only columns after the key, each computed from the list element.</summary>
        public KeyedListTable<T> WithInfoColumns(params (string header, Func<object, string> value)[] columns)
        {
            info = columns;
            return this;
        }

        public override string Name => name;
        public override IReadOnlyList<string> Columns =>
            new[] { keyColumn }.Concat(info.Select(i => i.header)).Concat(items.Select(i => i.header)).ToList();

        public override IReadOnlyList<string> ColumnNotes =>
            new[] { BalanceDocs.ColumnNote(Name, keyColumn, null) }
                .Concat(info.Select(i => BalanceDocs.ColumnNote(Name, i.header, null)))
                .Concat(FieldNotes(typeof(T), items.Select(i => (i.header, ElementPath(0, i.path))))).ToList();

        private object Element(T target, int index) => FieldAccess.Get(target, $"{listPath}.Array.data[{index}]");

        private string ElementPath(int index, string item) => $"{listPath}.Array.data[{index}].{item}";

        /// <summary>The included elements, by key, in list order.</summary>
        private List<(string key, int index)> Elements(T target)
        {
            var result = new List<(string, int)>();
            int count = FieldAccess.Count(target, listPath);
            for (int i = 0; i < count; i++)
            {
                if (!include(Element(target, i))) continue;
                result.Add((FormatField(target, ElementPath(i, keyPath)), i));
            }
            return result;
        }

        public override BalanceTable Export()
        {
            var table = new BalanceTable(Name, Columns);
            var target = asset();
            foreach (var (key, index) in Elements(target))
            {
                var element = Element(target, index);
                table.AddRow(new[] { key }
                    .Concat(info.Select(column => column.value(element) ?? string.Empty))
                    .Concat(items.Select(item => FormatField(target, ElementPath(index, item.path)))).ToArray());
            }
            return table;
        }

        public override void Plan(BalanceTable table, ImportPlan plan)
        {
            if (!CheckColumns(table, plan)) return;
            var target = asset();

            var byKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, index) in Elements(target))
            {
                if (!byKey.TryAdd(key, index)) plan.Error(Name, $"two entries have the key {key}");
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                int line = BalanceTable.LineOf(i);
                string rowKey = row[0].Trim();
                if (!byKey.TryGetValue(rowKey, out int index))
                {
                    plan.Error(Name, line, $"no {keyColumn} \"{rowKey}\" (rows can't be added to this table; add it in Unity)");
                    continue;
                }
                if (!seen.Add(rowKey))
                {
                    plan.Error(Name, line, $"{rowKey} has two rows");
                    continue;
                }
                for (int c = 0; c < items.Length; c++)
                {
                    PlanCell(plan, line, rowKey, target, target, ElementPath(index, items[c].path), row[1 + info.Length + c], column: items[c].header);
                }
            }

            foreach (string missing in byKey.Keys.Where(k => !seen.Contains(k)))
            {
                plan.Error(Name, $"the row for {missing} is missing (rows can't be removed)");
            }
        }
    }
}
