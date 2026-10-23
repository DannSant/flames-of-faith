using System.Collections.Generic;
using System.Linq;
using Game.Effects;
using Game.RunEncounters;
using Game.Utils;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// The Luck-weighted quality roll used by the shop and the event item rewards. Negative Luck
    /// (Cursed Coin, Faikan Boots) used to zero or flip the tier weights: at -2/-3 Luck only Common
    /// could roll, and the shop threw once its Commons ran out, freezing the level load.
    /// </summary>
    public class EffectQualityScalerTests
    {
        private const float Scale = 2f;
        private readonly List<Effect> created = new List<Effect>();

        [TearDown]
        public void TearDown()
        {
            foreach (var effect in created) Object.DestroyImmediate(effect);
            created.Clear();
        }

        private List<Effect> MakePool(params (EffectQuality quality, int count)[] tiers)
        {
            var pool = new List<Effect>();
            foreach (var (quality, count) in tiers)
            {
                for (int i = 0; i < count; i++)
                {
                    var effect = ScriptableObject.CreateInstance<Effect>();
                    effect.Quality = quality;
                    created.Add(effect);
                    pool.Add(effect);
                }
            }
            return pool;
        }

        [Test]
        public void Weights_StayPositive_ForAnyLuck([Values(-20, -5, -3, -2, -1, 0, 1, 5, 20)] int luck)
        {
            var weights = EffectQualityScaler.GetTierWeights(luck, Scale);
            Assert.That(weights, Has.All.GreaterThan(0f), $"Luck {luck}: {string.Join(", ", weights)}");
        }

        [Test]
        public void NegativeLuck_MakesHigherTiersRarer([Values(-1, -2, -3, -10)] int luck)
        {
            var weights = EffectQualityScaler.GetTierWeights(luck, Scale);
            for (int i = 1; i < weights.Length; i++)
            {
                Assert.Less(weights[i], weights[i - 1], $"Luck {luck}: tier {i} isn't rarer than tier {i - 1}");
            }
        }

        [Test]
        public void ZeroLuck_WeighsEveryTierEqually()
        {
            Assert.That(EffectQualityScaler.GetTierWeights(0, Scale), Has.All.EqualTo(1f));
        }

        [Test]
        public void Roll_NeverPicksATierMissingFromThePool([Values(-5, -3, -2, 0, 3)] int luck)
        {
            var pool = MakePool((EffectQuality.Uncommon, 2), (EffectQuality.Heroic, 2));
            for (int i = 0; i < 500; i++)
            {
                var rolled = EffectQualityScaler.RollQualityTier(luck, Scale, pool);
                Assert.That(rolled, Is.EqualTo(EffectQuality.Uncommon).Or.EqualTo(EffectQuality.Heroic), $"Luck {luck}");
            }
        }

        // The shop's draw loop with its real slot count: more slots than Commons at -2/-3 Luck, which used to throw,
        // and the minimum slots at very low Luck.
        [Test]
        public void ShopDraw_WithFewCommons_FillsEverySlot([Values(-20, -8, -4, -3, -2, 0, 5)] int luck)
        {
            var pool = MakePool((EffectQuality.Common, 6), (EffectQuality.Uncommon, 6), (EffectQuality.Rare, 11), (EffectQuality.Heroic, 16));
            int slots = ShopKeeper.GetDisplayCount(10, luck, 1, 3, 20);

            for (int run = 0; run < 50; run++)
            {
                var remaining = new List<Effect>(pool);
                for (int i = 0; i < slots; i++)
                {
                    var rolled = EffectQualityScaler.RollQualityTier(luck, Scale, remaining);
                    var candidate = remaining.FirstOrDefault(e => e.Quality == rolled);
                    Assert.IsNotNull(candidate, $"Luck {luck}: rolled {rolled} with none left (slot {i + 1} of {slots})");
                    remaining.Remove(candidate);
                }
            }
        }
    }
}
