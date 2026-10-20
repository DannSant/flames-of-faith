using Game.Common;
using Game.UI;
using NUnit.Framework;
using System.Collections.Generic;

namespace Game.Tests
{
    /// <summary>
    /// HUD cooldown sweeps: the timers send a last "0 left" when a cooldown ends, so the sweep clears
    /// completely instead of freezing on the last frame's thin sliver; and the fill math is safe.
    /// </summary>
    public class CooldownSweepTests
    {
        private readonly List<(float timeLeft, float duration)> reports = new List<(float, float)>();
        private void Report(float timeLeft, float duration) => reports.Add((timeLeft, duration));

        [SetUp]
        public void SetUp() => reports.Clear();

        [Test]
        public void Reporter_ReportsEveryTickWhileRunning()
        {
            var reporter = new CooldownProgressReporter();
            reporter.Tick(true, 1.5f, 2f, Report);
            reporter.Tick(true, 0.02f, 2f, Report);

            CollectionAssert.AreEqual(new[] { (1.5f, 2f), (0.02f, 2f) }, reports);
        }

        [Test]
        public void Reporter_SendsAFinalZeroWhenTheCooldownEnds()
        {
            var reporter = new CooldownProgressReporter();
            reporter.Tick(true, 0.02f, 2f, Report);
            reporter.Tick(false, 0f, 0f, Report);

            Assert.AreEqual((0f, 2f), reports[reports.Count - 1], "Last report: nothing left, with the cooldown's own duration");
        }

        [Test]
        public void Reporter_SendsTheFinalZeroOnlyOnce()
        {
            var reporter = new CooldownProgressReporter();
            reporter.Tick(true, 0.5f, 2f, Report);
            reporter.Tick(false, 0f, 2f, Report);
            reporter.Tick(false, 0f, 2f, Report);
            reporter.Tick(false, 0f, 2f, Report);

            Assert.AreEqual(2, reports.Count);
        }

        [Test]
        public void Reporter_IsSilentBeforeAnyCooldown()
        {
            var reporter = new CooldownProgressReporter();
            reporter.Tick(false, 0f, 2f, Report);

            Assert.IsEmpty(reports);
        }

        [Test]
        public void Reporter_WorksAgainForTheNextCooldown()
        {
            var reporter = new CooldownProgressReporter();
            reporter.Tick(true, 0.1f, 2f, Report);
            reporter.Tick(false, 0f, 2f, Report);
            reporter.Tick(true, 1f, 3f, Report);
            reporter.Tick(false, 0f, 3f, Report);

            Assert.AreEqual((0f, 3f), reports[reports.Count - 1]);
            Assert.AreEqual(4, reports.Count);
        }

        [TestCase(2f, 2f, 1f)]
        [TestCase(1f, 2f, 0.5f)]
        [TestCase(0f, 2f, 0f)]
        [TestCase(-0.01f, 2f, 0f)]
        [TestCase(3f, 2f, 1f)]
        [TestCase(0f, 0f, 0f)]
        [TestCase(1f, 0f, 0f)]
        public void Fill_IsTheShareOfTheCooldownLeft(float timeLeft, float duration, float expected)
        {
            Assert.AreEqual(expected, IconCooldownDisplay.GetCooldownFill(timeLeft, duration), 1e-5f);
        }
    }
}
