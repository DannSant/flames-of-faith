using Game.UI;
using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>
    /// When Pause (Esc / Start) may open the pause menu (tester feedback, 2026-10-25): over the item bag or the
    /// level-up window, which pause the game themselves, the menu's Resume unpaused the game behind them and
    /// hid their Stats and Inventory panes.
    /// </summary>
    public class PauseMenuRuleTests
    {
        [Test]
        public void Opens_DuringNormalPlay()
        {
            Assert.IsTrue(PauseMenuController.CanOpenPauseMenu(false, false, false, true));
        }

        [Test]
        public void DoesNotOpen_WhileAnotherWindowPausedTheGame()
        {
            Assert.IsFalse(PauseMenuController.CanOpenPauseMenu(false, false, true, true),
                "The item bag / level-up window already paused the game");
        }

        [TestCase(true, false, false, true, TestName = "Dead")]
        [TestCase(false, true, false, true, TestName = "EndScreen")]
        [TestCase(false, false, false, false, TestName = "LevelDisallowsPause")]
        public void DoesNotOpen(bool dead, bool endscreen, bool paused, bool levelAllows)
        {
            Assert.IsFalse(PauseMenuController.CanOpenPauseMenu(dead, endscreen, paused, levelAllows));
        }
    }
}
