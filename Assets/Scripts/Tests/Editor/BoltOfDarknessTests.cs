using Game.AI;
using Game.Boss;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The boss's Bolt of Darkness splits into smaller projectiles (its EnemyExplotion). The ability never gave the
    /// explosion a damage value, so the fragments dealt 0 (tester feedback, v0.5.2). The ability now passes a share of
    /// the bolt's damage.
    /// </summary>
    public class BoltOfDarknessTests
    {
        [TestCase(30f, 0.5f, 15f)]
        [TestCase(30f, 0f, 0f)]
        [TestCase(30f, 1f, 30f)]
        [TestCase(30f, 2f, 30f)]
        [TestCase(-5f, 0.5f, 0f)]
        public void FragmentDamage_IsAShareOfTheBolt(float bolt, float fraction, float expected)
        {
            Assert.AreEqual(expected, VoltOfDarknessAbility.FragmentDamage(bolt, fraction), 0.0001f);
        }

        [Test]
        public void Initialize_SetsTheFragmentDamage()
        {
            var go = new GameObject("Bolt");
            try
            {
                var explosion = go.AddComponent<EnemyExplotion>();
                Assert.AreEqual(0f, explosion.Damage, "No damage until Initialize");
                explosion.Initialize(15f);
                Assert.AreEqual(15f, explosion.Damage);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EveryBoltThatSplits_GivesItsFragmentsDamage()
        {
            var abilities = AssetDatabase.FindAssets("t:VoltOfDarknessAbility")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<VoltOfDarknessAbility>)
                .ToList();
            Assert.IsNotEmpty(abilities, "No Bolt of Darkness abilities found");

            foreach (var ability in abilities)
            {
                if (ability.projectilePrefab == null || !ability.projectilePrefab.TryGetComponent(out EnemyExplotion _)) continue;
                Assert.Greater(ability.FragmentDamageFraction, 0f,
                    $"{ability.name}: its bolt splits into projectiles, which deal no damage at a Fragment Damage Fraction of 0");
            }
        }
    }
}
