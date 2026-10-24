using Game.Combat;
using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>
    /// Frost's extra damage taken: a percentage of the hit, at least 1. It used to subtract the hit's damage a second
    /// time, so a Frosted enemy took double damage whatever the debuff's strength.
    /// </summary>
    public class FrostDamageTests
    {
        [Test]
        public void ExtraDamage_IsThePercentageOfTheHit()
        {
            Assert.AreEqual(5f, EnemyHealth.CalculateExtraDamage(50f, 0.1f), 0.0001f);
            Assert.AreEqual(20f, EnemyHealth.CalculateExtraDamage(40f, 0.5f), 0.0001f);
        }

        [Test]
        public void ExtraDamage_IsAtLeastOne()
        {
            Assert.AreEqual(1f, EnemyHealth.CalculateExtraDamage(4f, 0.1f), 0.0001f);
        }

        [Test]
        public void ExtraDamage_IsNotTheWholeHitAgain()
        {
            Assert.Less(EnemyHealth.CalculateExtraDamage(50f, 0.1f), 50f);
        }
    }
}
