using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.Combat;
using Game.Enemies;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    public class StatBenefitTests
    {
        private const float Tolerance = 0.0001f;
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private T Make<T>() where T : ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<T>();
            created.Add(so);
            return so;
        }

        private static string Num(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
        private static string Pct(float fraction) => (fraction * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "%";

        // A full context with default settings and a Bow-like weapon.
        private StatBenefitContext MakeContext()
        {
            return new StatBenefitContext
            {
                damageSettings = Make<EnemyDamageSettings>(),
                pierceSettings = Make<PierceSettings>(),
                statSettings = Make<PlayerStatSettings>(),
                experienceSettings = Make<ExperienceSettings>(),
                corruptionSettings = Make<CorruptionSettings>(),
                hasWeapon = true,
                attackCooldownBase = 1.5f,
                attackSpeedScale = 0.075f,
                hasSpecial = true,
                specialCooldownBase = 4f,
                specialAttackSpeedScale = 0.075f,
                weaponUsesRange = true,
                rangeBase = 4f,
                rangeScale = 0.2f,
                dashCooldownBase = 2f,
                moveSpeedBase = 5f,
                moveSpeedScale = 0.25f,
                hasRetribution = true,
                retribution = new RetributionCurve
                {
                    damageTier1Cap = 35, damageTier2Cap = 50, damageHardCap = 70,
                    damageTier1Rate = 1, damageTier2Rate = 0.2f, damageTier3Rate = 0.01f,
                    meleeDamageScaling = 0.2f,
                    graceTier1Cap = 25, graceHardCap = 50, graceTier1Rate = 1, graceTier2Rate = 0.5f,
                    graceGeneratedAmount = 1, graceGainCooldown = 20,
                },
                meleeDamage = 10,
            };
        }

        // --- Moving hardcoded numbers to PlayerStatSettings didn't change gameplay -------------

        [Test]
        public void Defaults_ReproduceTheOldHardcodedFormulas()
        {
            var s = Make<PlayerStatSettings>();

            for (int p = 1; p <= 30; p++)
            {
                Assert.AreEqual(5f / (1f + (p - 1f) / 2.25f), s.GetRegenInterval(p), Tolerance, $"regen at {p}");
                Assert.AreEqual(0.20f + 0.01f * p, s.GetLifestealChance(p), Tolerance, $"lifesteal chance at {p}");
                Assert.AreEqual(Mathf.Clamp(p * 0.1f, 1f, 20f), s.GetLifestealHeal(p), Tolerance, $"lifesteal heal at {p}");
                Assert.AreEqual(Mathf.Max(2f / (1f + p * 0.08f), 0.01f), s.GetDashCooldown(p, 2f), Tolerance, $"dash at {p}");
                Assert.AreEqual(2f + p * 0.1f, s.GetPickupRange(p), Tolerance, $"pickup range at {p}");
                Assert.AreEqual(StatsCalculations.CalculateShopDiscountMultiplier(p), s.GetShopDiscountMultiplier(p), Tolerance, $"shop at {p}");
            }
            Assert.AreEqual(float.PositiveInfinity, s.GetRegenInterval(0));
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(60)]
        [TestCase(120)]
        public void LuckTiers_KeepTheOldRollProportions(int luck)
        {
            // The old UpgradeManager roll: tier 1 - luck (min 0), tiers 2-4 + luck/3, then weighted.
            float[] old = { Mathf.Max(0f, 80f - luck), 10f + luck / 3f, 8f + luck / 3f, 2f + luck / 3f };
            float oldTotal = old.Sum();

            float[] chances = Make<PlayerStatSettings>().GetUpgradeTierChances(luck);

            Assert.AreEqual(100f, chances.Sum(), 0.01f);
            for (int i = 0; i < old.Length; i++)
            {
                Assert.AreEqual(old[i] / oldTotal * 100f, chances[i], 0.01f, $"tier {i + 1}");
            }
        }

        // --- Negative speed stats can't make things instant anymore ----------------------------

        [TestCase(-5)]
        [TestCase(-14)]
        [TestCase(-100)]
        public void NegativeAttackSpeed_SlowsDown_NeverSpeedsUp(int attackSpeed)
        {
            float delay = StatsCalculations.CalculateAttackDelay(attackSpeed, 1.5f, 0.075f);

            Assert.GreaterOrEqual(delay, 1.5f);
            Assert.LessOrEqual(delay, 15f + Tolerance, "at most 10x slower");
        }

        [Test]
        public void NegativeDashRecovery_SlowsDown_NeverSpeedsUp()
        {
            Assert.GreaterOrEqual(StatsCalculations.CalculateDashCooldown(-50, 2f), 2f);
        }

        // --- Descriptions use the gameplay numbers --------------------------------------------

        [Test]
        public void Armor_ShowsTheRealReduction()
        {
            var ctx = MakeContext();
            float mult = ArmorMitigation.Multiplier(10, 25f, 0.15f);

            StringAssert.Contains(Pct(1f - mult) + " less damage", StatBenefitDescriber.Describe(StatType.Armor, 10, ctx));
            StringAssert.Contains("more damage", StatBenefitDescriber.Describe(StatType.Armor, -25, ctx));
        }

        [Test]
        public void Evasion_ShowsChanceAndCap()
        {
            var ctx = MakeContext();
            string text = StatBenefitDescriber.Describe(StatType.Evasion, 50, ctx);

            StringAssert.Contains("25% chance to dodge", text);
            StringAssert.Contains("max 30%", text);
        }

        [Test]
        public void Pierce_ShowsExtraTargetsSplitChanceAndNextStep()
        {
            var ctx = MakeContext();
            string text = StatBenefitDescriber.Describe(StatType.Pierce, 5, ctx);

            StringAssert.Contains("+1 enemies pierced", text);
            StringAssert.Contains("15% chance for arrows to split", text);
            StringAssert.Contains("next enemy at 8 Pierce", text);
        }

        [Test]
        public void AttackSpeed_ShowsBothCooldowns()
        {
            var ctx = MakeContext();
            string text = StatBenefitDescriber.Describe(StatType.AttackSpeed, 10, ctx);

            StringAssert.Contains($"every {Num(StatsCalculations.CalculateAttackDelay(10, 1.5f, 0.075f))}s", text);
            StringAssert.Contains($"special every {Num(StatsCalculations.CalculateAttackDelay(10, 4f, 0.075f))}s", text);
        }

        [Test]
        public void Regen_And_Lifesteal_UseTheSettings()
        {
            var ctx = MakeContext();

            StringAssert.Contains($"every {Num(ctx.statSettings.GetRegenInterval(3))}s", StatBenefitDescriber.Describe(StatType.HealthRegen, 3, ctx));
            StringAssert.Contains("No health regeneration", StatBenefitDescriber.Describe(StatType.HealthRegen, 0, ctx));
            StringAssert.Contains("30% chance on hit to heal 1 HP", StatBenefitDescriber.Describe(StatType.LifeSteal, 10, ctx));
        }

        [Test]
        public void Retribution_ShowsReflectFlatPartAndGrace()
        {
            var ctx = MakeContext();
            string text = StatBenefitDescriber.Describe(StatType.Retribution, 10, ctx);

            StringAssert.Contains("Reflect 10% of damage taken + 2", text);
            StringAssert.Contains("10% chance on hit to gain 1 Grace", text);
        }

        [Test]
        public void Range_SaysWhenTheWeaponDoesntUseIt()
        {
            var ctx = MakeContext();
            StringAssert.Contains("Attack range 6", StatBenefitDescriber.Describe(StatType.Range, 10, ctx));

            ctx.weaponUsesRange = false;
            StringAssert.Contains("doesn't use Attack Range", StatBenefitDescriber.Describe(StatType.Range, 10, ctx));
        }

        [Test]
        public void ShopDiscount_ShowsCurrentAndMax()
        {
            var ctx = MakeContext();
            string text = StatBenefitDescriber.Describe(StatType.ShopItemDiscount, 20, ctx);

            StringAssert.Contains("20% off", text);
            StringAssert.Contains("max 65%", text);
        }

        [Test]
        public void MissingContext_GivesNoLine()
        {
            Assert.IsNull(StatBenefitDescriber.Describe(StatType.Armor, 10, null));
            Assert.IsNull(StatBenefitDescriber.Describe(StatType.AttackSpeed, 10, new StatBenefitContext()));
        }

        // --- Guard: a new stat needs a line or an explicit "nothing to compute" ----------------

        private static readonly StatType[] StatsWithoutComputedBenefit =
        {
            StatType.MaxHealth, StatType.MeleeDamage, StatType.RangedDamage, StatType.MagicDamage,
            StatType.MastowAffinity, // depends on each debuff's own data
        };

        [Test]
        public void EveryStat_HasABenefitLine_OrIsListedAsHavingNone()
        {
            var ctx = MakeContext();
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                string text = StatBenefitDescriber.Describe(stat, 5, ctx);
                if (StatsWithoutComputedBenefit.Contains(stat))
                {
                    Assert.IsNull(text, $"{stat} is listed as having no computed benefit");
                }
                else
                {
                    Assert.IsFalse(string.IsNullOrEmpty(text), $"{stat} has no benefit line: add one to StatBenefitDescriber or list it here");
                }
            }
        }
    }
}
