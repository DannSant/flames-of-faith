using Game.Combat;
using Game.Control;
using Game.Effects;
using Game.Effects.Abilities;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The spawner owns an attack's damage (data-driven step 5): the prefab's DamageSourceData only says how it hits,
    /// so every spawn path must set the damage. Item behaviors carry a damage profile, each class's dash has a damage
    /// stat, and the dash / explosion abilities have a stat share on every tier.
    /// </summary>
    public class DamageOwnershipTests
    {
        private const string BehaviorsPath = "Assets/Resources/Effects/EffectBehaviors";

        private static T[] Load<T>(string folder) where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).Where(a => a != null).ToArray();

        [Test]
        public void EverySpawningBehaviorWithADamagingPrefab_DealsDamage()
        {
            var behaviors = Load<SpawningEffectBehavior>(BehaviorsPath);
            Assert.IsNotEmpty(behaviors);
            foreach (var behavior in behaviors)
            {
                Assert.IsNotNull(behavior.PrefabToSpawn, $"{behavior.name} spawns nothing");
                if (behavior.PrefabToSpawn.GetComponentInChildren<DamageSourceBase>(true) == null) continue;
                Assert.IsTrue(behavior.Damage.DealsDamage,
                    $"{behavior.name} spawns a damage source but its damage profile is empty (Base Damage 0 and no stat share)");
            }
        }

        /// <summary>
        /// Every spawning item can be owned more than once, so a second copy must do something. Consecrate and Holy
        /// Missiles had no rule (found in the items step 5 play-test), so their extra copies did nothing.
        /// </summary>
        [Test]
        public void EverySpawningBehavior_HasAStackingRule()
        {
            foreach (var behavior in Load<SpawningEffectBehavior>(BehaviorsPath))
            {
                int rules = new SerializedObject(behavior).FindProperty("stackScaling").arraySize;
                Assert.Greater(rules, 0, $"{behavior.name} has no stacking rule (ItemBehaviorStacking), so extra copies do nothing");
            }
        }

        [Test]
        public void EveryClass_HasADashDamageStat()
        {
            foreach (var data in Load<CharacterClassData>("Assets/Resources/CharacterData"))
            {
                Assert.AreNotEqual(WeaponClass.None, data.dashDamageStat, $"{data.name}'s dash damage stat is None");
            }
        }

        [Test]
        public void DashAndExplosionTiers_HaveAStatShare()
        {
            var trap = AssetDatabase.LoadAssetAtPath<TrapStepAbility>($"{BehaviorsPath}/TrapStepAbility.asset");
            var flame = AssetDatabase.LoadAssetAtPath<FlameDashAbility>($"{BehaviorsPath}/FlameDashAbility.asset");
            var tornadoes = AssetDatabase.LoadAssetAtPath<FireTornadoesAbility>($"{BehaviorsPath}/FireTornadoesAbility.asset");
            Assert.That(trap.Tiers.Select(t => t.trapStatScale), Has.All.GreaterThan(0f), "Trap Step");
            Assert.That(flame.Tiers.Select(t => t.flameStatScale), Has.All.GreaterThan(0f), "Flame Dash");
            Assert.That(tornadoes.Tiers.Select(t => t.explosionStatScale), Has.All.GreaterThan(0f), "Fire Tornadoes");
        }

        [Test]
        public void ADamageProfile_SetsBaseStatAndShare()
        {
            var go = new GameObject("Source");
            try
            {
                var source = go.AddComponent<DamageSourceBase>();
                new DamageProfile(7, WeaponClass.Magic, 0.25f).ApplyTo(source);
                Assert.AreEqual(7, source.BaseDamage);
                Assert.AreEqual(WeaponClass.Magic, source.WeaponClass);
                Assert.AreEqual(0.25f, source.AttackScale, 1e-5f);

                Assert.IsFalse(default(DamageProfile).DealsDamage);
                Assert.IsTrue(new DamageProfile(0, WeaponClass.Melee, 1f).DealsDamage);
                Assert.IsFalse(new DamageProfile(0, WeaponClass.None, 1f).DealsDamage);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
