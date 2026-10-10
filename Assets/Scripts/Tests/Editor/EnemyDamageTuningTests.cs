using Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Guard tests against the real tuning: Resources/Combat/EnemyDamageSettings and every EnemyData.
    /// They don't pin exact numbers, only catch values that can't be intended (unsorted rows,
    /// a higher tier that hits softer, an enemy that one-shots or can't kill the expected player).
    /// </summary>
    public class EnemyDamageTuningTests
    {
        private const string SettingsPath = "Combat/EnemyDamageSettings";
        private const string EnemiesPath = "Enemies";

        // Range of hits any enemy attack may take to kill a player with the expected stats.
        private const int MinHitsToKill = 1;
        private const int MaxHitsToKill = 60;
        private const int WavesPerLevelToCheck = 3;

        private static EnemyDamageSettings LoadSettings()
        {
            var settings = Resources.Load<EnemyDamageSettings>(SettingsPath);
            Assert.IsNotNull(settings, $"Missing Resources/{SettingsPath}.asset");
            return settings;
        }

        [Test]
        public void ExpectedCurve_IsSorted_AndNeverGetsWeaker()
        {
            var curve = LoadSettings().ExpectedCurve;
            Assert.IsNotEmpty(curve);

            for (int i = 1; i < curve.Count; i++)
            {
                Assert.Greater(curve[i].levelsBeaten, curve[i - 1].levelsBeaten, $"Row {i} is not sorted by levels beaten");
                Assert.GreaterOrEqual(curve[i].maxHealth, curve[i - 1].maxHealth, $"Expected max health drops at row {i}");
                Assert.GreaterOrEqual(curve[i].armor, curve[i - 1].armor, $"Expected armor drops at row {i}");
            }
            Assert.Greater(curve[0].maxHealth, 0f);
        }

        [Test]
        public void TierHits_ArePositive_AndStrictlyDecreasing()
        {
            var tiers = LoadSettings().TierHitsToKill;
            Assert.IsNotEmpty(tiers);

            for (int i = 0; i < tiers.Count; i++)
            {
                Assert.Greater(tiers[i], 0f, $"Tier {i + 1} must take at least some hits");
                if (i > 0)
                {
                    Assert.Less(tiers[i], tiers[i - 1], $"Tier {i + 1} must kill faster than tier {i}");
                }
            }
        }

        [Test]
        public void EveryEnemyAttack_KillsTheExpectedPlayerInAReasonableNumberOfHits()
        {
            var settings = LoadSettings();
            var enemies = Resources.LoadAll<EnemyData>(EnemiesPath);
            Assert.IsNotEmpty(enemies);

            var curve = settings.ExpectedCurve;
            int lastLevel = Mathf.CeilToInt(curve[curve.Count - 1].levelsBeaten) + 1;

            foreach (var enemy in enemies)
            {
                foreach (EnemyDamageKind kind in System.Enum.GetValues(typeof(EnemyDamageKind)))
                {
                    if (enemy.GetDamageTier(kind) <= 0f) continue; // This enemy has no attack of this kind

                    for (int level = 0; level <= lastLevel; level++)
                    {
                        for (int wave = 1; wave <= WavesPerLevelToCheck; wave++)
                        {
                            float raw = EnemyDamageCalculator.Calculate(enemy, wave, level, kind, settings);
                            var expected = EnemyDamageCalculator.GetExpectedStats(
                                EnemyDamageCalculator.GetProgress(wave, level, settings), settings);
                            int hits = EnemyDamageCalculator.SimulateHitsToDie(expected.maxHealth, expected.armor, raw, settings);

                            Assert.That(hits, Is.InRange(MinHitsToKill, MaxHitsToKill),
                                $"{enemy.name} {kind} at {level} levels beaten, wave {wave}: {hits} hits ({raw:0.00} raw damage)");
                        }
                    }
                }
            }
        }

        [Test]
        public void EveryEnemy_HasNonNegativeTiers()
        {
            foreach (var enemy in Resources.LoadAll<EnemyData>(EnemiesPath))
            {
                Assert.GreaterOrEqual(enemy.contactDamageTier, 0f, $"{enemy.name} contact tier");
                Assert.GreaterOrEqual(enemy.projectileDamageTier, 0f, $"{enemy.name} projectile tier");
            }
        }
    }
}
