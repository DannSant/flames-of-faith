using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.BalanceData
{
    /// <summary>
    /// Turns a field's value into a table cell and back. Numbers use the invariant culture (a dot for decimals),
    /// booleans are TRUE / FALSE (what spreadsheets show), enums are written by name, never by index (stats and
    /// enemy types are stored by index, so an index in the data would shift when an enum changes), and asset
    /// references by asset name (assets under a Resources folder). Parsing checks the field's [Min] / [Range].
    /// </summary>
    public static class CellCodec
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>
        /// Where the assets a reference column can name live, per asset type (a Resources path). A reference is looked
        /// up there by name; only these folders are loaded, which keeps it cheap in builds.
        /// </summary>
        public static readonly System.Collections.Generic.Dictionary<Type, string> ReferenceFolders = new();

        // The assets of each reference folder, loaded once per import (an item table has a reference in every row)
        private static readonly System.Collections.Generic.Dictionary<(Type, string), Object[]> referenceCache = new();

        /// <summary>Forgets the loaded reference folders; BalanceTables.Plan calls it before every import.</summary>
        public static void ClearReferenceCache() => referenceCache.Clear();

        public static bool IsSupported(Type type) =>
            type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(string)
            || type.IsEnum || typeof(Object).IsAssignableFrom(type);

        public static string Format(object value, Type type)
        {
            if (type == typeof(int)) return ((int)value).ToString(Invariant);
            if (type == typeof(float)) return FormatFloat((float)value);
            if (type == typeof(bool)) return (bool)value ? "TRUE" : "FALSE";
            if (type == typeof(string)) return (string)value ?? string.Empty;
            if (type.IsEnum) return Enum.GetName(type, value) ?? Convert.ToInt32(value).ToString(Invariant);
            if (typeof(Object).IsAssignableFrom(type)) return value is Object o && o != null ? o.name : string.Empty;
            throw new NotSupportedException($"{type.Name} fields can't be a table cell");
        }

        public static string FormatFloat(float value) => value.ToString(Invariant);

        /// <summary>Parses a cell for a field of <paramref name="type"/>; fails with a reason the designer can act on.</summary>
        public static bool TryParse(string text, Type type, FieldInfo field, out object value, out string error)
        {
            value = null;
            error = null;
            text = (text ?? string.Empty).Trim();

            if (type == typeof(int))
            {
                if (!int.TryParse(text, NumberStyles.Integer, Invariant, out int i))
                {
                    error = $"\"{text}\" is not a whole number";
                    return false;
                }
                value = i;
                return InRange(field, i, out error);
            }
            if (type == typeof(float))
            {
                if (!float.TryParse(text, NumberStyles.Float, Invariant, out float f) || float.IsNaN(f) || float.IsInfinity(f))
                {
                    error = $"\"{text}\" is not a number (use a dot for decimals)";
                    return false;
                }
                value = f;
                return InRange(field, f, out error);
            }
            if (type == typeof(bool))
            {
                if (!TryParseBool(text, out bool b))
                {
                    error = $"\"{text}\" is not TRUE or FALSE";
                    return false;
                }
                value = b;
                return true;
            }
            if (type == typeof(string))
            {
                value = NormalizeText(text);
                return true;
            }
            if (type.IsEnum)
            {
                string name = Enum.GetNames(type).FirstOrDefault(n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase));
                if (name == null)
                {
                    error = $"\"{text}\" is not one of: {string.Join(", ", Enum.GetNames(type))}";
                    return false;
                }
                value = Enum.Parse(type, name);
                return true;
            }
            if (typeof(Object).IsAssignableFrom(type))
            {
                if (text.Length == 0) return true;
                if (!ReferenceFolders.TryGetValue(type, out string folder))
                {
                    error = $"{type.Name} references can't be set from a table (no Resources folder registered for them)";
                    return false;
                }
                if (!referenceCache.TryGetValue((type, folder), out var assets))
                {
                    referenceCache[(type, folder)] = assets = Resources.LoadAll(folder, type).Where(o => o != null).Distinct().ToArray();
                }
                var matches = assets.Where(o => o.name == text).ToList();
                if (matches.Count != 1)
                {
                    error = matches.Count == 0 ? $"no {type.Name} named \"{text}\" in Resources/{folder}" : $"{matches.Count} {type.Name} assets are named \"{text}\"";
                    return false;
                }
                value = matches[0];
                return true;
            }

            error = $"{type.Name} fields can't be a table cell";
            return false;
        }

        /// <summary>Whether the cell already holds the value (floats compare with a small tolerance).</summary>
        public static bool Matches(object current, Type type, string text)
        {
            text = (text ?? string.Empty).Trim();
            if (type == typeof(float))
            {
                return float.TryParse(text, NumberStyles.Float, Invariant, out float f)
                    && Mathf.Abs(f - (float)current) <= 1e-6f * Mathf.Max(1f, Mathf.Abs(f));
            }
            if (type == typeof(bool)) return TryParseBool(text, out bool b) && b == (bool)current;
            if (type.IsEnum) return string.Equals(Format(current, type), text, StringComparison.OrdinalIgnoreCase);
            if (type == typeof(string)) return NormalizeText(Format(current, type)) == NormalizeText(text);
            return Format(current, type) == text;
        }

        /// <summary>
        /// Text as a cell holds it: Windows line ends become \n and outer whitespace goes (sheets trim cells), so a
        /// description with a trailing line break still reads back as unchanged.
        /// </summary>
        public static string NormalizeText(string text) => (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Trim();

        private static bool TryParseBool(string text, out bool value)
        {
            switch (text.ToUpperInvariant())
            {
                case "TRUE": case "1": case "YES": value = true; return true;
                case "FALSE": case "0": case "NO": value = false; return true;
                default: value = false; return false;
            }
        }

        private static bool InRange(FieldInfo field, float value, out string error)
        {
            error = null;
            if (field == null) return true;
            var min = field.GetCustomAttribute<MinAttribute>();
            if (min != null && value < min.min)
            {
                error = $"{FormatFloat(value)} is below the minimum {FormatFloat(min.min)}";
                return false;
            }
            var range = field.GetCustomAttribute<RangeAttribute>();
            if (range != null && (value < range.min || value > range.max))
            {
                error = $"{FormatFloat(value)} is outside {FormatFloat(range.min)}..{FormatFloat(range.max)}";
                return false;
            }
            return true;
        }

        /// <summary>The field's tooltip, or an empty string.</summary>
        public static string TooltipOf(FieldInfo field) => field?.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty;
    }
}
