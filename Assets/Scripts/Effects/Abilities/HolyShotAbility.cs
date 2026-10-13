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
            [Tooltip("Extra arrows fired in a cone around the main one. Not implemented yet (class kits step 7).")]
            public int extraArrows = 0;
            [Tooltip("Degrees between neighbouring arrows of the cone. Not implemented yet (class kits step 7).")]
            public float coneAngle = 15f;
        }

        public override AbilitySlot Slot => AbilitySlot.Primary;
    }
}
