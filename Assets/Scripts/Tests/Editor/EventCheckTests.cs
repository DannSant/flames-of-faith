using Game.Control;
using Game.Progression;
using Game.RunEncounters;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The event stat checks: the chance formula, stat scales, current Grace, the class's Key stat, and the
    /// real settings asset.
    /// </summary>
    public class EventCheckTests
    {
        private const string SettingsPath = "Assets/Resources/RunEncounters/EventCheckSettings.asset";

        private static EventCheckSettings Settings()
        {
            var s = ScriptableObject.CreateInstance<EventCheckSettings>();
            s.easyChance = 0.75f;
            s.evenChance = 0.5f;
            s.hardChance = 0.3f;
            s.defaultChancePerPoint = 0.03f;
            s.graceChancePerPoint = 0.02f;
            s.minChance = 0.05f;
            s.maxChance = 0.95f;
            s.statScales = new List<EventCheckStatScale>
            {
                new EventCheckStatScale { stat = StatType.MaxHealth, baseline = 20f, chancePerPoint = 0.01f }
            };
            return s;
        }

        [Test]
        public void Chance_IsBasePlusPointsAboveBaseline_Clamped()
        {
            Assert.AreEqual(0.5f, EventCheckMath.GetChance(0.5f, 0f, 0f, 0.03f, 0.05f, 0.95f), 1e-5f);
            Assert.AreEqual(0.8f, EventCheckMath.GetChance(0.5f, 10f, 0f, 0.03f, 0.05f, 0.95f), 1e-5f);
            Assert.AreEqual(0.95f, EventCheckMath.GetChance(0.5f, 100f, 0f, 0.03f, 0.05f, 0.95f), 1e-5f);
            Assert.AreEqual(0.05f, EventCheckMath.GetChance(0.5f, -100f, 0f, 0.03f, 0.05f, 0.95f), 1e-5f);
        }

        [Test]
        public void Difficulties_AreOrdered()
        {
            var s = Settings();
            float easy = s.GetStatChance(StatType.Luck, EventCheckDifficulty.Easy, 3);
            float even = s.GetStatChance(StatType.Luck, EventCheckDifficulty.Even, 3);
            float hard = s.GetStatChance(StatType.Luck, EventCheckDifficulty.Hard, 3);
            Assert.Greater(easy, even);
            Assert.Greater(even, hard);
        }

        [Test]
        public void StatScale_UsesItsBaselineAndPerPoint()
        {
            var s = Settings();
            // Max Health 20 is the baseline: the base chance; 45 is 25 points above at 1% each
            Assert.AreEqual(0.5f, s.GetStatChance(StatType.MaxHealth, EventCheckDifficulty.Even, 20), 1e-5f);
            Assert.AreEqual(0.75f, s.GetStatChance(StatType.MaxHealth, EventCheckDifficulty.Even, 45), 1e-5f);
            // A stat without a scale uses the default per point from 0
            Assert.AreEqual(0.65f, s.GetStatChance(StatType.Armor, EventCheckDifficulty.Even, 5), 1e-5f);
        }

        [Test]
        public void Grace_CanLowerTheChance()
        {
            var s = Settings();
            Assert.AreEqual(0.7f, s.GetGraceChance(EventCheckDifficulty.Even, 10), 1e-5f);
            Assert.AreEqual(0.3f, s.GetGraceChance(EventCheckDifficulty.Even, -10), 1e-5f);
        }

        [Test]
        public void Succeeds_WhenTheRollIsBelowTheChance()
        {
            Assert.IsTrue(EventCheckMath.Succeeds(0.5f, 0.49f));
            Assert.IsFalse(EventCheckMath.Succeeds(0.5f, 0.5f));
        }

        [TestCase("Warrior", StatType.MeleeDamage)]
        [TestCase("Archer", StatType.RangedDamage)]
        [TestCase("Augur", StatType.MagicDamage)]
        public void KeyStat_IsTheClassKeyStat(string className, StatType expected)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterClassData>($"Assets/Resources/CharacterData/{className}.asset");
            Assert.IsNotNull(data, $"Missing class data {className}");
            var check = new EventCheck { source = EventCheckSource.KeyStat, stat = StatType.Luck };
            Assert.AreEqual(expected, EventCheckRoller.ResolveStat(check, data.statPriorities));
        }

        [Test]
        public void StatSource_ReadsItsOwnStat()
        {
            var check = new EventCheck { source = EventCheckSource.Stat, stat = StatType.Armor };
            Assert.AreEqual(StatType.Armor, EventCheckRoller.ResolveStat(check, null));
        }

        [Test]
        public void SettingsAsset_IsSound()
        {
            var s = AssetDatabase.LoadAssetAtPath<EventCheckSettings>(SettingsPath);
            Assert.IsNotNull(s, $"Missing {SettingsPath}");
            Assert.Greater(s.easyChance, s.evenChance, "Easy must be likelier than Even");
            Assert.Greater(s.evenChance, s.hardChance, "Even must be likelier than Hard");
            Assert.Less(s.minChance, s.maxChance);
            Assert.Greater(s.minChance, 0f, "Every check keeps some chance");
            Assert.Less(s.maxChance, 1f, "No check is certain");
            Assert.Greater(s.defaultChancePerPoint, 0f);
            Assert.Greater(s.graceChancePerPoint, 0f);
            foreach (var scale in s.statScales) Assert.Greater(scale.chancePerPoint, 0f, $"{scale.stat} scale");
        }
    }
}
