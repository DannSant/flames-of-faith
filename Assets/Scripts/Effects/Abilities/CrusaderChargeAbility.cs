using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Warrior's dash (Glyph of Crusader Charge), performed by <see cref="Game.Control.WarriorDash"/>.
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
            [Tooltip("Share of the Melee Damage stat added to each hit (1.5 = 150%).")]
            public float meleeDamageScale = 1.5f;
        }

        protected override string Describe(Values v) => AbilityTiers.Lines(
            $"Deals {v.damage} base damage plus {AbilityTiers.Percent(v.meleeDamageScale)} of Melee Damage and knocks enemies back");

        public override AbilitySlot Slot => AbilitySlot.Dash;
    }
}
