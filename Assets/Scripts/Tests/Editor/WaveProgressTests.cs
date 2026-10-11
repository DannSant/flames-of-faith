using Game.Waves;
using NUnit.Framework;

namespace Game.Tests
{
    public class WaveProgressTests
    {
        [TestCase(0, 1, true)]
        [TestCase(0, 3, false)]
        [TestCase(1, 3, false)]
        [TestCase(2, 3, true)]
        [TestCase(3, 3, false)]   // past the end (after the level finished)
        [TestCase(-1, 3, false)]  // before the first wave started
        [TestCase(0, 0, false)]   // a level with no waves
        [TestCase(-1, 0, false)]
        public void IsLastWave(int waveIndex, int waveCount, bool expected)
        {
            Assert.AreEqual(expected, WaveProgress.IsLastWave(waveIndex, waveCount));
        }
    }
}
