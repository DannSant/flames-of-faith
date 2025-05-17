using UnityEngine;

namespace Game.Combat {
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
       
        [Tooltip("Time between attacks.")]
        public float attackCooldownBase = 1f;

        [Tooltip("Initial damage")]
        public int baseDamage = 2;

        [Tooltip("For each MeleeDamage stat, damage will increase by this number ")]
        public int attackScale = 1;

        [Tooltip("For each AttackSpeed stat, regular damage will increase by this number ")]
        public float attackSpeedScale = 0.0075f;

        /*[Tooltip("Time between special attacks.")]
        public float specialAttackCooldown = 3f;

        [Tooltip("Initial damage for special attack")]
        public int specialBaseDamage = 5;        

        [Tooltip("For each MeleeDamage stat, special damage will increase by this number ")]
        public int specialAttackScale = 3;*/

        

    }

}