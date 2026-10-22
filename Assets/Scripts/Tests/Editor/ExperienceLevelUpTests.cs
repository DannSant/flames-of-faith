using Game.Progression;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// PlayerExperience.AddExperience: a gain worth several levels charges each level its own
    /// requirement (10, 12, 15... with the default curve), and the XP bar is told the requirement of
    /// the level it is now filling toward. Uses its own ExperienceSettings with the default curve, so
    /// tuning the real asset doesn't break these.
    /// </summary>
    public class ExperienceLevelUpTests
    {
        private GameObject go;
        private PlayerExperience experience;
        private ExperienceSettings settings;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("PlayerExperienceTest");
            experience = go.AddComponent<PlayerExperience>();
            settings = ScriptableObject.CreateInstance<ExperienceSettings>();
            experience.UseSettings(settings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Curve_IsTheDefault()
        {
            Assert.AreEqual(10, experience.GetXPRequired(1));
            Assert.AreEqual(12, experience.GetXPRequired(2));
            Assert.AreEqual(15, experience.GetXPRequired(3));
        }

        [Test]
        public void Curve_GrowsByTheGivenRate()
        {
            Assert.AreEqual(20, ExperienceSettings.CalculateXPRequired(1, 20f, 1.5f));
            Assert.AreEqual(30, ExperienceSettings.CalculateXPRequired(2, 20f, 1.5f));
            Assert.AreEqual(45, ExperienceSettings.CalculateXPRequired(3, 20f, 1.5f));
        }

        [Test]
        public void OneGainWorthTwoLevels_ChargesEachLevelItsOwnRequirement()
        {
            experience.AddExperience(10f + 12f + 5f);

            Assert.AreEqual(3, experience.GetCurrentLevel());
            Assert.AreEqual(5f, experience.GetCurrentXP(), 1e-4f, "Level 2 costs 12, not level 1's 10");
        }

        [Test]
        public void GainExactlyReachingTheNextLevel_LeavesNoXP()
        {
            experience.AddExperience(22f);

            Assert.AreEqual(3, experience.GetCurrentLevel());
            Assert.AreEqual(0f, experience.GetCurrentXP(), 1e-4f);
        }

        [Test]
        public void GainShortOfTheSecondLevel_StopsAtOneLevel()
        {
            experience.AddExperience(21f);

            Assert.AreEqual(2, experience.GetCurrentLevel());
            Assert.AreEqual(11f, experience.GetCurrentXP(), 1e-4f);
        }

        [Test]
        public void XPBar_FillsTowardTheCurrentLevelsRequirement()
        {
            var reported = new List<(float xp, int max)>();
            experience.OnPlayerExperienceGainEvent += (xp, max) => reported.Add((xp, max));

            experience.AddExperience(10f + 12f + 5f);

            var last = reported[reported.Count - 1];
            Assert.AreEqual(5f, last.xp, 1e-4f);
            Assert.AreEqual(experience.GetXPRequired(3), last.max, "The last update should use level 3's requirement");
        }
    }
}
