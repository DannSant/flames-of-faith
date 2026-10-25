using Game.Combat;
using UnityEngine;

namespace Game.Effects
{
    /// <summary>
    /// An item behavior that spawns something that can deal damage: its prefab and the damage that prefab deals.
    /// <see cref="EffectBehavior.SpawnEffectObject"/> pushes <see cref="Damage"/> into every damage source on the
    /// spawned object, so the item, not the prefab, decides how hard it hits (the sheet's ItemBehaviors_ tabs).
    /// </summary>
    public abstract class SpawningEffectBehavior : EffectBehavior
    {
        [Header("Spawn")]
        [Tooltip("What the behavior spawns.")]
        [SerializeField] protected GameObject prefabToSpawn;
        [Tooltip("The damage of what it spawns: base damage, the damage stat it scales with, and how much of that stat.")]
        [SerializeField] protected DamageProfile damage;

        public GameObject PrefabToSpawn => prefabToSpawn;
        public DamageProfile Damage => damage;

        protected override DamageProfile? SpawnDamage => damage;
    }
}
