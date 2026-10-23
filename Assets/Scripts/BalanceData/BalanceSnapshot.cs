using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BalanceData
{
    /// <summary>
    /// One published version of the designer's sheet, as the sheet's web app serves it
    /// (BalanceData/Sheets/BalanceSync.gs): every data tab as CSV text, plus who published it, when and why.
    /// JsonUtility-friendly on purpose, so builds can read it without another JSON library.
    /// </summary>
    [Serializable]
    public class BalanceSnapshot
    {
        public int version;
        public string publishedAt;
        public string author;
        public string note;
        public string hash;
        public BalanceSnapshotTable[] tables;
        /// <summary>Set by the web app instead of the rest when the request failed.</summary>
        public string error;

        public static BalanceSnapshot FromJson(string json) => JsonUtility.FromJson<BalanceSnapshot>(json);

        /// <summary>The tables by name. A CSV that can't be read is an error, listed in <paramref name="errors"/>.</summary>
        public Dictionary<string, BalanceTable> ReadTables(List<string> errors)
        {
            var result = new Dictionary<string, BalanceTable>();
            foreach (var table in tables ?? Array.Empty<BalanceSnapshotTable>())
            {
                try
                {
                    result[table.name] = Csv.Read(table.name, table.csv);
                }
                catch (FormatException e)
                {
                    errors.Add($"{table.name}: {e.Message}");
                }
            }
            return result;
        }

        public string Describe() => $"version {version} by {author} ({publishedAt}){(string.IsNullOrEmpty(note) ? string.Empty : ": " + note)}";
    }

    [Serializable]
    public class BalanceSnapshotTable
    {
        public string name;
        public string csv;
    }
}
