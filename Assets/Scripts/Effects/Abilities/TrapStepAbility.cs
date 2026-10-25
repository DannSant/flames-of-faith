using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Archer's dash (Glyph of Trap Step), performed by <see cref="Game.Control.ArcherDash"/>.
    /// Without this item the dash keeps its prefab values.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Archer/Trap Step")]
    public class TrapStepAbility : AbilityBehavior<TrapStepAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Seconds the dash lasts (longer = further).")]
            [Min(0.01f)] public float dashDuration = 0.15f;
            [Tooltip("Base damage of each trap.")]
            public int trapDamage = 2;
            [Tooltip("Share of Ranged Damage each trap hit adds (1 = the full stat).")]
            [Min(0f)]
            public float trapStatScale = 0.7f;
            [Tooltip("Most traps set at once; setting another removes the oldest.")]
            [Min(1)] public int maxTraps = 3;
        }

        protected override string Describe(Values v) => AbilityTiers.Lines(
            $"Traps deal {v.trapDamage} base damage, up to {v.maxTraps} at once");

        public override AbilitySlot Slot => AbilitySlot.Dash;
    }
}
