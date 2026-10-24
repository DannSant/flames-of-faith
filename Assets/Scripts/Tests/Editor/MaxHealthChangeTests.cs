using Game.Combat;
using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>
    /// Current health moves by the same amount as max health when max changes during play
    /// (a level-up, an item). It used to scale by the ratio and round down: +5 at 5/15 gave 6/20.
    /// </summary>
    public class MaxHealthChangeTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(5f, 15f, 20f, 10f)]   // the play-test case
        [TestCase(10f, 20f, 25f, 15f)]  // the second play-test case (was 12/25)
        [TestCase(15f, 15f, 20f, 20f)]  // full stays full
        [TestCase(4.3f, 15f, 20f, 9.3f)] // fractional health isn't rounded away
        public void Increase_AddsTheGainedMax(float current, float oldMax, float newMax, float expected)
        {
            Assert.AreEqual(expected, PlayerHealth.CalculateHealthAfterMaxChange(current, oldMax, newMax), Tolerance);
        }

        [Test]
        public void Decrease_KeepsTheMissingHealth()
        {
            Assert.AreEqual(12f, PlayerHealth.CalculateHealthAfterMaxChange(17f, 25f, 20f), Tolerance);
        }

        [Test]
        public void Decrease_NeverKills()
        {
            Assert.AreEqual(1f, PlayerHealth.CalculateHealthAfterMaxChange(3f, 20f, 15f), Tolerance);
        }

        [Test]
        public void Dead_StaysDead()
        {
            Assert.AreEqual(0f, PlayerHealth.CalculateHealthAfterMaxChange(0f, 15f, 20f), Tolerance);
        }

        [Test]
        public void LosingThenRegainingMax_GivesBackTheSameHealth()
        {
            float afterLoss = PlayerHealth.CalculateHealthAfterMaxChange(10f, 25f, 20f);
            Assert.AreEqual(10f, PlayerHealth.CalculateHealthAfterMaxChange(afterLoss, 20f, 25f), Tolerance);
        }
    }
}
