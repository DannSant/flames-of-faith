using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Augur's basic attack (Scroll of Fireball), performed by <see cref="Game.Combat.ScepterWeapon"/>.
    /// Without this item the scepter uses its own WeaponData.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Augur/Fireball")]
    public class FireballAbility : AbilityBehavior<FireballAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the scepter WeaponData's base damage.")]
            public int baseDamage = 12;
            [Tooltip("Base damage of the explosion where the fireball hits an enemy. 0 = no explosion.")]
            public int impactExplosionDamage = 0;
        }

        public override AbilitySlot Slot => AbilitySlot.Primary;
    }
}
