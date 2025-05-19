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