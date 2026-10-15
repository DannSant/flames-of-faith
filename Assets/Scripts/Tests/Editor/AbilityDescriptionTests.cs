using Game.Effects.Abilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// Kit item tooltips describe the owned tier from the ability's own numbers. Guards that every
    /// tier says something, and that upgrade-only mechanics show exactly on the tiers that have them.
    /// </summary>
    public class AbilityDescriptionTests
    {
        private static List<T> Load<T>() where T : AbilityBehavior =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(a => a != null)
                .ToList();

        [Test]
        public void EveryTier_HasADescription()
        {
            var abilities = Load<AbilityBehavior>();
            Assert.IsNotEmpty(abilities, "No ability assets found");

            foreach (var ability in abilities)
            {
                for (int tier = 0; tier < ability.TierCount; tier++)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(ability.DescribeTier(tier)), $"{ability.name} tier {tier} has no description");
                }
            }
        }

        [Test]
        public void Fireball_MentionsTheExplosionOnlyWhenItHasOne()
        {
            foreach (var ability in Load<FireballAbility>())
            {
                for (int tier = 0; tier < ability.TierCount; tier++)
                {
                    bool explodes = ability.GetTier(tier).impactExplosionDamage > 0;
                    string text = ability.DescribeTier(tier);
                    if (explodes) StringAssert.Contains("Explodes", text, $"{ability.name} tier {tier}");
                    else StringAssert.DoesNotContain("Explodes", text, $"{ability.name} tier {tier}");
                }
            }
        }

        [Test]
        public void Whirlwind_MentionsTheShieldOnlyWhenItHasOne()
        {
            foreach (var ability in Load<WhirlwindAbility>())
            {
                for (int tier = 0; tier < ability.TierCount; tier++)
                {
                    bool shield = ability.GetTier(tier).shieldDuration > 0f;
                    string text = ability.DescribeTier(tier);
                    if (shield) StringAssert.Contains("shield", text, $"{ability.name} tier {tier}");
                    else StringAssert.DoesNotContain("shield", text, $"{ability.name} tier {tier}");
                }
            }
        }

        [Test]
        public void HolyShot_MentionsExtraArrowsOnlyWhenItHasThem()
        {
            foreach (var ability in Load<HolyShotAbility>())
            {
                for (int tier = 0; tier < ability.TierCount; tier++)
                {
                    bool extra = ability.GetTier(tier).extraArrows > 0;
                    string text = ability.DescribeTier(tier);
                    if (extra) StringAssert.Contains("extra arrow", text, $"{ability.name} tier {tier}");
                    else StringAssert.DoesNotContain("extra arrow", text, $"{ability.name} tier {tier}");
                }
            }
        }

        [TestCase(1, "1 arrow")]
        [TestCase(3, "3 arrows")]
        public void Count_UsesSingularAndPlural(int count, string expected)
        {
            Assert.AreEqual(expected, AbilityTiers.Count(count, "arrow", "arrows"));
        }

        [Test]
        public void Lines_SkipsEmptyLines()
        {
            Assert.AreEqual("a\nb", AbilityTiers.Lines("a", null, "", "b"));
            Assert.IsNull(AbilityTiers.Lines(null, ""));
        }
    }
}
