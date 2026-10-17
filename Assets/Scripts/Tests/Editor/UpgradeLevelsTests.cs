using Game.Progression;
using NUnit.Framework;

namespace Game.Tests
{
    public class UpgradeLevelsTests
    {
        // Level 1 -> 3 in one wave: the first pick is for level 2, the second for level 3.
        [TestCase(3, 2, 2)]
        [TestCase(2, 1, 2)]
        [TestCase(10, 4, 7)]
        [TestCase(5, 0, 6)]   // nothing gained: no groups, but never below the next level
        [TestCase(5, -1, 6)]  // a negative count is treated as 0
        public void FirstLevel_IsTheLevelAfterTheWaveStarted(int currentLevel, int levelsGained, int expected)
        {
            Assert.AreEqual(expected, UpgradeLevels.FirstLevel(currentLevel, levelsGained));
        }

        [TestCase(2, 0, 2)]
        [TestCase(2, 1, 3)]
        [TestCase(7, 3, 10)]
        [TestCase(4, -1, 4)]
        public void LevelForChoice_CountsUpFromTheFirst(int firstLevel, int choiceIndex, int expected)
        {
            Assert.AreEqual(expected, UpgradeLevels.LevelForChoice(firstLevel, choiceIndex));
        }

        [Test]
        public void LastChoice_IsTheCurrentLevel()
        {
            int currentLevel = 9, gained = 3;
            int first = UpgradeLevels.FirstLevel(currentLevel, gained);
            Assert.AreEqual(currentLevel, UpgradeLevels.LevelForChoice(first, gained - 1));
        }
    }
}
