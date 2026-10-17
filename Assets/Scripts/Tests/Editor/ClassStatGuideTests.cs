using Game.Control;
using Game.Progression;
using Game.UI;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// A class's recommended stats: the pure rules (role, order, tab, tooltip line) and guards over
    /// the real class assets and the star on the Stats pane rows and upgrade buttons.
    /// </summary>
    public class ClassStatGuideTests
    {
        private static ClassStatPriority P(StatType stat, StatRole role, string why = "why") =>
            new ClassStatPriority { stat = stat, role = role, justification = why };

        // --- Pure rules --------------------------------------------------------------------

        [Test]
        public void Sort_KeyThenComplementaryInAuthoredOrder_ThenTheRest()
        {
            var priorities = new List<ClassStatPriority>
            {
                P(StatType.Armor, StatRole.Complementary),
                P(StatType.MeleeDamage, StatRole.Key),
                P(StatType.Retribution, StatRole.Complementary),
            };
            var stats = new[] { StatType.MaxHealth, StatType.MeleeDamage, StatType.AttackSpeed, StatType.Armor, StatType.Retribution, StatType.Luck };

            CollectionAssert.AreEqual(
                new[] { StatType.MeleeDamage, StatType.Armor, StatType.Retribution, StatType.MaxHealth, StatType.AttackSpeed, StatType.Luck },
                ClassStatGuide.SortForDisplay(stats, priorities));
        }

        [Test]
        public void Sort_WithoutAGuide_KeepsTheOrder()
        {
            var stats = new[] { StatType.Luck, StatType.MaxHealth, StatType.Armor };
            CollectionAssert.AreEqual(stats, ClassStatGuide.SortForDisplay(stats, null));
            CollectionAssert.AreEqual(stats, ClassStatGuide.SortForDisplay(stats, new List<ClassStatPriority>()));
        }

        [Test]
        public void Sort_IgnoresGuideStatsThatAreNotShown()
        {
            var priorities = new List<ClassStatPriority> { P(StatType.Pierce, StatRole.Key) };
            var stats = new[] { StatType.MaxHealth, StatType.Armor };
            CollectionAssert.AreEqual(stats, ClassStatGuide.SortForDisplay(stats, priorities));
        }

        [Test]
        public void Role_ComesFromTheGuide_NoneOtherwise()
        {
            var priorities = new List<ClassStatPriority>
            {
                P(StatType.RangedDamage, StatRole.Key),
                P(StatType.Evasion, StatRole.Complementary),
                P(StatType.Luck, StatRole.None),
            };
            Assert.AreEqual(StatRole.Key, ClassStatGuide.GetRole(priorities, StatType.RangedDamage));
            Assert.AreEqual(StatRole.Complementary, ClassStatGuide.GetRole(priorities, StatType.Evasion));
            Assert.AreEqual(StatRole.None, ClassStatGuide.GetRole(priorities, StatType.Luck));
            Assert.AreEqual(StatRole.None, ClassStatGuide.GetRole(priorities, StatType.Armor));
            Assert.AreEqual(StatRole.None, ClassStatGuide.GetRole(null, StatType.Armor));
        }

        [TestCase(StatCategory.Primary, StatRole.None, true)]
        [TestCase(StatCategory.Secondary, StatRole.None, false)]
        [TestCase(StatCategory.Secondary, StatRole.Key, true)]
        [TestCase(StatCategory.Secondary, StatRole.Complementary, true)]
        public void ClassStats_AlwaysShowOnThePrimaryTab(StatCategory category, StatRole role, bool expected)
        {
            Assert.AreEqual(expected, ClassStatGuide.ShowsOnPrimaryTab(category, role));
        }

        [Test]
        public void DescribeRole_NamesTheRoleTheClassAndWhy()
        {
            Assert.AreEqual("Key stat for the Warrior: Swords hit hard.",
                ClassStatGuide.DescribeRole(P(StatType.MeleeDamage, StatRole.Key, " Swords hit hard. "), "Warrior"));
            Assert.AreEqual("Complementary stat for the Archer",
                ClassStatGuide.DescribeRole(P(StatType.Range, StatRole.Complementary, ""), "Archer"));
            Assert.AreEqual("Key stat: why", ClassStatGuide.DescribeRole(P(StatType.MeleeDamage, StatRole.Key), null));
            Assert.IsNull(ClassStatGuide.DescribeRole(null, "Warrior"));
            Assert.IsNull(ClassStatGuide.DescribeRole(P(StatType.Luck, StatRole.None), "Warrior"));
        }

        // --- The real class assets ---------------------------------------------------------

        private static CharacterClassData LoadClass(string name)
        {
            string path = $"Assets/Resources/CharacterData/{name}.asset";
            var data = AssetDatabase.LoadAssetAtPath<CharacterClassData>(path);
            Assert.IsNotNull(data, $"Missing {path}");
            return data;
        }

        private static readonly object[] Specs =
        {
            new object[] { "Warrior", StatType.MeleeDamage, new[] { StatType.Retribution, StatType.Armor, StatType.MaxHealth, StatType.DashCooldown } },
            new object[] { "Archer", StatType.RangedDamage, new[] { StatType.Evasion, StatType.Pierce, StatType.Range } },
            new object[] { "Augur", StatType.MagicDamage, new[] { StatType.MastowAffinity, StatType.SkillDuration } },
        };

        [TestCaseSource(nameof(Specs))]
        public void Class_HasItsKeyAndComplementaryStats(string name, StatType key, StatType[] complementary)
        {
            var guide = LoadClass(name).statPriorities;
            Assert.IsNotNull(guide, $"{name} has no stat guide");

            var keys = guide.Where(p => p.role == StatRole.Key).Select(p => p.stat).ToList();
            CollectionAssert.AreEqual(new[] { key }, keys, $"{name} should have exactly one Key stat");
            CollectionAssert.AreEquivalent(complementary,
                guide.Where(p => p.role == StatRole.Complementary).Select(p => p.stat), $"{name} Complementary stats");
        }

        [TestCase("Warrior")]
        [TestCase("Archer")]
        [TestCase("Augur")]
        public void ClassGuide_IsWellFormed(string name)
        {
            var guide = LoadClass(name).statPriorities;
            Assert.IsTrue(guide.All(p => p != null && p.role != StatRole.None), $"{name}: an entry has no role");
            Assert.AreEqual(guide.Count, guide.Select(p => p.stat).Distinct().Count(), $"{name}: a stat is listed twice");
            foreach (var p in guide)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(p.justification), $"{name}: {p.stat} has no justification");
            }
        }

        // --- The stars on the prefabs ------------------------------------------------------

        [TestCase("Assets/Prefabs/UI/Progression/StatRow.prefab", typeof(StatRowUI))]
        [TestCase("Assets/Prefabs/UI/Progression/UpgradeButton.prefab", typeof(UpgradeButtonUI))]
        public void Prefab_HasItsRoleStar(string path, System.Type component)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Missing prefab {path}");
            var owner = prefab.GetComponentInChildren(component, true);
            Assert.IsNotNull(owner, $"{path} has no {component.Name}");

            var star = new SerializedObject(owner).FindProperty("roleStar")?.objectReferenceValue as Image;
            Assert.IsNotNull(star, $"{path}: roleStar is not set");
            Assert.IsNotNull(star.sprite, $"{path}: the role star has no sprite");
        }
    }
}
