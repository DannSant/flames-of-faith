using System;
using UnityEngine;

namespace Game.Effects.Abilities
{
    /// <summary>
    /// The Warrior's basic attack (Scroll of Crusader Strike), performed by <see cref="Game.Combat.SwordWeapon"/>.
    /// Without this item the sword uses its own WeaponData.
    /// </summary>
    [CreateAssetMenu(menuName = "Effects/Abilities/Warrior/Crusader Strike")]
    public class CrusaderStrikeAbility : AbilityBehavior<CrusaderStrikeAbility.Values>
    {
        [Serializable]
        public class Values
        {
            [Tooltip("Replaces the sword WeaponData's base damage.")]
            public int baseDamage = 10;
            [Tooltip("Added to the bleed debuff's landing chance (0.05 = +5%).")]
            public float bleedChanceBonus = 0f;
        }

        public override AbilitySlot Slot => AbilitySlot.Primary;
    }
}
