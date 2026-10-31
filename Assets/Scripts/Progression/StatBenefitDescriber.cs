using Game.Combat;
using Game.Enemies;
using System.Globalization;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Everything the stat descriptions need besides the stat value. Built on the gameplay side
    /// (PlayerProgression.BuildStatBenefitContext) from the same settings and components that
    /// gameplay uses; tests build their own. Missing pieces just skip that stat's line.
    /// </summary>
    public class StatBenefitContext
    {
        public EnemyDamageSettings damageSettings;
        public PierceSettings pierceSettings;
        public PlayerStatSettings statSettings;
        public ExperienceSettings experienceSettings;
        public CorruptionSettings corruptionSettings;

        // Current weapon (0 / false when unknown)
        public bool hasWeapon;
        public float attackCooldownBase;
        public float attackSpeedScale;
        public bool hasSpecial;
        public float specialCooldownBase;
        public float specialAttackSpeedScale;
        public bool weaponUsesRange;
        public float rangeBase;
        public float rangeScale;

        public float dashCooldownBase;   // 0 = unknown
        public float moveSpeedBase;      // 0 = unknown
        public float moveSpeedScale;

        public bool hasRetribution;
        public RetributionCurve retribution;
        public int meleeDamage;          // Retribution adds a share of it to reflected damage
    }

    /// <summary>
    /// Turns a stat value into the concrete benefit it gives right now ("17% less damage taken"),
    /// using the same pure formulas as gameplay. Returns null for stats with nothing to compute
    /// (Health, the damage stats) so the static description stands alone.
    /// </summary>
    public static class StatBenefitDescriber
    {
        public static string Describe(StatType stat, int points, StatBenefitContext ctx)
        {
            if (ctx == null) return null;

            switch (stat)
            {
                case StatType.Armor: return DescribeArmor(points, ctx);
                case StatType.Evasion: return DescribeEvasion(points, ctx);
                case StatType.Pierce: return DescribePierce(points, ctx);
                case StatType.AttackSpeed: return DescribeAttackSpeed(points, ctx);
                case StatType.MoveSpeed: return DescribeMoveSpeed(points, ctx);
                case StatType.DashCooldown: return DescribeDash(points, ctx);
                case StatType.HealthRegen: return DescribeRegen(points, ctx);
                case StatType.LifeSteal: return DescribeLifesteal(points, ctx);
                case StatType.ShopItemDiscount: return DescribeShopDiscount(points, ctx);
                case StatType.Retribution: return DescribeRetribution(points, ctx);
                case StatType.Range: return DescribeRange(points, ctx);
                case StatType.PickupRange: return DescribePickupRange(points, ctx);
                case StatType.GraceAffinity: return DescribeGraceAffinity(points, ctx);
                case StatType.ExperienceToLevelUpReduction: return DescribeExperience(points, ctx);
                case StatType.Luck: return DescribeLuck(points, ctx);
                case StatType.SkillDuration: return DescribeSkillDuration(points);
                case StatType.HealingReceived: return DescribeHealingReceived(points);
                default: return null;
            }
        }

        // --- Formatting -------------------------------------------------------------------------

        private static string Pct(float fraction) => (fraction * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "%";
        private static string Num(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
        private static string Signed(float value) => (value >= 0 ? "+" : "") + Num(value);

        // --- Stats --------------------------------------------------------------------------------

        private static string DescribeArmor(int points, StatBenefitContext ctx)
        {
            var s = ctx.damageSettings;
            if (s == null) return null;

            float multiplier = ArmorMitigation.Multiplier(points, s.ArmorEffectivenessConstant, s.MinDamagePercent);
            if (points < 0)
            {
                return $"Take {Pct(multiplier - 1f)} more damage (negative Armor)";
            }
            return $"Take {Pct(1f - multiplier)} less damage (never more than {Pct(1f - s.MinDamagePercent)} less)";
        }

        private static string DescribeEvasion(int points, StatBenefitContext ctx)
        {
            var s = ctx.damageSettings;
            if (s == null) return null;
            return $"{Pct(s.GetEvasionChance(points))} chance to dodge a hit (max {Pct(s.EvasionMaxChance)})";
        }

        private static string DescribePierce(int points, StatBenefitContext ctx)
        {
            var s = ctx.pierceSettings;
            if (s == null) return null;

            int extra = s.GetExtraPierce(points);
            string text = $"{Signed(extra)} enemies pierced, {Pct(s.GetSplitChance(points))} chance for arrows to split";
            if (s.ExtraPiercePerPoint > 0f)
            {
                int next = Mathf.CeilToInt((extra + 1) / s.ExtraPiercePerPoint - 0.0001f);
                if (next > points) text += $" (next enemy at {next} Pierce)";
            }
            return text;
        }

        private static string DescribeAttackSpeed(int points, StatBenefitContext ctx)
        {
            var s = ctx.statSettings;
            if (s == null || !ctx.hasWeapon) return null;

            float delay = s.GetAttackDelay(points, ctx.attackCooldownBase, ctx.attackSpeedScale);
            string text = $"Basic attack every {Num(delay)}s";
            if (ctx.hasSpecial)
            {
                float special = s.GetAttackDelay(points, ctx.specialCooldownBase, ctx.specialAttackSpeedScale);
                text += $", special every {Num(special)}s";
            }
            if (points >= s.AttackSpeedHardCap)
            {
                text += $". At the {s.AttackSpeedHardCap} point cap: more points do nothing";
            }
            else if (points > s.AttackSpeedSoftCap)
            {
                text += $". Past {s.AttackSpeedSoftCap} points each one counts for less (cap {s.AttackSpeedHardCap})";
            }
            return text;
        }

        private static string DescribeMoveSpeed(int points, StatBenefitContext ctx)
        {
            if (ctx.moveSpeedBase <= 0f) return null;

            float speed = StatsCalculations.CalculateMoveSpeed(points, ctx.moveSpeedBase, ctx.moveSpeedScale);
            return $"Move speed {Num(speed)} ({Signed((speed / ctx.moveSpeedBase - 1f) * 100f)}%)";
        }

        private static string DescribeDash(int points, StatBenefitContext ctx)
        {
            if (ctx.statSettings == null || ctx.dashCooldownBase <= 0f) return null;

            float cooldown = ctx.statSettings.GetDashCooldown(points, ctx.dashCooldownBase);
            return $"Dash recharges in {Num(cooldown)}s (base {Num(ctx.dashCooldownBase)}s)";
        }

        private static string DescribeRegen(int points, StatBenefitContext ctx)
        {
            if (ctx.statSettings == null) return null;
            if (points <= 0) return "No health regeneration";
            return $"Heal 1 HP every {Num(ctx.statSettings.GetRegenInterval(points))}s";
        }

        private static string DescribeLifesteal(int points, StatBenefitContext ctx)
        {
            if (ctx.statSettings == null) return null;
            if (points <= 0) return "No life steal";

            var s = ctx.statSettings;
            return $"{Pct(s.GetLifestealChance(points))} chance on hit to heal {Num(s.GetLifestealHeal(points))} HP";
        }

        private static string DescribeShopDiscount(int points, StatBenefitContext ctx)
        {
            var s = ctx.statSettings;
            if (s == null) return null;

            float discount = 1f - s.GetShopDiscountMultiplier(points);
            float max = 1f - s.GetShopDiscountMultiplier(s.ShopDiscountHardCap);
            return $"{Pct(discount)} off shop prices (max {Pct(max)})";
        }

        private static string DescribeRetribution(int points, StatBenefitContext ctx)
        {
            if (!ctx.hasRetribution) return null;
            if (points <= 0) return "No damage reflected";

            var r = ctx.retribution;
            float flat = ctx.meleeDamage * r.meleeDamageScaling;
            string text = $"Reflect {Pct(r.GetReflectPercent(points))} of damage taken";
            if (flat > 0f) text += $" + {Num(flat)}";
            text += $". {Pct(r.GetGraceChance(points))} chance on hit to gain {r.graceGeneratedAmount} Grace (at most once every {Num(r.graceGainCooldown)}s)";
            return text;
        }

        private static string DescribeRange(int points, StatBenefitContext ctx)
        {
            if (!ctx.hasWeapon) return null;
            if (!ctx.weaponUsesRange) return "Your weapon doesn't use Attack Range";

            float range = ctx.rangeBase + points * ctx.rangeScale;
            return $"Attack range {Num(range)} (base {Num(ctx.rangeBase)})";
        }

        private static string DescribePickupRange(int points, StatBenefitContext ctx)
        {
            if (ctx.statSettings == null) return null;
            return $"Pickups fly to you from {Num(ctx.statSettings.GetPickupRange(points))} units away";
        }

        private static string DescribeGraceAffinity(int points, StatBenefitContext ctx)
        {
            string text = $"{Signed(points)} Grace at the end of each wave";
            if (ctx.corruptionSettings != null)
            {
                text += $", {Pct(ctx.corruptionSettings.GetGracePickupChance(points))} chance for enemies to drop a Grace pickup";
            }
            return text;
        }

        private static string DescribeExperience(int points, StatBenefitContext ctx)
        {
            if (ctx.experienceSettings == null) return null;
            return $"{Signed(points * ctx.experienceSettings.XpBonusPerReductionPoint * 100f)}% experience from enemies";
        }

        private static string DescribeLuck(int points, StatBenefitContext ctx)
        {
            if (ctx.statSettings == null) return null;

            float[] tiers = ctx.statSettings.GetUpgradeTierChances(points);
            if (tiers.Length == 0) return null;
            var parts = new string[tiers.Length];
            for (int i = 0; i < tiers.Length; i++) parts[i] = $"T{i + 1} {Num(Mathf.Round(tiers[i] * 10f) / 10f)}%";
            return "Level-up upgrade tiers: " + string.Join(", ", parts);
        }

        private static string DescribeSkillDuration(int points)
        {
            return $"{Signed(points)}s to the duration of skills and item effects";
        }

        private static string DescribeHealingReceived(int points)
        {
            return $"{Signed(points)} HP on every heal (not regeneration)";
        }
    }
}
