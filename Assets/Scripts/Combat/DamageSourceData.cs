using Game.Combat.Elemental;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// The design numbers of one player damage source (a weapon hitbox, a projectile, an item's spawned
    /// object), in one asset instead of spread over the prefab's components. <see cref="DamageSourceBase"/>
    /// loads its damage, hit, pierce, knockback and debuff values from here; the prefab's projectile
    /// movement (<see cref="Projectiles.ProjectileMovementBase"/>) and lifetime components
    /// (<c>LifetimeByStat</c>, <c>DestroyAfterTime</c>) load the projectile values.
    ///
    /// These are defaults: a weapon or ability that fires the source can still override them at spawn
    /// (<see cref="DamageSourceBase.Initialize"/>, ability tier tables), and item stacking scales them.
    /// One asset per prefab, in Resources/Combat/DamageSources, named after the prefab.
    /// </summary>
    [CreateAssetMenu(fileName = "DamageSourceData", menuName = "Combat/Damage Source Data")]
    public class DamageSourceData : ScriptableObject
    {
        [Header("Damage")]
        [Tooltip("Damage before the player's damage stat is added.")]
        [Min(0)]
        [SerializeField] private int baseDamage = 5;
        [Tooltip("Which damage stat is added: Melee, Ranged or Magic Damage. None adds no stat.")]
        [SerializeField] private WeaponClass weaponClass = WeaponClass.None;
        [Tooltip("Share of that damage stat added to each hit (1 = the full stat, 0.5 = half).")]
        [Min(0f)]
        [SerializeField] private float attackScale = 1f;

        [Header("Hit Mode")]
        [Tooltip("Hit an enemy when it enters the hitbox.")]
        [SerializeField] private bool useTriggerEnter = true;
        [Tooltip("Hit an enemy on contact, then again every Tick Interval while it stays inside.")]
        [SerializeField] private bool useTriggerStay = false;
        [Tooltip("Seconds between hits on the same enemy while it stays inside (Trigger Stay only).")]
        [Min(0.01f)]
        [SerializeField] private float tickInterval = 1f;
        [Tooltip("Overlapping copies each deal their hits.")]
        [SerializeField] private bool shouldStackDamage = true;

        [Header("Pierce")]
        [Tooltip("Destroyed after hitting Pierce Count enemies. Off = hits every enemy it touches.")]
        [SerializeField] private bool enablePierce = false;
        [Min(1)]
        [SerializeField] private int pierceCount = 1;
        [Tooltip("Add the Pierce stat's extra targets to Pierce Count (for projectiles no weapon fires, e.g. Strafe arrows).")]
        [SerializeField] private bool addPierceStat = false;

        [Header("Knockback")]
        [SerializeField] private bool applyKnockback = false;
        [Min(0f)]
        [SerializeField] private float knockbackForce = 5f;

        [Header("Lifesteal")]
        [Tooltip("Hits can trigger Life Steal.")]
        [SerializeField] private bool canTriggerLifesteal = true;

        [Header("Debuff")]
        [Tooltip("Elemental debuff every hit tries to apply, besides the weapon's and the items'.")]
        [SerializeField] private ElementalDebuffData directElementalDebuffData;

        [Header("Projectile")]
        [Tooltip("Speed of the prefab's projectile movement. 0 = the prefab has none (a hitbox, an orbiting object).")]
        [Min(0f)]
        [SerializeField] private float projectileSpeed = 0f;
        [Tooltip("Seconds before it disappears. 0 = the prefab has no lifetime component.")]
        [Min(0f)]
        [SerializeField] private float lifetime = 0f;
        [Tooltip("Extra seconds of lifetime per point of Skill Duration (only for prefabs whose lifetime grows with it).")]
        [Min(0f)]
        [SerializeField] private float lifetimePerSkillDuration = 0f;

        public int BaseDamage => baseDamage;
        public WeaponClass WeaponClass => weaponClass;
        public float AttackScale => attackScale;
        public bool UseTriggerEnter => useTriggerEnter;
        public bool UseTriggerStay => useTriggerStay;
        public float TickInterval => tickInterval;
        public bool ShouldStackDamage => shouldStackDamage;
        public bool EnablePierce => enablePierce;
        public int PierceCount => pierceCount;
        public bool AddPierceStat => addPierceStat;
        public bool ApplyKnockback => applyKnockback;
        public float KnockbackForce => knockbackForce;
        public bool CanTriggerLifesteal => canTriggerLifesteal;
        public ElementalDebuffData DirectElementalDebuffData => directElementalDebuffData;
        public float ProjectileSpeed => projectileSpeed;
        public float Lifetime => lifetime;
        public float LifetimePerSkillDuration => lifetimePerSkillDuration;
    }
}
