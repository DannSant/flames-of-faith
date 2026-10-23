using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.BalanceData
{
    /// <summary>
    /// One balance table as text: a header and string rows. This is what a CSV file in BalanceData/ (and later a
    /// sheet tab) holds; the table definitions turn it into asset fields and back.
    /// </summary>
    public class BalanceTable
    {
        public string Name { get; }
        public IReadOnlyList<string> Columns { get; }
        public List<string[]> Rows { get; } = new();

        public BalanceTable(string name, IEnumerable<string> columns)
        {
            Name = name;
            Columns = columns.ToList();
        }

        public void AddRow(params string[] cells)
        {
            if (cells.Length != Columns.Count)
            {
                throw new ArgumentException($"{Name}: a row has {cells.Length} cells, the table has {Columns.Count} columns");
            }
            Rows.Add(cells);
        }

        /// <summary>The file line of a row (the header is line 1).</summary>
        public static int LineOf(int rowIndex) => rowIndex + 2;
    }

    /// <summary>
    /// Reads and writes CSV (RFC 4180: comma-separated, quotes around cells with commas, quotes or line breaks,
    /// a quote inside written twice). UTF-8 without a BOM, one record per line.
    /// </summary>
    public static class Csv
    {
        public static string Write(BalanceTable table)
        {
            var text = new StringBuilder();
            AppendRecord(text, table.Columns);
            foreach (var row in table.Rows)
            {
                AppendRecord(text, row);
            }
            return text.ToString();
        }

        private static void AppendRecord(StringBuilder text, IEnumerable<string> cells)
        {
            text.Append(string.Join(",", cells.Select(Escape)));
            text.Append('\n');
        }

        private static string Escape(string cell)
        {
            cell ??= string.Empty;
            bool quote = cell.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 || cell != cell.Trim();
            return quote ? "\"" + cell.Replace("\"", "\"\"") + "\"" : cell;
        }

        /// <summary>Parses CSV text; the first record is the header. Blank lines are skipped.</summary>
        public static BalanceTable Read(string name, string text)
        {
            var records = ParseRecords(text);
            if (records.Count == 0)
            {
                throw new FormatException($"{name}.csv is empty (it needs at least the header row)");
            }
            var table = new BalanceTable(name, records[0]);
            foreach (var record in records.Skip(1))
            {
                table.Rows.Add(record.ToArray());
            }
            return table;
        }

        public static List<List<string>> ParseRecords(string text)
        {
            var records = new List<List<string>>();
            if (string.IsNullOrEmpty(text)) return records;
            if (text[0] == '﻿') text = text.Substring(1);

            var record = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            int line = 1;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            cell.Append('"');
                            i++;
                        }
                        else
                        {
                            quoted = false;
                        }
                    }
                    else
                    {
                        if (c == '\n') line++;
                        cell.Append(c);
                    }
                    continue;
                }

                switch (c)
                {
                    case '"':
                        quoted = true;
                        break;
                    case ',':
                        record.Add(cell.ToString());
                        cell.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(cell.ToString());
                        cell.Clear();
                        AddRecord(records, record);
                        record = new List<string>();
                        line++;
                        break;
                    default:
                        cell.Append(c);
                        break;
                }
            }

            if (quoted)
            {
                throw new FormatException($"Unclosed quote that starts before line {line}");
            }
            if (cell.Length > 0 || record.Count > 0)
            {
                record.Add(cell.ToString());
                AddRecord(records, record);
            }
            return records;
        }

        private static void AddRecord(List<List<string>> records, List<string> record)
        {
            // A blank line (one empty cell) is not a record
            if (record.Count == 1 && record[0].Length == 0) return;
            records.Add(record);
        }
    }
}
