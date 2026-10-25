using Game.Combat;
using Game.Combat.Projectiles;
using Game.Misc;
using Game.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Every player damage source keeps its design numbers in one <see cref="DamageSourceData"/> asset
    /// (Resources/Combat/DamageSources) instead of on its prefab's components. These check that each one
    /// has its asset with sane values, and that what spawners set at runtime still wins over the asset.
    /// </summary>
    public class DamageSourceDataTests
    {
        private const string DataFolder = "Assets/Resources/Combat/DamageSources";

        private static List<(string prefab, DamageSourceBase source)> PrefabSources()
        {
            var result = new List<(string, DamageSourceBase)>();
            foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var source in prefab.GetComponentsInChildren<DamageSourceBase>(true))
                {
                    // A source inside a nested prefab is checked in that prefab.
                    if (PrefabUtility.GetCorrespondingObjectFromSource(source) != null) continue;
                    result.Add((path, source));
                }
            }
            return result;
        }

        [Test]
        public void EveryDamageSource_HasItsData()
        {
            var sources = PrefabSources();
            Assert.IsNotEmpty(sources);
            var missing = sources.Where(s => s.source.Data == null)
                .Select(s => $"{s.prefab} ({s.source.gameObject.name})")
                .ToList();
            Assert.IsEmpty(missing, "Damage sources without a DamageSourceData (run Tools/Flames of Faith/Balance Data/" +
                "Create Damage Source Data):\n" + string.Join("\n", missing));
        }

        [Test]
        public void EveryDataAsset_IsUsedByOneDamageSource()
        {
            var used = PrefabSources().Where(s => s.source.Data != null).GroupBy(s => s.source.Data).ToList();
            var shared = used.Where(g => g.Count() > 1).Select(g => $"{g.Key.name}: {string.Join(", ", g.Select(s => s.prefab))}").ToList();
            Assert.IsEmpty(shared, "Data assets shared by several damage sources (give each its own row):\n" + string.Join("\n", shared));

            var usedData = new HashSet<DamageSourceData>(used.Select(g => g.Key));
            var unused = AssetDatabase.FindAssets("t:DamageSourceData", new[] { DataFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !usedData.Contains(AssetDatabase.LoadAssetAtPath<DamageSourceData>(p)))
                .ToList();
            Assert.IsEmpty(unused, "DamageSourceData no prefab uses:\n" + string.Join("\n", unused));
        }

        [Test]
        public void EveryDataAsset_HasSaneValues()
        {
            var problems = new List<string>();
            foreach (var (prefab, source) in PrefabSources().Where(s => s.source.Data != null))
            {
                var data = source.Data;
                string name = data.name;
                if (!data.UseTriggerEnter && !data.UseTriggerStay) problems.Add($"{name}: neither trigger enter nor stay, it never hits");
                if (data.UseTriggerStay && data.TickInterval <= 0f) problems.Add($"{name}: trigger stay needs a tick interval > 0");
                if (data.EnablePierce && data.PierceCount < 1) problems.Add($"{name}: pierce count below 1");

                bool moves = source.GetComponent<ProjectileMovementBase>() != null;
                if (moves && data.ProjectileSpeed <= 0f) problems.Add($"{name}: {prefab} is a projectile but its speed is 0");
                if (!moves && data.ProjectileSpeed > 0f) problems.Add($"{name}: has a speed but {prefab} has no projectile movement");

                bool expires = source.GetComponent<LifetimeByStat>() != null || source.GetComponent<DestroyAfterTime>() != null;
                if (expires && data.Lifetime <= 0f) problems.Add($"{name}: {prefab} has a lifetime component but its lifetime is 0");
                if (!expires && data.Lifetime > 0f) problems.Add($"{name}: has a lifetime but {prefab} has no lifetime component");
            }
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        // --- Runtime: the asset gives defaults, spawners override them -------------------------------

        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private DamageSourceData MakeData(int pierceCount = 1, float knockbackForce = 5f)
        {
            var data = ScriptableObject.CreateInstance<DamageSourceData>();
            created.Add(data);
            var so = new SerializedObject(data);
            so.FindProperty("pierceCount").intValue = pierceCount;
            so.FindProperty("knockbackForce").floatValue = knockbackForce;
            so.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private DamageSourceBase MakeSource(DamageSourceData data)
        {
            var go = new GameObject("DamageSource");
            created.Add(go);
            var source = go.AddComponent<DamageSourceBase>();
            var so = new SerializedObject(source);
            so.FindProperty("data").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
            return source;
        }

        [Test]
        public void Source_LoadsHowItHits_AndNoDamage()
        {
            var source = MakeSource(MakeData(pierceCount: 3, knockbackForce: 7f));

            Assert.AreEqual(3, source.PierceCount);
            Assert.AreEqual(7f, source.KnockbackForce, 1e-5f);
            Assert.AreEqual(0, source.BaseDamage, "The damage comes from the spawner, not the data");
            Assert.AreEqual(WeaponClass.None, source.WeaponClass);
        }

        [Test]
        public void SpawnerDamage_SticksEvenBeforeTheSourceIsActive()
        {
            // Nothing has read the data yet (no Awake in edit mode), like a source configured before activation.
            var source = MakeSource(MakeData(knockbackForce: 7f));
            new DamageProfile(40, WeaponClass.Ranged, 0.5f).ApplyTo(source);

            Assert.AreEqual(40, source.BaseDamage, "Loading the data must not clear the spawner's damage");
            Assert.AreEqual(WeaponClass.Ranged, source.WeaponClass);
            Assert.AreEqual(0.5f, source.AttackScale, 1e-5f);
            Assert.AreEqual(7f, source.KnockbackForce, 1e-5f);
        }

        [Test]
        public void SplitClone_KeepsTheOriginalsRuntimeValues()
        {
            var original = MakeSource(MakeData());
            original.Initialize(9, 3, "item-id", WeaponClass.Ranged, null);
            original.SetAttackScale(0.75f);

            var clone = MakeSource(MakeData());
            clone.CopyRuntimeFrom(original);

            Assert.AreEqual(9, clone.BaseDamage);
            Assert.AreEqual(3, clone.PierceCount);
            Assert.AreEqual(WeaponClass.Ranged, clone.WeaponClass);
            Assert.AreEqual(0.75f, clone.AttackScale, 1e-5f);
        }
    }
}
