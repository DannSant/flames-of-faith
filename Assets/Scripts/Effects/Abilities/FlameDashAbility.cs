using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Augur's dash (Scroll of Flame Dash), performed by <see cref="Game.Control.AugurDash"/>.
    /// Without this item the dash keeps its prefab values.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Augur/Flame Dash")]
    public class FlameDashAbility : AbilityBehavior<FlameDashAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Seconds the dash lasts (longer = further).")]
            [Min(0.01f)] public float dashDuration = 0.3f;
            [Tooltip("Base damage of each flame left behind.")]
            public int flameDamage = 2;
            [Tooltip("Dash charges.")]
            [Min(1)] public int maxCharges = 2;
            [Tooltip("Seconds between flames while dashing (lower = more flames).")]
            [LowerIsBetter, Min(0.01f)] public float flameSpawnInterval = 0.05f;
        }

        public override AbilitySlot Slot => AbilitySlot.Dash;
    }
}
