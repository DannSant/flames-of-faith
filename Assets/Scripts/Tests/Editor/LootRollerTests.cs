using Game.Effects;
using Game.Loot;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    /// <summary>
    /// The item roll every source uses (enemy pickup, shop, treasure, event): who is eligible (source flag, min levels
    /// beaten, unlocked, not starting-only), the quality odds with Luck, the pick inside a quality, and the shop's size.
    /// Negative Luck (Cursed Coin, Faikan Boots) used to zero the higher qualities and empty the shop, which froze the
    /// level load; the weights must stay positive.
    /// </summary>
    public class LootRollerTests
    {
        private readonly List<Effect> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var effect in created) Object.DestroyImmediate(effect);
            created.Clear();
        }

        private ItemDropEntry Entry(EffectQuality quality, int minLevels = 0, float weight = 1f, bool unlocked = true, bool startingOnly = false)
        {
            var effect = ScriptableObject.CreateInstance<Effect>();
            effect.name = $"{quality} {created.Count}";
            effect.Quality = quality;
            effect.UnlockedByDefault = unlocked;
            effect.StartingOnly = startingOnly;
            created.Add(effect);
            return new ItemDropEntry { item = effect, enemyPickup = true, shop = true, treasure = true, events = true, minLevelsBeaten = minLevels, weight = weight };
        }

        private static LootSourceSettings Equal(float luckScale = 2f) => new()
        {
            commonWeight = 25f, uncommonWeight = 25f, rareWeight = 25f, heroicWeight = 25f, epicWeight = 25f, legendaryWeight = 25f,
            luckScalePerStep = luckScale,
        };

        private static Func<double> Seeded(int seed)
        {
            var rng = new System.Random(seed);
            return rng.NextDouble;
        }

        private static List<ItemDropEntry> Eligible(IEnumerable<ItemDropEntry> entries, LootSource source = LootSource.Shop, int levels = 0) =>
            LootRoller.GetEligible(entries, source, levels, e => e.UnlockedByDefault);

        // --- Eligibility --------------------------------------------------------------------------------------------

        [Test]
        public void MinLevelsBeaten_HoldsAnItemBackUntilReached()
        {
            var early = Entry(EffectQuality.Common);
            var late = Entry(EffectQuality.Heroic, minLevels: 3);
            var entries = new[] { early, late };

            CollectionAssert.AreEquivalent(new[] { early }, Eligible(entries, levels: 0));
            CollectionAssert.AreEquivalent(new[] { early }, Eligible(entries, levels: 2));
            CollectionAssert.AreEquivalent(new[] { early, late }, Eligible(entries, levels: 3));
        }

        [Test]
        public void OnlyItemsFlaggedForTheSource_AreEligible()
        {
            var shopOnly = Entry(EffectQuality.Common);
            shopOnly.enemyPickup = shopOnly.treasure = shopOnly.events = false;

            Assert.AreEqual(1, Eligible(new[] { shopOnly }, LootSource.Shop).Count);
            Assert.IsEmpty(Eligible(new[] { shopOnly }, LootSource.EnemyPickup));
            Assert.IsEmpty(Eligible(new[] { shopOnly }, LootSource.Treasure));
            Assert.IsEmpty(Eligible(new[] { shopOnly }, LootSource.Event));
        }

        [Test]
        public void LockedStartingOnlyAndZeroWeightItems_NeverRoll()
        {
            var entries = new[]
            {
                Entry(EffectQuality.Common, unlocked: false),
                Entry(EffectQuality.Common, startingOnly: true),
                Entry(EffectQuality.Common, weight: 0f),
            };
            Assert.IsEmpty(Eligible(entries));
            Assert.IsNull(LootRoller.RollOne(Eligible(entries), Equal(), 0, Seeded(1)));
        }

        // --- Quality odds ------------------------------------------------------------------------------------------

        [Test]
        public void Weights_StayPositive_ForAnyLuck([Values(-20, -5, -3, -2, -1, 0, 1, 5, 20)] int luck)
        {
            var weights = LootRoller.GetTierWeights(Equal(), luck);
            Assert.That(weights, Has.All.GreaterThan(0f), $"Luck {luck}: {string.Join(", ", weights)}");
        }

        [Test]
        public void NegativeLuck_MakesHigherQualitiesRarer([Values(-1, -2, -3, -10)] int luck)
        {
            var weights = LootRoller.GetTierWeights(Equal(), luck);
            for (int i = 1; i < weights.Length; i++) Assert.Less(weights[i], weights[i - 1], $"Luck {luck}, tier {i}");
        }

        [Test]
        public void PositiveLuck_MakesHigherQualitiesLikelier([Values(1, 2, 10)] int luck)
        {
            var weights = LootRoller.GetTierWeights(Equal(), luck);
            for (int i = 1; i < weights.Length; i++) Assert.Greater(weights[i], weights[i - 1], $"Luck {luck}, tier {i}");
        }

        [Test]
        public void AtLuckZero_TheOddsAreTheBaseWeights()
        {
            var settings = new LootSourceSettings { commonWeight = 40f, uncommonWeight = 30f, rareWeight = 20f, heroicWeight = 10f };
            var eligible = new List<ItemDropEntry>
            {
                Entry(EffectQuality.Common), Entry(EffectQuality.Uncommon), Entry(EffectQuality.Rare), Entry(EffectQuality.Heroic),
            };
            var odds = LootRoller.GetTierOdds(eligible, settings, 0);
            Assert.AreEqual(0.4f, odds[0], 0.0001f);
            Assert.AreEqual(0.3f, odds[1], 0.0001f);
            Assert.AreEqual(0.2f, odds[2], 0.0001f);
            Assert.AreEqual(0.1f, odds[3], 0.0001f);
        }

        [Test]
        public void ALuckScaleOfZero_IgnoresLuck()
        {
            CollectionAssert.AreEqual(LootRoller.GetTierWeights(Equal(0f), 0), LootRoller.GetTierWeights(Equal(0f), 15));
        }

        [Test]
        public void QualitiesWithNoEligibleItem_NeverRoll()
        {
            var eligible = new List<ItemDropEntry> { Entry(EffectQuality.Uncommon), Entry(EffectQuality.Heroic) };
            var random = Seeded(7);
            for (int i = 0; i < 300; i++)
            {
                var item = LootRoller.RollOne(eligible, Equal(), 5, random);
                Assert.That(item.Quality, Is.EqualTo(EffectQuality.Uncommon).Or.EqualTo(EffectQuality.Heroic));
            }
        }

        [Test]
        public void AQualityWithZeroBaseWeight_NeverRolls()
        {
            var settings = new LootSourceSettings { commonWeight = 0f, uncommonWeight = 0f, rareWeight = 40f, heroicWeight = 60f };
            var eligible = new List<ItemDropEntry> { Entry(EffectQuality.Common), Entry(EffectQuality.Rare) };
            var random = Seeded(3);
            for (int i = 0; i < 200; i++) Assert.AreEqual(EffectQuality.Rare, LootRoller.RollOne(eligible, settings, 10, random).Quality);
        }

        // --- Pick inside a quality ------------------------------------------------------------------------------------

        [Test]
        public void ItemWeights_SetTheOddsInsideAQuality()
        {
            var light = Entry(EffectQuality.Rare, weight: 1f);
            var heavy = Entry(EffectQuality.Rare, weight: 3f);
            var eligible = new List<ItemDropEntry> { light, heavy };
            var random = Seeded(11);

            int heavyCount = 0;
            const int rolls = 4000;
            for (int i = 0; i < rolls; i++)
            {
                if (LootRoller.RollOne(eligible, Equal(), 0, random) == heavy.item) heavyCount++;
            }
            Assert.AreEqual(0.75, heavyCount / (double)rolls, 0.03);
        }

        [Test]
        public void Pick_NeverLandsOnAZeroWeight()
        {
            var weights = new[] { 0f, 2f, 0f, 1f, 0f };
            Assert.AreEqual(1, LootRoller.Pick(weights, 0.0));
            Assert.AreEqual(3, LootRoller.Pick(weights, 0.99));
            Assert.AreEqual(3, LootRoller.Pick(weights, 1.0));
            Assert.AreEqual(-1, LootRoller.Pick(new[] { 0f, 0f }, 0.5));
        }

        // --- Shop -----------------------------------------------------------------------------------------------------

        [Test]
        public void UniqueRolls_NeverRepeatAndStopWhenThePoolRunsOut()
        {
            var eligible = new List<ItemDropEntry>
            {
                Entry(EffectQuality.Common), Entry(EffectQuality.Common), Entry(EffectQuality.Rare), Entry(EffectQuality.Heroic),
            };
            var items = LootRoller.Roll(eligible, Equal(), 3, 10, true, Seeded(5));
            Assert.AreEqual(4, items.Count, "Only 4 items exist");
            CollectionAssert.AllItemsAreUnique(items);
        }

        /// <summary>The real shop pool's shape (6 Common, 6 Uncommon, 11 Rare, 16 Heroic) fills every slot at any Luck.</summary>
        [Test]
        public void TheShop_FillsEverySlot([Values(-20, -5, -3, -2, 0, 5)] int luck)
        {
            var eligible = new List<ItemDropEntry>();
            foreach (var (quality, count) in new[] { (EffectQuality.Common, 6), (EffectQuality.Uncommon, 6), (EffectQuality.Rare, 11), (EffectQuality.Heroic, 16) })
            {
                for (int i = 0; i < count; i++) eligible.Add(Entry(quality));
            }
            int count20 = LootRoller.GetShopItemCount(10, luck, 1, 3, 20);
            for (int seed = 0; seed < 50; seed++)
            {
                Assert.AreEqual(count20, LootRoller.Roll(eligible, Equal(), luck, count20, true, Seeded(seed)).Count, $"Luck {luck}, seed {seed}");
            }
        }

        [TestCase(0, 10)]
        [TestCase(4, 14)]
        [TestCase(-3, 7)]
        [TestCase(-7, 3)]
        [TestCase(-50, 3)]
        [TestCase(10, 20)]
        [TestCase(50, 20)]
        public void ShopItemCount_FollowsLuckWithinTheLimits(int luck, int expected)
        {
            Assert.AreEqual(expected, LootRoller.GetShopItemCount(10, luck, 1, 3, 20));
        }

        [Test]
        public void ShopItemCount_MinWinsOverASmallerMax()
        {
            Assert.AreEqual(5, LootRoller.GetShopItemCount(10, 0, 1, 5, 2));
        }

        [Test]
        public void ExpectedQuality_IsTheWeightedAverage()
        {
            var settings = new LootSourceSettings { commonWeight = 50f, uncommonWeight = 0f, rareWeight = 50f, heroicWeight = 0f };
            var eligible = new List<ItemDropEntry> { Entry(EffectQuality.Common), Entry(EffectQuality.Rare) };
            Assert.AreEqual(EffectQuality.Uncommon, LootRoller.GetExpectedQuality(eligible, settings, 0));
        }
    }
}
