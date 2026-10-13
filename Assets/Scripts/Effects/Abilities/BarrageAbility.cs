using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Archer's special (Scroll of Barrage), performed by <see cref="Game.Combat.BowWeapon"/>: a
    /// ring of arrows around the player, then homing arrows at the marked target.
    /// Without this item the bow uses its own special WeaponData and Inspector values.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Archer/Barrage")]
    public class BarrageAbility : AbilityBehavior<BarrageAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the bow special WeaponData's base damage. The target arrows deal a share of it (BowWeapon.barrageDamageMultiplier).")]
            public int baseDamage = 30;
            [Tooltip("Arrows in the ring around the player (replaces the special WeaponData's projectileAmount).")]
            [Min(1)] public int ringArrowCount = 16;
            [Tooltip("Homing arrows fired at the marked target (replaces BowWeapon.barrageArrowCount). 0 = none.")]
            [Min(0)] public int targetArrowCount = 6;
        }

        public override AbilitySlot Slot => AbilitySlot.Special;
    }
}
