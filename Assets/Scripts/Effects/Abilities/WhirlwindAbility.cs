using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Warrior's special (Scroll of Whirlwind), performed by <see cref="Game.Combat.SwordWeapon"/>.
    /// Without this item the sword uses its own special WeaponData.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Warrior/Whirlwind")]
    public class WhirlwindAbility : AbilityBehavior<WhirlwindAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the sword special WeaponData's base damage.")]
            public int baseDamage = 20;
            [Tooltip("Seconds of the holy shield raised when the whirlwind is cast. 0 = no shield. While it's up, " +
                "enemy projectiles are absorbed and heal instead of hurting (melee and contact hits still land).")]
            [Min(0f)] public float shieldDuration = 0f;
            [Tooltip("Share of an absorbed projectile's damage healed (0.5 = half), through the normal healing path.")]
            [Min(0f)] public float absorbHealFraction = 0.5f;
        }

        public override AbilitySlot Slot => AbilitySlot.Special;
    }
}
