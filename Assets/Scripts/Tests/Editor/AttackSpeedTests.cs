using Game.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>The Attack Speed caps and the one cooldown formula behind the weapons and the tooltip.</summary>
    public class AttackSpeedTests
    {
        private const float Tolerance = 0.0001f;

        private PlayerStatSettings settings;

        [SetUp]
        public void SetUp() => settings = ScriptableObject.CreateInstance<PlayerStatSettings>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        [TestCase(0, 0f)]
        [TestCase(20, 20f)]
        [TestCase(30, 30f)]
        [TestCase(31, 30.5f)]
        [TestCase(40, 35f)]
        [TestCase(60, 45f)]
        [TestCase(61, 45f)]
        [TestCase(999, 45f)]
        public void EffectivePoints_FollowTheCaps(int points, float expected)
        {
            Assert.AreEqual(expected, StatsCalculations.CalculateEffectiveAttackSpeed(points, 30, 60, 0.5f), Tolerance);
        }

        [TestCase(-1)]
        [TestCase(-50)]
        public void EffectivePoints_NegativeOnesPassThrough(int points)
        {
            Assert.AreEqual(points, StatsCalculations.CalculateEffectiveAttackSpeed(points, 30, 60, 0.5f), Tolerance);
        }

        [Test]
        public void EffectivePoints_AHardCapBelowTheSoftCapStopsAtTheSoftCap()
        {
            Assert.AreEqual(30f, StatsCalculations.CalculateEffectiveAttackSpeed(50, 30, 10, 0.5f), Tolerance);
        }

        [Test]
        public void EffectivePoints_NeverGoDownAsPointsGoUp()
        {
            float previous = float.NegativeInfinity;
            for (int points = -20; points <= 120; points++)
            {
                float effective = settings.GetEffectiveAttackSpeed(points);
                Assert.GreaterOrEqual(effective, previous, $"at {points}");
                previous = effective;
            }
        }

        [Test]
        public void Delay_ChangesNothingBelowTheSoftCap()
        {
            for (int points = -20; points <= 30; points++)
            {
                Assert.AreEqual(StatsCalculations.CalculateAttackDelay(points, 1.5f, 0.075f),
                    settings.GetAttackDelay(points, 1.5f, 0.075f), Tolerance, $"at {points}");
            }
        }

        [Test]
        public void Delay_AtTheHardCap_IsTheProposedSpeedUp()
        {
            Assert.AreEqual(1.5f / 4.375f, settings.GetAttackDelay(60, 1.5f, 0.075f), Tolerance);
            Assert.AreEqual(1.5f / 4.375f, settings.GetAttackDelay(200, 1.5f, 0.075f), Tolerance, "no faster past it");
        }
    }
}
