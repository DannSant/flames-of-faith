using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Archer's basic attack (Scroll of Holy Shot), performed by <see cref="Game.Combat.BowWeapon"/>.
    /// Without this item the bow uses its own WeaponData.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Archer/Holy Shot")]
    public class HolyShotAbility : AbilityBehavior<HolyShotAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the bow WeaponData's base damage.")]
            public int baseDamage = 15;
            [Tooltip("Extra arrows fired in a cone around the main one, alternating sides. They fly straight " +
                "for the weapon's range and don't trigger on-hit items (those trigger once per volley).")]
            [Min(0)] public int extraArrows = 0;
            [Tooltip("Degrees between neighbouring arrows of the cone.")]
            public float coneAngle = 15f;
            [Tooltip("Damage of each extra arrow as a fraction of the main arrow's base damage.")]
            [Min(0f)] public float extraArrowDamageMultiplier = 1f;
        }

        public override AbilitySlot Slot => AbilitySlot.Primary;
    }
}
