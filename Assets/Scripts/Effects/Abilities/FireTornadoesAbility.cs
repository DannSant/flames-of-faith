using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Augur's special (Scroll of Fire Tornadoes), performed by <see cref="Game.Combat.ScepterWeapon"/>:
    /// tornadoes around the player, then timed explosions on the marked target.
    /// Without this item the scepter uses its own special WeaponData.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Augur/Fire Tornadoes")]
    public class FireTornadoesAbility : AbilityBehavior<FireTornadoesAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the scepter special WeaponData's base damage. The target explosions deal a share of it (ScepterWeapon.targetExplosionDamageMultiplier).")]
            public int baseDamage = 30;
            [Tooltip("Tornadoes spawned evenly around the player (replaces the special WeaponData's projectileAmount).")]
            [Min(1)] public int tornadoCount = 4;
            [Tooltip("Base damage of each tornado's own explosion. Not implemented yet (class kits step 7): " +
                "the tornado prefab's fixed damage is used.")]
            public int tornadoExplosionDamage = 5;
        }

        public override AbilitySlot Slot => AbilitySlot.Special;
    }
}
