using Game.VFX;
using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>
    /// The Ley Charge strike bolt is drawn once and fades out (it used to redraw 6-7 times, which read
    /// as 6-7 strikes on the same enemy).
    /// </summary>
    public class ChainLightningBoltTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Fade_StartsFullAndEndsAtZero()
        {
            Assert.AreEqual(1f, ChainLightningBolt.GetFadeAlpha(0f, 0.2f), Tolerance);
            Assert.AreEqual(0.5f, ChainLightningBolt.GetFadeAlpha(0.1f, 0.2f), Tolerance);
            Assert.AreEqual(0f, ChainLightningBolt.GetFadeAlpha(0.2f, 0.2f), Tolerance);
        }

        [Test]
        public void Fade_StaysInRange()
        {
            Assert.AreEqual(0f, ChainLightningBolt.GetFadeAlpha(1f, 0.2f), Tolerance, "Past the lifetime");
            Assert.AreEqual(1f, ChainLightningBolt.GetFadeAlpha(-1f, 0.2f), Tolerance, "Before it starts");
            Assert.AreEqual(0f, ChainLightningBolt.GetFadeAlpha(0f, 0f), Tolerance, "No lifetime: invisible, no division by zero");
        }
    }
}
