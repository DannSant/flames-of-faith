using Game.Combat;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>At high Attack Speed the swing's hitbox still lasts long enough for a physics step to see it.</summary>
    public class SwordHitboxTests
    {
        [TestCase(10f, 10f, 0.04f, false)]
        [TestCase(10.03f, 10f, 0.04f, false)]
        [TestCase(10.05f, 10f, 0.04f, true)]
        [TestCase(11f, 10f, 0.04f, true)]
        [TestCase(10f, 10f, 0f, true)]
        public void Hitbox_ClosesOnlyAfterTheMinimumTime(float now, float openTime, float minTime, bool expected)
        {
            Assert.AreEqual(expected, SwordWeapon.CanCloseSwingHitbox(now, openTime, minTime));
        }

        [Test]
        public void MinimumTime_OutlastsAPhysicsStep()
        {
            var settings = Resources.Load<PlayerStatSettings>("Progression/PlayerStatSettings");
            Assert.IsNotNull(settings);
            Assert.GreaterOrEqual(settings.MinMeleeHitboxTime, Time.fixedDeltaTime,
                "A shorter hitbox can fall between two physics steps and hit nothing");
        }
    }
}
