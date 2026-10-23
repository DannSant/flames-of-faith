using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Applies a balance import to the project: every change is recorded for Undo (one group per import), new assets
    /// are created next to the asset they belong with, and everything is saved at the end.
    /// </summary>
    public class EditorBalanceStore : IBalanceStore
    {
        private int undoGroup;

        public void BeginApply()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Import balance data");
            undoGroup = Undo.GetCurrentGroup();
        }

        public void BeforeChange(Object asset) => Undo.RecordObject(asset, "Import balance data");

        public void AfterChange(Object asset) => EditorUtility.SetDirty(asset);

        public T CreateAsset<T>(Object nextTo, string name) where T : ScriptableObject
        {
            string folder = nextTo != null ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(nextTo))?.Replace('\\', '/') : null;
            if (string.IsNullOrEmpty(folder)) folder = "Assets";
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset"));
            Undo.RegisterCreatedObjectUndo(asset, "Import balance data");
            return asset;
        }

        public void EndApply()
        {
            Undo.CollapseUndoOperations(undoGroup);
            AssetDatabase.SaveAssets();
        }
    }
}
