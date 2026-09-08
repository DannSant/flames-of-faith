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
        public List<Sprite> abilityIcons;

        [Header("Main Stat Progression")]
        [Tooltip("The stat this class's weapon benefits most from (e.g. Melee Damage for a sword class).")]
        public StatType mainStat = StatType.MeleeDamage;
        [Tooltip("The player gains +1 to mainStat every N level-ups. Set to 1 for every level.")]
        public int levelsPerMainStatPoint = 1;

        [Header("Dash Ability")]
        public string dashName;
        [TextArea] public string dashDescription;

        [Header("Menu Display")]
        [TextArea] public string classDescription;
        public Effect startingItem;
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