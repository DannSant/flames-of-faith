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

        [Tooltip("Depending on the weapon class damage will scale diferently. Example Melee weapons only scale if damage is done through a weapon collider instead of a projectile")]
        public WeaponClass weaponClass = WeaponClass.Melee;
    }

    public enum WeaponClass { 
        None,
        Melee,
        Ranged,
        Magic
    }

}