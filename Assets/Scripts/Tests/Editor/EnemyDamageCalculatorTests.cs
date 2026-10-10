using System.Collections.Generic;
using Game.Combat;
using Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Formula tests: each builds its own EnemyDamageSettings, so retuning the real asset never
    /// breaks them. Run from Window > General > Test Runner > EditMode.
    /// </summary>
    public class EnemyDamageCalculatorTests
    {
        private const float Tolerance = 0.001f;

        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in created)
            {
                Object.DestroyImmediate(obj);
            }
            created.Clear();
        }

        private EnemyDamageSettings MakeSettings(IEnumerable<ExpectedPlayerStats> curve, IEnumerable<float> tiers,
            float waveStep = 0.25f)
        {
            var settings = ScriptableObject.CreateInstance<EnemyDamageSettings>();
            settings.Configure(curve, waveStep, tiers, armorConstant: 25f, minPercent: 0.15f, minimumDamage: 0.1f);
            created.Add(settings);
            return settings;
        }

        private EnemyDamageSettings MakeFlatSettings(float maxHealth, float armor, params float[] tiers)
        {
            return MakeSettings(new[] { new ExpectedPlayerStats(0, maxHealth, armor) }, tiers);
        }

        // --- Scenarios ---------------------------------------------------------------------

        [Test]
        public void Tier1_ExpectedPlayer50Hp0Armor_Takes1Point67PerHit_AndDiesIn30Hits()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f, 25f);

            float raw = EnemyDamageCalculator.CalculateForTier(1f, 0f, settings);

            Assert.AreEqual(50f / 30f, raw, Tolerance);
            Assert.AreEqual(30, EnemyDamageCalculator.SimulateHitsToDie(50f, 0f, raw, settings));
        }

        [Test]
        public void Tier2_ExpectedPlayer60Hp2Armor_Takes2Point4PerHitAfterArmor_AndDiesIn25Hits()
        {
            var settings = MakeFlatSettings(60f, 2f, 30f, 25f);

            float raw = EnemyDamageCalculator.CalculateForTier(2f, 0f, settings);
            float afterArmor = ArmorMitigation.Apply(raw, 2f, 25f, 0.15f);

            Assert.AreEqual(2.4f, afterArmor, Tolerance);
            Assert.AreEqual(25, EnemyDamageCalculator.SimulateHitsToDie(60f, 2f, raw, settings));
        }

        [Test]
        public void PlayerAboveExpectedStats_SurvivesMoreHits_BelowExpected_SurvivesFewer()
        {
            var settings = MakeFlatSettings(60f, 2f, 30f, 25f);
            float raw = EnemyDamageCalculator.CalculateForTier(2f, 0f, settings);

            Assert.Greater(EnemyDamageCalculator.SimulateHitsToDie(60f, 4f, raw, settings), 25, "more armor than expected");
            Assert.Greater(EnemyDamageCalculator.SimulateHitsToDie(80f, 2f, raw, settings), 25, "more health than expected");
            Assert.Less(EnemyDamageCalculator.SimulateHitsToDie(60f, 0f, raw, settings), 25, "less armor than expected");
            Assert.Less(EnemyDamageCalculator.SimulateHitsToDie(40f, 2f, raw, settings), 25, "less health than expected");
        }

        // --- Tiers -------------------------------------------------------------------------

        [Test]
        public void FractionalTier_InterpolatesHitsBetweenNeighbours()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f, 20f);

            Assert.AreEqual(25f, EnemyDamageCalculator.GetHitsToKill(1.5f, settings), Tolerance);
        }

        [Test]
        public void TierBelowOne_ScalesTierOneDown()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f, 20f);

            Assert.AreEqual(60f, EnemyDamageCalculator.GetHitsToKill(0.5f, settings), Tolerance);
        }

        [Test]
        public void TierAboveTable_IsClampedToLastEntry()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f, 20f);

            Assert.AreEqual(20f, EnemyDamageCalculator.GetHitsToKill(7f, settings), Tolerance);
        }

        [Test]
        public void TierZero_DealsNoDamage()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f);

            Assert.AreEqual(0f, EnemyDamageCalculator.CalculateForTier(0f, 0f, settings));
        }

        [Test]
        public void HigherTier_DealsMoreDamage()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f, 25f, 15f);

            float tier1 = EnemyDamageCalculator.CalculateForTier(1f, 0f, settings);
            float tier2 = EnemyDamageCalculator.CalculateForTier(2f, 0f, settings);
            float tier3 = EnemyDamageCalculator.CalculateForTier(3f, 0f, settings);

            Assert.Less(tier1, tier2);
            Assert.Less(tier2, tier3);
        }

        // --- Progress and the expected-player curve ---------------------------------------

        [Test]
        public void Progress_AddsAFractionPerWave()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f);

            Assert.AreEqual(0f, EnemyDamageCalculator.GetProgress(1, 0, settings), Tolerance);
            Assert.AreEqual(2.5f, EnemyDamageCalculator.GetProgress(3, 2, settings), Tolerance);
        }

        [Test]
        public void ExpectedStats_InterpolateBetweenRows_AndClampAtTheEnds()
        {
            var settings = MakeSettings(new[]
            {
                new ExpectedPlayerStats(0, 20, 0),
                new ExpectedPlayerStats(4, 40, 4),
            }, new[] { 10f });

            var middle = EnemyDamageCalculator.GetExpectedStats(2f, settings);
            Assert.AreEqual(30f, middle.maxHealth, Tolerance);
            Assert.AreEqual(2f, middle.armor, Tolerance);

            Assert.AreEqual(20f, EnemyDamageCalculator.GetExpectedStats(-1f, settings).maxHealth, Tolerance);
            Assert.AreEqual(40f, EnemyDamageCalculator.GetExpectedStats(10f, settings).maxHealth, Tolerance);
        }

        [Test]
        public void SameTier_HitsHarderLaterInTheRun_ButAlwaysTakesTheSameHitsForTheExpectedPlayer()
        {
            var settings = MakeSettings(new[]
            {
                new ExpectedPlayerStats(0, 25, 0),
                new ExpectedPlayerStats(6, 55, 4),
            }, new[] { 10f });

            float early = EnemyDamageCalculator.CalculateForTier(1f, 0f, settings);
            float late = EnemyDamageCalculator.CalculateForTier(1f, 6f, settings);

            Assert.Greater(late, early);
            Assert.AreEqual(10, EnemyDamageCalculator.SimulateHitsToDie(25f, 0f, early, settings));
            Assert.AreEqual(10, EnemyDamageCalculator.SimulateHitsToDie(55f, 4f, late, settings));
        }

        // --- EnemyData ---------------------------------------------------------------------

        [Test]
        public void Calculate_UsesTheTierOfTheAttackKind()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f, 25f);
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            created.Add(enemy);
            enemy.contactDamageTier = 1f;
            enemy.projectileDamageTier = 2f;

            Assert.AreEqual(50f / 30f, EnemyDamageCalculator.Calculate(enemy, 1, 0, EnemyDamageKind.Contact, settings), Tolerance);
            Assert.AreEqual(50f / 25f, EnemyDamageCalculator.Calculate(enemy, 1, 0, EnemyDamageKind.Projectile, settings), Tolerance);
        }

        // --- Armor -------------------------------------------------------------------------

        [Test]
        public void Armor_MatchesTheProportionalFormula()
        {
            Assert.AreEqual(1f, ArmorMitigation.Multiplier(0f, 25f, 0.15f), Tolerance);
            Assert.AreEqual(1f - 10f / 35f, ArmorMitigation.Multiplier(10f, 25f, 0.15f), Tolerance);
            Assert.AreEqual(0.5f, ArmorMitigation.Multiplier(25f, 25f, 0.15f), Tolerance);
        }

        [Test]
        public void Armor_NeverGoesBelowMinDamagePercent()
        {
            Assert.AreEqual(0.15f, ArmorMitigation.Multiplier(1000f, 25f, 0.15f), Tolerance);
        }

        [Test]
        public void NegativeArmor_IncreasesDamage_ButNeverDoublesIt()
        {
            Assert.AreEqual(2f - 25f / 30f, ArmorMitigation.Multiplier(-5f, 25f, 0.15f), Tolerance); // +16.7%
            Assert.AreEqual(1.5f, ArmorMitigation.Multiplier(-25f, 25f, 0.15f), Tolerance);
            Assert.Less(ArmorMitigation.Multiplier(-1000f, 25f, 0.15f), 2f);
            Assert.Greater(ArmorMitigation.Multiplier(-1f, 25f, 0.15f), 1f);
        }

        [Test]
        public void PlayerWithNegativeArmor_DiesInFewerHitsThanExpected()
        {
            var settings = MakeFlatSettings(50f, 0f, 30f);
            float raw = EnemyDamageCalculator.CalculateForTier(1f, 0f, settings);

            Assert.Less(EnemyDamageCalculator.SimulateHitsToDie(50f, -5f, raw, settings), 30);
        }
    }
}
