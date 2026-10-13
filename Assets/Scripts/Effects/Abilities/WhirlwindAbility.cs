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
            [Tooltip("Seconds the projectile-reflecting shield lasts after the whirlwind. 0 = no shield. " +
                "Not implemented yet (class kits step 7).")]
            public float shieldDuration = 0f;
        }

        public override AbilitySlot Slot => AbilitySlot.Special;
    }
}
