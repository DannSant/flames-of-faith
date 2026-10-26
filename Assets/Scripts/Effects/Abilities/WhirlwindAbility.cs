using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Warrior's special (Glyph of Whirlwind), performed by <see cref="Game.Combat.SwordWeapon"/>.
    /// Without this item the sword uses its own special WeaponData.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Warrior/Whirlwind")]
    public class WhirlwindAbility : AbilityBehavior<WhirlwindAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the sword special WeaponData's base damage. Dealt on contact, then every Tick Interval.")]
            public int baseDamage = 20;
            [Tooltip("Seconds the spin keeps hitting. The special animation replays until it ends. 0 = the spin lasts its animation once.")]
            [Min(0f)] public float spinDuration = 2f;
            [Tooltip("Seconds between hits on an enemy that stays in the spin. 0 = each enemy is hit once per spin.")]
            [Min(0f)] [LowerIsBetter] public float tickInterval = 0.5f;
            [Tooltip("Seconds of the holy shield raised when the whirlwind is cast. 0 = no shield. While it's up, " +
                "enemy projectiles are absorbed and heal instead of hurting (melee and contact hits still land).")]
            [Min(0f)] public float shieldDuration = 0f;
            [Tooltip("Share of an absorbed projectile's damage healed (0.5 = half), through the normal healing path.")]
            [Min(0f)] public float absorbHealFraction = 0.5f;
        }

        protected override string Describe(Values v) => AbilityTiers.Lines(
            v.tickInterval > 0f && v.spinDuration > 0f
                ? $"Base damage {v.baseDamage} every {AbilityTiers.Number(v.tickInterval)}s for {AbilityTiers.Number(v.spinDuration)}s"
                : $"Base damage {v.baseDamage}",
            v.shieldDuration > 0f
                ? $"Raises a holy shield for {AbilityTiers.Number(v.shieldDuration)}s: enemy projectiles are absorbed and heal {AbilityTiers.Percent(v.absorbHealFraction)} of their damage"
                : null);

        public override AbilitySlot Slot => AbilitySlot.Special;
    }
}
