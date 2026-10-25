using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// How hard one attack hits: base damage plus a share of one of the player's damage stats. The spawner owns it
    /// (an item behavior, a dash ability, a weapon) and pushes it into the damage source it spawns, so the damage of an
    /// attack lives in one place: the balance sheet's ItemBehaviors_, Ability_ and Weapons tabs. The prefab's
    /// DamageSourceData only says how it hits (hit mode, pierce, knockback, speed, lifetime).
    /// Damage = (Base Damage + damage stat x Stat Scale) x stack multiplier x Grace (DamageCalculator).
    /// </summary>
    [Serializable]
    public struct DamageProfile
    {
        [Tooltip("Damage before the player's damage stat is added.")]
        [Min(0)]
        public int baseDamage;
        [Tooltip("Which damage stat adds to it: Melee, Ranged or Magic Damage. None adds no stat.")]
        public WeaponClass damageStat;
        [Tooltip("Share of that damage stat added to each hit (1 = the full stat, 0.2 = a fifth).")]
        [Min(0f)]
        public float statScale;

        public DamageProfile(int baseDamage, WeaponClass damageStat, float statScale)
        {
            this.baseDamage = baseDamage;
            this.damageStat = damageStat;
            this.statScale = statScale;
        }

        /// <summary>Whether it deals anything before Grace: a base or a stat share.</summary>
        public bool DealsDamage => baseDamage > 0 || (damageStat != WeaponClass.None && statScale > 0f);

        public void ApplyTo(DamageSourceBase source)
        {
            if (source != null) source.SetDamage(this);
        }
    }
}
