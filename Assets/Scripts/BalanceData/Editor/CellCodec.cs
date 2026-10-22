using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Turns a serialized field into a table cell and back. Numbers use the invariant culture (a dot for decimals),
    /// booleans are TRUE / FALSE (what spreadsheets show), enums are written by name, never by index (stats and
    /// enemy types are stored by index, so an index in the data would shift when an enum changes), and asset
    /// references by asset name. Parsing checks the field's [Min] / [Range] attributes.
    /// </summary>
    public static class CellCodec
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static string Format(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return property.intValue.ToString(Invariant);
                case SerializedPropertyType.Float: return FormatFloat(property.floatValue);
                case SerializedPropertyType.Boolean: return property.boolValue ? "TRUE" : "FALSE";
                case SerializedPropertyType.String: return property.stringValue;
                case SerializedPropertyType.Enum:
                    var names = property.enumNames;
                    int index = property.enumValueIndex;
                    return index >= 0 && index < names.Length ? names[index] : property.intValue.ToString(Invariant);
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.name : string.Empty;
                default:
                    throw new NotSupportedException($"{property.propertyPath}: {property.propertyType} fields can't be a table cell");
            }
        }

        public static string FormatFloat(float value) => value.ToString(Invariant);

        /// <summary>
        /// Parses a cell for a field. On success, <paramref name="apply"/> writes the value into a property at the same
        /// path. Fails with a reason the designer can act on.
        /// </summary>
        public static bool TryParse(SerializedProperty property, string text, out Action<SerializedProperty> apply, out string error)
        {
            apply = null;
            error = null;
            text = (text ?? string.Empty).Trim();
            var field = FieldFor(property);

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (!int.TryParse(text, NumberStyles.Integer, Invariant, out int i))
                    {
                        error = $"\"{text}\" is not a whole number";
                        return false;
                    }
                    if (!InRange(field, i, out error)) return false;
                    apply = p => p.intValue = i;
                    return true;

                case SerializedPropertyType.Float:
                    if (!float.TryParse(text, NumberStyles.Float, Invariant, out float f) || float.IsNaN(f) || float.IsInfinity(f))
                    {
                        error = $"\"{text}\" is not a number (use a dot for decimals)";
                        return false;
                    }
                    if (!InRange(field, f, out error)) return false;
                    apply = p => p.floatValue = f;
                    return true;

                case SerializedPropertyType.Boolean:
                    if (!TryParseBool(text, out bool b))
                    {
                        error = $"\"{text}\" is not TRUE or FALSE";
                        return false;
                    }
                    apply = p => p.boolValue = b;
                    return true;

                case SerializedPropertyType.String:
                    string s = text;
                    apply = p => p.stringValue = s;
                    return true;

                case SerializedPropertyType.Enum:
                    int enumIndex = Array.FindIndex(property.enumNames, n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase));
                    if (enumIndex < 0)
                    {
                        error = $"\"{text}\" is not one of: {string.Join(", ", property.enumNames)}";
                        return false;
                    }
                    apply = p => p.enumValueIndex = enumIndex;
                    return true;

                case SerializedPropertyType.ObjectReference:
                    if (text.Length == 0)
                    {
                        apply = p => p.objectReferenceValue = null;
                        return true;
                    }
                    var type = ElementType(field?.FieldType) ?? typeof(Object);
                    var matches = AssetDatabase.FindAssets($"t:{type.Name}")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Select(path => AssetDatabase.LoadAssetAtPath(path, type))
                        .Where(o => o != null && o.name == text)
                        .Distinct()
                        .ToList();
                    if (matches.Count != 1)
                    {
                        error = matches.Count == 0 ? $"no {type.Name} named \"{text}\"" : $"{matches.Count} {type.Name} assets are named \"{text}\"";
                        return false;
                    }
                    var reference = matches[0];
                    apply = p => p.objectReferenceValue = reference;
                    return true;

                default:
                    error = $"{property.propertyType} fields can't be a table cell";
                    return false;
            }
        }

        /// <summary>Whether the cell already holds the field's value (floats compare with a small tolerance).</summary>
        public static bool Matches(SerializedProperty property, string text)
        {
            text = (text ?? string.Empty).Trim();
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    return float.TryParse(text, NumberStyles.Float, Invariant, out float f)
                        && Mathf.Abs(f - property.floatValue) <= 1e-6f * Mathf.Max(1f, Mathf.Abs(f));
                case SerializedPropertyType.Boolean:
                    return TryParseBool(text, out bool b) && b == property.boolValue;
                case SerializedPropertyType.Enum:
                    return string.Equals(Format(property), text, StringComparison.OrdinalIgnoreCase);
                default:
                    return Format(property) == text;
            }
        }

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

        /// <summary>The tooltip of the field behind a property, or an empty string.</summary>
        public static string TooltipOf(SerializedProperty property) =>
            FieldFor(property)?.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty;

        private static readonly Regex ArrayElement = new(@"^data\[\d+\]$");

        /// <summary>The C# field a property path ends at (list elements resolve to the list's field).</summary>
        public static FieldInfo FieldFor(SerializedProperty property)
        {
            var type = property.serializedObject.targetObject.GetType();
            FieldInfo field = null;
            foreach (string segment in property.propertyPath.Split('.'))
            {
                if (segment == "Array" || ArrayElement.IsMatch(segment))
                {
                    continue;
                }
                if (field != null)
                {
                    type = ElementType(field.FieldType);
                }
                field = FindField(type, segment);
                if (field == null) return null;
            }
            return field;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        private static Type ElementType(Type type)
        {
            if (type == null) return null;
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }
    }
}
