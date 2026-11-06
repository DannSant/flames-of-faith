using Game.Effects;
using Game.Progression;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Control
{
    [CreateAssetMenu(fileName = "CharacterClass", menuName = "Game/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {
        public string characterName;
        public CharacterClass characterClass;
        public RuntimeAnimatorController animatorController;
        public Sprite defaultSprite;
        [Tooltip("Shown for the class in menus (e.g. the skill tree). Falls back to Default Sprite.")]
        public Sprite icon;
        public List<Sprite> abilityIcons;

        [Header("Dash Ability")]
        public string dashName;
        [TextArea] public string dashDescription;
        [Tooltip("Seconds for a used dash charge to come back, before the Dash Cooldown stat.")]
        [Min(0.01f)]
        public float dashCooldown = 2f;
        [Tooltip("Speed while dashing, as a multiple of the walking speed (3 = three times as fast).")]
        [Min(0f)]
        public float dashSpeed = 3f;
        [Tooltip("Seconds a dash lasts. The class's dash Glyph can override it.")]
        [Min(0.01f)]
        public float dashDuration = 0.2f;
        [Tooltip("Dashes that can be stored and used back to back. The class's dash Glyph can override it.")]
        [Min(1)]
        public int dashMaxCharges = 1;
        [Tooltip("The damage stat the dash's damage scales with (the Crusader's charge, the Sentinel's traps, the Augur's flames).")]
        public Game.Combat.WeaponClass dashDamageStat = Game.Combat.WeaponClass.None;

        [Header("Menu Display")]
        [TextArea] public string classDescription;

        [Header("Stat Guide")]
        [Tooltip("Stats that matter most for this class: one Key stat (gold star) and its Complementary stats " +
            "(silver star), in display order. They go on top of the Stats pane's first tab and are marked on " +
            "the upgrade choices; the justification shows in their tooltip.")]
        public List<ClassStatPriority> statPriorities = new();
    }

    /// <summary>
    /// The classes. Assets store this enum by index, so never reorder it: append. Saves store it by name,
    /// so a rename needs an entry in <see cref="CharacterClassNames"/>.
    /// </summary>
    public enum CharacterClass
    {
        None,
        Crusader, // "Warrior" until Nov 2026
        Augur,    // "Mage" until Nov 2026
        Sentinel, // "Archer" until Nov 2026
        Rogue
    }

    /// <summary>Saved class names, including the names classes had before they were renamed.</summary>
    public static class CharacterClassNames
    {
        // Old name -> current class. The meta save (<player>_meta.dat) stores classes by name.
        private static readonly Dictionary<string, CharacterClass> FormerNames = new()
        {
            ["Warrior"] = CharacterClass.Crusader,
            ["Archer"] = CharacterClass.Sentinel,
            ["Mage"] = CharacterClass.Augur,
        };

        /// <summary>The current name for a saved one: a former name is mapped, anything else is kept as is.</summary>
        public static string Normalize(string savedName)
        {
            return savedName != null && FormerNames.TryGetValue(savedName, out var current) ? current.ToString() : savedName;
        }
    }

}