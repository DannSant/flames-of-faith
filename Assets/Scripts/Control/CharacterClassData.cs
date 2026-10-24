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

        [Header("Menu Display")]
        [TextArea] public string classDescription;

        [Header("Stat Guide")]
        [Tooltip("Stats that matter most for this class: one Key stat (gold star) and its Complementary stats " +
            "(silver star), in display order. They go on top of the Stats pane's first tab and are marked on " +
            "the upgrade choices; the justification shows in their tooltip.")]
        public List<ClassStatPriority> statPriorities = new();
    }

    public enum CharacterClass
    {
        None,
        Warrior,
        Mage,
        Archer,
        Rogue
    }

}