using Game.Combat;
using Game.Effects.Abilities;
using NUnit.Framework;
using System.Linq;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The Whirlwind rework (tester feedback, 2026-10-26): a 2 s spin that hits each enemy on contact, then
    /// every Tick Interval (shorter with each tier), with a longer projectile shield (3 s or more on the
    /// upgraded tiers) and no invulnerability.
    /// </summary>
    public class WhirlwindTests
    {
        private const string AbilityPath = "Assets/Resources/Effects/EffectBehaviors/WhirlwindAbility.asset";

        // --- Per-enemy hit timing --------------------------------------------------------------------------

        [Test]
        public void Ticker_HitsOnContact_ThenOncePerInterval()
        {
            var ticker = new HitTicker { Interval = 0.5f };
            var enemy = new object();

            Assert.IsTrue(ticker.TryHit(enemy, 0f), "Hit on contact");
            Assert.IsFalse(ticker.TryHit(enemy, 0.2f), "Not again within the interval");
            Assert.IsFalse(ticker.TryHit(enemy, 0.49f));
            Assert.IsTrue(ticker.TryHit(enemy, 0.5f), "Again once the interval passed");
            Assert.IsFalse(ticker.TryHit(enemy, 0.7f));
            Assert.IsTrue(ticker.TryHit(enemy, 1.0f));
        }

        [Test]
        public void Ticker_EachEnemyHasItsOwnTimer()
        {
            var ticker = new HitTicker { Interval = 0.5f };
            var first = new object();
            var second = new object();

            Assert.IsTrue(ticker.TryHit(first, 0f));
            Assert.IsTrue(ticker.TryHit(second, 0.3f), "A second enemy entering later is hit on its own contact");
            Assert.IsTrue(ticker.TryHit(first, 0.5f));
            Assert.IsFalse(ticker.TryHit(second, 0.5f), "...and keeps its own interval");
            Assert.IsTrue(ticker.TryHit(second, 0.8f));
        }

        [Test]
        public void Ticker_WithoutInterval_HitsOncePerActivation()
        {
            var ticker = new HitTicker { Interval = 0f };
            var enemy = new object();

            Assert.IsTrue(ticker.TryHit(enemy, 0f));
            Assert.IsFalse(ticker.TryHit(enemy, 5f), "No ticking without an interval");

            ticker.Clear();
            Assert.IsTrue(ticker.TryHit(enemy, 6f), "A new activation (hitbox switched on again) starts fresh");
        }

        [Test]
        public void Spin_LastsUntilItsEndTime()
        {
            Assert.IsTrue(SwordWeapon.IsSpinOngoing(1.9f, 2f));
            Assert.IsFalse(SwordWeapon.IsSpinOngoing(2f, 2f));
            Assert.IsFalse(SwordWeapon.IsSpinOngoing(0f, 0f), "Spin Duration 0: the spin lasts its animation once");
        }

        // --- The real tiers ---------------------------------------------------------------------------------

        private static WhirlwindAbility.Values[] Tiers()
        {
            var ability = AssetDatabase.LoadAssetAtPath<WhirlwindAbility>(AbilityPath);
            Assert.IsNotNull(ability, $"Missing {AbilityPath}");
            return ability.Tiers.ToArray();
        }

        // The numbers are the designer's (sheet tab Ability_Whirlwind); these check the rules, not the values
        [Test]
        public void Tiers_SpinTwoSecondsAndTickFasterWithEachUpgrade()
        {
            var tiers = Tiers();
            Assert.AreEqual(4, tiers.Length);
            Assert.IsTrue(tiers.All(t => t.tickInterval > 0f), "Every tier ticks (0 would hit once per spin)");
            for (int i = 1; i < tiers.Length; i++)
            {
                Assert.Less(tiers[i].tickInterval, tiers[i - 1].tickInterval, $"Tier {i} must tick faster than tier {i - 1}");
            }
            Assert.IsTrue(tiers.All(t => t.spinDuration >= 2f), "The new animation lasts 2 s");
        }

        [Test]
        public void Tiers_UpgradedShieldLastsAtLeastThreeSeconds()
        {
            var tiers = Tiers();
            Assert.AreEqual(0f, tiers[0].shieldDuration, "The shield is an upgrade-only mechanic");
            Assert.IsTrue(tiers.Skip(1).All(t => t.shieldDuration >= 3f), "Testers found the shield too short");
        }

        [Test]
        public void Tooltip_SaysHowOftenAndHowLong()
        {
            var ability = AssetDatabase.LoadAssetAtPath<WhirlwindAbility>(AbilityPath);
            var tier = ability.Tiers[0];
            string line = ability.DescribeTier(0);
            StringAssert.Contains(
                $"every {AbilityTiers.Number(tier.tickInterval)}s for {AbilityTiers.Number(tier.spinDuration)}s", line);
        }
    }
}
