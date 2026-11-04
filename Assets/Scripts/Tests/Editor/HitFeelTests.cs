using Game.Feel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// Hit feel: the time scale rules (lowest wins, expiry, pause), the heavy hit and slider rules, the settings
    /// guard (one sane row per moment, everything short) and that only PauseManager writes Time.timeScale.
    /// </summary>
    public class HitFeelTests
    {
        private const string SettingsPath = "Assets/Resources/Feel/GameFeelSettings.asset";
        private const string TimeScaleOwner = "Assets/Scripts/GameSettings/PauseManager.cs";

        // --- Time dilation ---------------------------------------------------------------------------------

        [Test]
        public void TimeDilation_NoRequestIsFullSpeed()
        {
            Assert.AreEqual(1f, TimeDilationRules.CurrentScale(new List<TimeDilationRequest>(), 5f));
            Assert.AreEqual(1f, TimeDilationRules.CurrentScale(null, 5f));
        }

        [Test]
        public void TimeDilation_LowestActiveScaleWins()
        {
            var requests = new List<TimeDilationRequest>
            {
                new TimeDilationRequest(0.4f, 10f),
                new TimeDilationRequest(0f, 2f),
                new TimeDilationRequest(0.3f, 5f),
            };
            Assert.AreEqual(0f, TimeDilationRules.CurrentScale(requests, 1f), "The hit stop wins while it runs");
            Assert.AreEqual(0.3f, TimeDilationRules.CurrentScale(requests, 3f), 1e-5f);
            Assert.AreEqual(0.4f, TimeDilationRules.CurrentScale(requests, 6f), 1e-5f);
            Assert.AreEqual(1f, TimeDilationRules.CurrentScale(requests, 10f), "Ended at its end time");
        }

        [Test]
        public void TimeDilation_ExpiredRequestsAreRemoved()
        {
            var requests = new List<TimeDilationRequest> { new TimeDilationRequest(0.5f, 1f), new TimeDilationRequest(0.5f, 3f) };
            Assert.AreEqual(1, TimeDilationRules.RemoveExpired(requests, 2f));
            Assert.AreEqual(1, requests.Count);
        }

        [Test]
        public void TimeScale_PauseIsZeroAndResumeGivesWhatStillRuns()
        {
            var requests = new List<TimeDilationRequest> { new TimeDilationRequest(0.3f, 4f) };
            // Paused at t=1 while the slow motion runs
            Assert.AreEqual(0f, TimeDilationRules.TimeScale(true, TimeDilationRules.CurrentScale(requests, 1f)));
            // Resumed at t=2: still slowed
            Assert.AreEqual(0.3f, TimeDilationRules.TimeScale(false, TimeDilationRules.CurrentScale(requests, 2f)), 1e-5f);
            // Resumed after it ended during the pause: full speed
            Assert.AreEqual(1f, TimeDilationRules.TimeScale(false, TimeDilationRules.CurrentScale(requests, 6f)));
        }

        [Test]
        public void TimeDilation_ClearEndsEverything()
        {
            TimeDilation.Request(0.2f, 100f);
            Assert.Less(TimeDilation.CurrentScale, 1f);
            TimeDilation.Clear();
            Assert.AreEqual(1f, TimeDilation.CurrentScale);
        }

        // --- Rules -----------------------------------------------------------------------------------------

        [Test]
        public void HeavyHit_FromTheFractionOfMaxHealth()
        {
            Assert.IsTrue(GameFeelRules.IsHeavyHit(4f, 20f, 0.2f));
            Assert.IsFalse(GameFeelRules.IsHeavyHit(3.9f, 20f, 0.2f));
            Assert.IsFalse(GameFeelRules.IsHeavyHit(5f, 0f, 0.2f), "No max health, no heavy hit");
            Assert.IsFalse(GameFeelRules.IsHeavyHit(0f, 20f, 0f), "A hit of 0 is never heavy");
        }

        [Test]
        public void Slider_ScalesAndZeroSendsNothing()
        {
            Assert.AreEqual(0.25f, GameFeelRules.Scaled(0.5f, 0.5f), 1e-5f);
            Assert.AreEqual(0f, GameFeelRules.Scaled(0.5f, 0f));
            Assert.AreEqual(0.5f, GameFeelRules.Scaled(0.5f, 3f), 1e-5f, "The slider is clamped to 1");
        }

        [Test]
        public void Cooldown_BlocksOnlyInsideIt()
        {
            Assert.IsFalse(GameFeelRules.IsOffCooldown(1f, 1.4f, 0.5f));
            Assert.IsTrue(GameFeelRules.IsOffCooldown(1f, 1.5f, 0.5f));
        }

        // --- Settings guard --------------------------------------------------------------------------------

        private static GameFeelSettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameFeelSettings>(SettingsPath);
            Assert.IsNotNull(settings, $"Missing {SettingsPath}");
            return settings;
        }

        [Test]
        public void Settings_EveryMomentHasOneRow()
        {
            var settings = Settings();
            foreach (FeelMoment moment in Enum.GetValues(typeof(FeelMoment)))
            {
                Assert.AreEqual(1, settings.Moments.Count(r => r != null && r.moment == moment), $"{moment} needs exactly one row");
            }
        }

        [Test]
        public void Settings_ValuesAreSaneAndShort()
        {
            var settings = Settings();
            Assert.That(settings.HeavyHitFraction, Is.InRange(0.05f, 1f));
            Assert.That(settings.HitStopScale, Is.InRange(0f, 0.5f));
            foreach (var row in settings.Moments)
            {
                Assert.That(row.shakeForce, Is.InRange(0f, 2f), $"{row.moment} shake");
                Assert.That(row.shakeDuration, Is.InRange(0f, 1.5f), $"{row.moment} shake duration");
                Assert.That(row.hitStop, Is.InRange(0f, 0.2f), $"{row.moment} hit stop: a long freeze reads as a hitch");
                Assert.That(row.slowMotionScale, Is.InRange(0.1f, 1f), $"{row.moment} slow motion scale");
                Assert.That(row.slowMotionDuration, Is.InRange(0f, 1.5f), $"{row.moment} slow motion duration");
                Assert.That(row.rumbleLow, Is.InRange(0f, 1f), $"{row.moment} rumble low");
                Assert.That(row.rumbleHigh, Is.InRange(0f, 1f), $"{row.moment} rumble high");
                Assert.That(row.rumbleDuration, Is.InRange(0f, 1f), $"{row.moment} rumble duration");
                Assert.That(row.cooldown, Is.InRange(0.1f, 10f), $"{row.moment} cooldown: without one a burst can chain");
            }
        }

        // --- Time scale owner ------------------------------------------------------------------------------

        [Test]
        public void TimeScale_IsWrittenOnlyByPauseManager()
        {
            var write = new Regex(@"Time\.timeScale\s*=[^=]");
            foreach (string path in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                string normalized = path.Replace('\\', '/');
                if (normalized == TimeScaleOwner || normalized.Contains("/Tests/")) continue;
                Assert.IsFalse(write.IsMatch(File.ReadAllText(path)), $"{normalized} writes Time.timeScale: go through PauseManager / TimeDilation");
            }
            Assert.IsTrue(write.IsMatch(File.ReadAllText(TimeScaleOwner)), "PauseManager no longer writes Time.timeScale");
        }
    }
}
