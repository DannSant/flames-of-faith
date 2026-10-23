using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.BalanceData
{
    /// <summary>
    /// Where an import writes. In the Editor (EditorBalanceStore) changes are recorded for Undo, new assets are created
    /// on disk and everything is saved. In a build (<see cref="RuntimeBalanceStore"/>) they only change the loaded
    /// assets, for the session.
    /// </summary>
    public interface IBalanceStore
    {
        void BeginApply();
        void BeforeChange(Object asset);
        void AfterChange(Object asset);
        /// <summary>A new asset of the given type, next to <paramref name="nextTo"/> when that means anything.</summary>
        T CreateAsset<T>(Object nextTo, string name) where T : ScriptableObject;
        void EndApply();
    }

    public class RuntimeBalanceStore : IBalanceStore
    {
        public void BeginApply() { }
        public void BeforeChange(Object asset) { }
        public void AfterChange(Object asset) { }

        public T CreateAsset<T>(Object nextTo, string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            return asset;
        }

        public void EndApply() { }
    }

    /// <summary>
    /// What an import would do, worked out before anything is touched: the errors (any error cancels the whole
    /// import), and the changes as readable diff lines ("SlimeChaser.Health Base: 50 -> 60") with the actions that
    /// make them. <see cref="Apply"/> runs the actions in order through the store.
    /// </summary>
    public class ImportPlan
    {
        private readonly List<string> errors = new();
        private readonly List<string> changes = new();
        private readonly List<Action> actions = new();
        private readonly Dictionary<string, Func<Object>> pendingAssets = new();

        public ImportPlan(IBalanceStore store = null)
        {
            Store = store ?? new RuntimeBalanceStore();
        }

        public IBalanceStore Store { get; }
        public IReadOnlyList<string> Errors => errors;
        public IReadOnlyList<string> Changes => changes;
        public bool HasErrors => errors.Count > 0;

        public void Error(string table, int line, string message) => errors.Add($"{table}, line {line}: {message}");

        public void Error(string table, string message) => errors.Add($"{table}: {message}");

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
        public void SetField(string description, Func<Object> target, string path, object value)
        {
            Do(description, () =>
            {
                var asset = target();
                Store.BeforeChange(asset);
                FieldAccess.Set(asset, path, value);
                Store.AfterChange(asset);
            });
        }

        /// <summary>Grows or shrinks a list field.</summary>
        public void ResizeList(string description, Func<Object> target, string listPath, int count)
        {
            Do(description, () =>
            {
                var asset = target();
                Store.BeforeChange(asset);
                FieldAccess.Resize(asset, listPath, count);
                Store.AfterChange(asset);
            });
        }

        public void Apply()
        {
            if (HasErrors)
            {
                throw new InvalidOperationException("An import with errors can't be applied");
            }
            Store.BeginApply();
            try
            {
                foreach (var action in actions) action();
            }
            finally
            {
                Store.EndApply();
            }
        }
    }
}
