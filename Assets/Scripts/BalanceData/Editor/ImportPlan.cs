using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// What an import would do, worked out before anything is touched: the errors (any error cancels the whole
    /// import), and the changes as readable diff lines ("SlimeChaser.healthBase: 50 -> 60") with the actions that
    /// make them. <see cref="Apply"/> runs the actions in order, with Undo, and saves.
    /// </summary>
    public class ImportPlan
    {
        private readonly List<string> errors = new();
        private readonly List<string> changes = new();
        private readonly List<Action> actions = new();
        private readonly Dictionary<string, Func<Object>> pendingAssets = new();

        public IReadOnlyList<string> Errors => errors;
        public IReadOnlyList<string> Changes => changes;
        public bool HasErrors => errors.Count > 0;

        public void Error(string table, int line, string message) => errors.Add($"{table}.csv, line {line}: {message}");

        public void Error(string table, string message) => errors.Add($"{table}.csv: {message}");

        /// <summary>Adds a change: its diff line and the action that makes it.</summary>
        public void Do(string description, Action action)
        {
            changes.Add(description);
            actions.Add(action);
        }

        /// <summary>
        /// Registers an asset a change creates (e.g. a new wave), so a later table can change it too. The resolver
        /// returns the asset once the creating action has run.
        /// </summary>
        public void AddPendingAsset(string key, Func<Object> resolve) => pendingAssets[key] = resolve;

        public bool TryGetPendingAsset(string key, out Func<Object> resolve) => pendingAssets.TryGetValue(key, out resolve);

        public IEnumerable<string> PendingAssetKeys => pendingAssets.Keys;

        /// <summary>Sets one field of an asset; the asset is resolved when the change runs.</summary>
        public void SetField(string description, Func<Object> target, string propertyPath, Action<SerializedProperty> setter)
        {
            Do(description, () =>
            {
                var asset = target();
                var serialized = new SerializedObject(asset);
                var property = serialized.FindProperty(propertyPath);
                if (property == null)
                {
                    throw new InvalidOperationException($"{asset.name} has no field {propertyPath}");
                }
                setter(property);
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
            });
        }

        public void Apply()
        {
            if (HasErrors)
            {
                throw new InvalidOperationException("An import with errors can't be applied");
            }
            Undo.SetCurrentGroupName("Import balance data");
            int group = Undo.GetCurrentGroup();
            foreach (var action in actions)
            {
                action();
            }
            Undo.CollapseUndoOperations(group);
            AssetDatabase.SaveAssets();
        }
    }
}
