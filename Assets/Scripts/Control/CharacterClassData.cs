using Game.Effects;
using Game.Progression;
using NUnit.Framework;
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