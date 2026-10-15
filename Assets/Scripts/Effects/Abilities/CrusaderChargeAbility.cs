using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Warrior's dash (Scroll of Crusader Charge), performed by <see cref="Game.Control.WarriorDash"/>.
    /// Without this item the dash hitbox keeps its prefab values.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Warrior/Crusader Charge")]
    public class CrusaderChargeAbility : AbilityBehavior<CrusaderChargeAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Base damage of the dash hitbox.")]
            public int damage = 2;
            [Tooltip("Knockback force the dash hitbox applies.")]
            public float knockbackForce = 10f;
        }

        protected override string Describe(Values v) => AbilityTiers.Lines(
            $"Deals {v.damage} base damage and knocks enemies back");

        public override AbilitySlot Slot => AbilitySlot.Dash;
    }
}
