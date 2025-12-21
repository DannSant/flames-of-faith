using Game.Combat.Elemental;
using Game.Combat.Projectiles;
using UnityEngine;

namespace Game.Combat {

    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
       
        [Tooltip("Time between attacks.")]
        public float attackCooldownBase = 1f;

        [Tooltip("Initial damage")]
        public int baseDamage = 2;

        [Tooltip("For each MeleeDamage or RangeDamage stat, damage will increase by this number ")]
        public int attackScale = 1;

        [Tooltip("For each AttackSpeed stat, regular damage will increase by this number ")]
        public float attackSpeedScale = 0.0075f;

        [Tooltip("Depending on the weapon class damage will scale diferently. Example Melee weapons only scale if damage is done through a weapon collider instead of a projectile")]
        public WeaponClass weaponClass = WeaponClass.Melee;

        [Tooltip("Initial range")]
        public float rangeBase = 2;

        [Tooltip("Prefab of the projectile if applicable")]
        public GameObject projectilePrefab;

        [Tooltip("Amount of targets the projectile can pierce before gets destroyed")]
        public int pierceAmount = 1;

        [Tooltip("Knockback force when damage is inflicted")]
        public float knockbackForce = 15f;
        [Tooltip("Wether the attack should apply knockback force")]
        public bool shouldApplyKnockback = true;

        [Tooltip("Amount of grace generated when damage is inflicted")]
        public int graceGenerated = 1;

        [Tooltip("Inflict an elemental type debuff")]
        public ElementalDebuffData elementalDebuffData;

        [Tooltip("Amount of projectiles to launch in special attack (if applicable)")]
        public int projectileAmount = 16;

    }

    public enum WeaponClass { 
        None,
        Melee,
        Ranged,
        Magic
    }

}