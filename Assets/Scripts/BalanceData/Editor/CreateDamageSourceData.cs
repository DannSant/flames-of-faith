using Game.Combat;
using Game.Combat.Projectiles;
using Game.Misc;
using Game.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.BalanceData.Editor
{
    /// <summary>
    /// Gives every DamageSourceBase in a prefab its own <see cref="DamageSourceData"/> asset, filled
    /// with the values the prefab has now: the damage source's fields, its projectile movement's speed
    /// and its lifetime component's duration. Skips sources that already have one, so it can be run
    /// again after adding a prefab. Sources inside a nested prefab are handled in that prefab.
    /// </summary>
    public static class CreateDamageSourceData
    {
        public const string OutputFolder = "Assets/Resources/Combat/DamageSources";
        private const string DamageSourceScriptPath = "Assets/Scripts/Combat/DamageSourceBase.cs";

        // Same names on DamageSourceBase (inline, before the move) and on DamageSourceData.
        private static readonly string[] SharedFields =
        {
            "baseDamage", "weaponClass", "attackScale",
            "useTriggerEnter", "useTriggerStay", "tickInterval", "shouldStackDamage",
            "enablePierce", "pierceCount", "addPierceStat",
            "applyKnockback", "knockbackForce", "canTriggerLifesteal", "directElementalDebuffData",
        };

        [MenuItem("Tools/Flames of Faith/Balance Data/Create Damage Source Data")]
        public static void Run()
        {
            var log = Create();
            EditorUtility.DisplayDialog("Damage source data", log.Count == 0 ? "Every damage source already has its data."
                : $"Created {log.Count} assets in {OutputFolder}. See the Console.", "OK");
        }

        /// <summary>Batchmode entry (-executeMethod Game.BalanceData.Editor.CreateDamageSourceData.RunBatch).</summary>
        public static void RunBatch() => Create();

        private static List<string> Create()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(OutputFolder).Replace('\\', '/'), Path.GetFileName(OutputFolder));
            }

            var log = new List<string>();
            // Cheap text filter first: only prefabs whose YAML mentions the script get loaded.
            string scriptGuid = AssetDatabase.AssetPathToGUID(DamageSourceScriptPath);
            var prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p);

            foreach (string path in prefabPaths)
            {
                if (!File.ReadAllText(path).Contains(scriptGuid)) continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool changed = false;
                    foreach (var source in root.GetComponentsInChildren<DamageSourceBase>(true))
                    {
                        if (PrefabUtility.IsPartOfPrefabInstance(source)) continue;
                        var sourceObject = new SerializedObject(source);
                        var dataProperty = sourceObject.FindProperty("data");
                        if (dataProperty.objectReferenceValue != null) continue;

                        string name = source.gameObject == root
                            ? root.name
                            : $"{Path.GetFileNameWithoutExtension(path)}_{source.gameObject.name}";
                        var data = CreateAsset(source, sourceObject, name);
                        dataProperty.objectReferenceValue = data;
                        sourceObject.ApplyModifiedPropertiesWithoutUndo();
                        changed = true;
                        log.Add($"{path} ({source.gameObject.name}) -> {AssetDatabase.GetAssetPath(data)}");
                    }

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[CreateDamageSourceData] " + (log.Count == 0 ? "Nothing to create." : string.Join("\n", log)));
            return log;
        }

        private static DamageSourceData CreateAsset(DamageSourceBase source, SerializedObject sourceObject, string name)
        {
            var data = ScriptableObject.CreateInstance<DamageSourceData>();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{OutputFolder}/{name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);

            var dataObject = new SerializedObject(data);
            foreach (string field in SharedFields)
            {
                CopyValue(sourceObject.FindProperty(field), dataObject.FindProperty(field));
            }

            var movement = source.GetComponent<ProjectileMovementBase>();
            if (movement != null)
            {
                dataObject.FindProperty("projectileSpeed").floatValue = new SerializedObject(movement).FindProperty("speed").floatValue;
            }

            if (source.TryGetComponent(out LifetimeByStat lifetimeByStat))
            {
                var lifetimeObject = new SerializedObject(lifetimeByStat);
                dataObject.FindProperty("lifetime").floatValue = lifetimeObject.FindProperty("baseDurationTime").floatValue;
                dataObject.FindProperty("lifetimePerSkillDuration").floatValue = lifetimeObject.FindProperty("durationScalingFactor").floatValue;
            }
            else if (source.TryGetComponent(out DestroyAfterTime destroyAfterTime))
            {
                dataObject.FindProperty("lifetime").floatValue = new SerializedObject(destroyAfterTime).FindProperty("timeToDestroy").floatValue;
            }

            dataObject.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private static void CopyValue(SerializedProperty from, SerializedProperty to)
        {
            if (from == null || to == null) return;
            switch (from.propertyType)
            {
                case SerializedPropertyType.Integer: to.intValue = from.intValue; break;
                case SerializedPropertyType.Float: to.floatValue = from.floatValue; break;
                case SerializedPropertyType.Boolean: to.boolValue = from.boolValue; break;
                case SerializedPropertyType.Enum: to.enumValueIndex = from.enumValueIndex; break;
                case SerializedPropertyType.ObjectReference: to.objectReferenceValue = from.objectReferenceValue; break;
                default: Debug.LogWarning($"[CreateDamageSourceData] Can't copy {from.propertyPath} ({from.propertyType})"); break;
            }
        }
    }
}
