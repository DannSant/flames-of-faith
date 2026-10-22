using Game.Control;
using Game.Effects;
using Game.Progression;
using Game.UI;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Retired stats (Cleave, Oct 2026) keep their StatType, because stats are stored by index, but are out of
    /// the game: no item grants them, no class recommends them, they are never offered on level-up and the
    /// Stats pane hides them.
    /// </summary>
    public class RetiredStatTests
    {
        private static StatDatabase Database()
        {
            var database = Resources.Load<StatDatabase>("Progression/StatDatabase");
            Assert.IsNotNull(database, "Missing Resources/Progression/StatDatabase.asset");
            return database;
        }

        private static HashSet<StatType> RetiredStats() =>
            new(Database().Stats.Where(s => s.Retired).Select(s => s.StatType));

        [Test]
        public void Cleave_IsRetiredAndNeverOffered()
        {
            var cleave = Database().Stats.Single(s => s.StatType == StatType.Cleave);
            Assert.IsTrue(cleave.Retired, "Cleave was removed from the game (Oct 2026)");
            Assert.IsFalse(cleave.AllowLevelUpgrades);
        }

        [Test]
        public void RetiredStats_AreNeverOfferedOnLevelUp()
        {
            foreach (var stat in Database().Stats.Where(s => s.Retired))
            {
                Assert.IsFalse(stat.AllowLevelUpgrades, $"{stat.StatType} is retired but still allows level-up upgrades");
            }
        }

        [Test]
        public void NoItem_GrantsARetiredStat()
        {
            var retired = RetiredStats();
            var effects = AssetDatabase.FindAssets("t:Effect", new[] { "Assets/Resources/Effects" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Effect>)
                .Where(e => e != null)
                .ToList();
            Assert.IsNotEmpty(effects);

            foreach (var effect in effects)
            {
                foreach (var modifier in effect.StatModifiers ?? new List<StatModifier>())
                {
                    Assert.IsFalse(modifier != null && retired.Contains(modifier.stat),
                        $"{effect.name} grants {modifier?.stat}, a retired stat: remove it in the Effect Database and regenerate");
                }
                foreach (var rule in effect.StatDependencies ?? new List<StatDependencyRule>())
                {
                    if (rule == null) continue;
                    Assert.IsFalse(retired.Contains(rule.sourceStat) || retired.Contains(rule.targetStat),
                        $"{effect.name} has a stat bonus rule on a retired stat ({rule.sourceStat} -> {rule.targetStat})");
                }
            }
        }

        [Test]
        public void NoClass_RecommendsARetiredStat()
        {
            var retired = RetiredStats();
            var classes = AssetDatabase.FindAssets("t:CharacterClassData")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CharacterClassData>)
                .Where(c => c != null)
                .ToList();
            Assert.IsNotEmpty(classes);

            foreach (var classData in classes)
            {
                foreach (var priority in classData.statPriorities ?? new List<ClassStatPriority>())
                {
                    Assert.IsFalse(retired.Contains(priority.stat), $"{classData.name} lists {priority.stat}, a retired stat");
                }
            }
        }

        [Test]
        public void StatsPane_HidesRetiredStats()
        {
            var database = Database();
            Assert.IsFalse(StatsPaneUI.ShowsInStatsPane(database.Stats.Single(s => s.StatType == StatType.Cleave)), "Cleave has no row");
            Assert.IsTrue(StatsPaneUI.ShowsInStatsPane(database.Stats.Single(s => s.StatType == StatType.Armor)), "Live stats keep their row");
            Assert.IsFalse(StatsPaneUI.ShowsInStatsPane(null), "A stat without data has no row");
        }
    }
}
