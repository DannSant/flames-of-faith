using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Augur's special (Glyph of Fire Tornadoes), performed by <see cref="Game.Combat.ScepterWeapon"/>:
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
            [Tooltip("Base damage of the explosion each tornado sets off on its first hit (replaces the explosion prefab's damage).")]
            public int tornadoExplosionDamage = 5;
            [Tooltip("Share of Magic Damage a tornado's explosion adds (1 = the full stat).")]
            [Min(0f)]
            public float explosionStatScale = 1f;
        }

        protected override string Describe(Values v) => AbilityTiers.Lines(
            $"Base damage {v.baseDamage}",
            $"{AbilityTiers.Count(v.tornadoCount, "tornado", "tornadoes")}, each exploding for {v.tornadoExplosionDamage} base damage on its first hit");

        public override AbilitySlot Slot => AbilitySlot.Special;
    }
}
