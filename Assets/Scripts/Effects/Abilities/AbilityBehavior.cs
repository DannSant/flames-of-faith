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
    /// A class ability carried by an item (a Scroll). It is data only: the class's weapon or dash
    /// component still performs the ability, and reads its numbers from here through
    /// <see cref="EffectStore.TryGetAbility{T}"/>. Without the item, those components fall back
    /// to their own values.
    ///
    /// The numbers live in a per-tier table. The owning item's <see cref="Effect.Tier"/> picks the
    /// row, so "Scroll of Whirlwind +2" is a separate item row that reuses this same asset.
    /// </summary>
    public abstract class AbilityBehavior : EffectBehavior
    {
        public abstract AbilitySlot Slot { get; }

        public abstract int TierCount { get; }
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
    }

    /// <summary>
    /// Marks a tier-table field where a smaller value is the stronger one (e.g. a spawn interval).
    /// Guard tests check that every tier is at least as strong as the one before it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LowerIsBetterAttribute : Attribute { }

    public static class AbilityTiers
    {
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
