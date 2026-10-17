using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>Which class ability an ability item drives.</summary>
    public enum AbilitySlot
    {
        Primary,
        Special,
        Dash
    }

    /// <summary>
    /// A class ability carried by an item (a Glyph). It is data only: the class's weapon or dash
    /// component still performs the ability, and reads its numbers from here through
    /// <see cref="EffectStore.TryGetAbility{T}"/>. Without the item, those components fall back
    /// to their own values.
    ///
    /// The numbers live in a per-tier table. The owning item's <see cref="Effect.Tier"/> picks the
    /// row, so "Glyph of Whirlwind +2" is a separate item row that reuses this same asset.
    /// </summary>
    public abstract class AbilityBehavior : EffectBehavior
    {
        public abstract AbilitySlot Slot { get; }

        public abstract int TierCount { get; }

        /// <summary>
        /// What the ability does at <paramref name="tier"/>, built from that tier's numbers (one line
        /// per fact, damage-type tags like _magic_damage_ allowed). Shown under the item's description,
        /// so tooltips can't drift from the real values. Null = nothing to add.
        /// </summary>
        public virtual string DescribeTier(int tier) => null;
    }

    /// <summary><see cref="AbilityBehavior"/> with its per-tier table of <typeparamref name="TTier"/> rows.</summary>
    public abstract class AbilityBehavior<TTier> : AbilityBehavior where TTier : class, new()
    {
        [Tooltip("One row per item tier: row 0 is the base item, row 1 is \"+1\", and so on. " +
            "A tier past the last row uses the last row.")]
        [SerializeField] protected List<TTier> tiers = new() { new TTier() };

        public override int TierCount => tiers != null ? tiers.Count : 0;

        public IReadOnlyList<TTier> Tiers => tiers;

        /// <summary>The row for <paramref name="tier"/>, or null when the table is empty.</summary>
        public TTier GetTier(int tier) => AbilityTiers.Get(tiers, tier);

        public override string DescribeTier(int tier)
        {
            var row = GetTier(tier);
            return row == null ? null : Describe(row);
        }

        /// <summary>The description lines for one tier row. See <see cref="AbilityBehavior.DescribeTier"/>.</summary>
        protected virtual string Describe(TTier row) => null;
    }

    /// <summary>
    /// Marks a tier-table field where a smaller value is the stronger one (e.g. a spawn interval).
    /// Guard tests check that every tier is at least as strong as the one before it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LowerIsBetterAttribute : Attribute { }

    public static class AbilityTiers
    {
        /// <summary>A number for descriptions: no trailing zeros ("1.5", "2").</summary>
        public static string Number(float value) => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>A fraction as a percentage ("50%").</summary>
        public static string Percent(float fraction) => Number(fraction * 100f) + "%";

        /// <summary>"1 arrow" / "3 arrows".</summary>
        public static string Count(int count, string singular, string plural) => $"{count} {(count == 1 ? singular : plural)}";

        public static string Lines(params string[] lines)
        {
            var kept = new List<string>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line)) kept.Add(line);
            }
            return kept.Count == 0 ? null : string.Join("\n", kept);
        }

        /// <summary>Clamps a tier into a table of <paramref name="count"/> rows; -1 when the table is empty.</summary>
        public static int ClampIndex(int tier, int count)
        {
            if (count <= 0) return -1;
            return Mathf.Clamp(tier, 0, count - 1);
        }

        public static T Get<T>(IReadOnlyList<T> rows, int tier) where T : class
        {
            if (rows == null) return null;
            int index = ClampIndex(tier, rows.Count);
            return index < 0 ? null : rows[index];
        }
    }
}
