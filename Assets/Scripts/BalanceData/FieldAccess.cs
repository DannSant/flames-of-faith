using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Game.BalanceData
{
    /// <summary>
    /// Reads and writes a serialized field by its path, written the way Unity names serialized properties:
    /// "healthBase", "strafeFlipInterval.x", "expectedCurve.Array.data[2].armor", "tierHitsToKill.Array.data[0]".
    /// Plain reflection, so it works in builds too (the balance override applies the designer's numbers there).
    /// Structs on the way are written back, so a field of a struct inside a list changes in place.
    /// </summary>
    public static class FieldAccess
    {
        private readonly struct Step
        {
            public readonly string Field;
            public readonly int Index;
            public bool IsIndex => Field == null;

            public Step(string field, int index)
            {
                Field = field;
                Index = index;
            }
        }

        private static List<Step> Parse(string path)
        {
            var steps = new List<Step>();
            foreach (string segment in path.Split('.'))
            {
                if (segment == "Array") continue;
                if (segment.StartsWith("data[") && segment.EndsWith("]"))
                {
                    steps.Add(new Step(null, int.Parse(segment.Substring(5, segment.Length - 6))));
                }
                else
                {
                    steps.Add(new Step(segment, -1));
                }
            }
            return steps;
        }

        public static FieldInfo FindField(Type type, string name)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        public static Type ElementType(Type listType)
        {
            if (listType == null) return null;
            if (listType.IsArray) return listType.GetElementType();
            if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>)) return listType.GetGenericArguments()[0];
            return null;
        }

        /// <summary>
        /// The type a path holds and the C# field it ends at (for a list element, the list's field, which carries the
        /// [Min] / [Range] attributes). False if the path doesn't exist on the type.
        /// </summary>
        public static bool TryDescribe(Type rootType, string path, out Type valueType, out FieldInfo field)
        {
            valueType = rootType;
            field = null;
            foreach (var step in Parse(path))
            {
                if (step.IsIndex)
                {
                    valueType = ElementType(valueType);
                    if (valueType == null) return false;
                    continue;
                }
                var next = FindField(valueType, step.Field);
                if (next == null) return false;
                field = next;
                valueType = next.FieldType;
            }
            return field != null;
        }

        public static object Get(object root, string path)
        {
            object current = root;
            foreach (var step in Parse(path))
            {
                if (current == null) return null;
                if (step.IsIndex)
                {
                    current = ((IList)current)[step.Index];
                    continue;
                }
                var field = FindField(current.GetType(), step.Field) ?? throw new ArgumentException($"No field {step.Field} on the way to {path}");
                current = field.GetValue(current);
            }
            return current;
        }

        public static void Set(object root, string path, object value)
        {
            var steps = Parse(path);
            SetStep(root, steps, 0, value);
        }

        // Returns the container after the write: a boxed struct comes back changed, and the caller stores it again.
        private static object SetStep(object container, List<Step> steps, int index, object value)
        {
            var step = steps[index];
            bool last = index == steps.Count - 1;
            if (step.IsIndex)
            {
                var list = (IList)container;
                list[step.Index] = last ? value : SetStep(list[step.Index], steps, index + 1, value);
                return container;
            }

            var field = FindField(container.GetType(), step.Field) ?? throw new ArgumentException($"No field {step.Field} on {container.GetType().Name}");
            field.SetValue(container, last ? value : SetStep(field.GetValue(container), steps, index + 1, value));
            return container;
        }

        public static int Count(object root, string listPath) => (Get(root, listPath) as IList)?.Count ?? 0;

        /// <summary>
        /// Grows or shrinks a List field. New elements copy the last one, as Unity does when an array grows, so fields no
        /// table sets (a token tier's sprite) keep a sensible value.
        /// </summary>
        public static void Resize(object root, string listPath, int count)
        {
            if (!TryDescribe(root.GetType(), listPath, out var listType, out _) || ElementType(listType) == null || listType.IsArray)
            {
                throw new ArgumentException($"{listPath} is not a List field");
            }
            var list = Get(root, listPath) as IList;
            if (list == null)
            {
                list = (IList)Activator.CreateInstance(listType);
                Set(root, listPath, list);
            }

            var elementType = ElementType(listType);
            while (list.Count > count) list.RemoveAt(list.Count - 1);
            while (list.Count < count)
            {
                list.Add(list.Count > 0 ? Copy(list[list.Count - 1], elementType) : NewElement(elementType));
            }
        }

        private static object Copy(object value, Type type)
        {
            if (value == null || type.IsValueType || type == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(type)) return value;
            return JsonUtility.FromJson(JsonUtility.ToJson(value), type);
        }

        private static object NewElement(Type type) =>
            type == typeof(string) ? string.Empty : typeof(UnityEngine.Object).IsAssignableFrom(type) ? null : Activator.CreateInstance(type);
    }
}
