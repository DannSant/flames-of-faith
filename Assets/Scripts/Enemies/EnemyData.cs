using Game.AI;
using UnityEngine;

namespace Game.Enemies
{
    [CreateAssetMenu(menuName = "Enemies/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public EnemyType enemyType;
        public GameObject enemyPrefab;
        [Tooltip("Health at wave 1 of a level, before any growth. Max health = this + (wave - 1) x Health Per Wave + levels beaten x Health Per Level.")]
        public int healthBase = 10;
        [Tooltip("Damage tier of touch and melee attacks (see EnemyDamageSettings). " +
            "Tier N kills a player with the expected stats in the tier's number of hits. Fractions interpolate; 0 = no damage.")]
        [Min(0f)]
        public float contactDamageTier = 1f;
        [Tooltip("Damage tier of projectiles and explosions this enemy spawns. 0 = no damage.")]
        [Min(0f)]
        public float projectileDamageTier = 1f;
        [Tooltip("Walking speed in units per second, for enemies that move continuously (chasers, DrekoEye). " +
            "0 for enemies that only hop.")]
        [Min(0f)]
        public float moveSpeed = 1f;
        [Tooltip("Push applied on each hop, for enemies that move in hops timed by their animation (slimes). " +
            "0 for enemies that walk.")]
        [Min(0f)]
        public float hopForce = 0f;
        [Tooltip("Highest experience token denomination this enemy can drop (see ExperienceSettings). 1 = regular tokens only.")]
        [Min(1)]
        public int xpTier = 1;
        [Tooltip("Health added for each wave after the first in a level.")]
        public int healthPerWave = 30; // Base health increase per wave
        [Tooltip("Health added for each level the player has beaten in the run.")]
        public int healthPerLevel = 40; // Health increase per level
        [Tooltip("Always spawns Corrupted, regardless of the player's Grace (e.g. Corruptors).")]
        public bool alwaysCorrupted = false;
        [Tooltip("Use corruptedHealthBonus instead of the global value from CorruptionSettings.")]
        public bool overrideCorruptedHealthBonus = false;
        [Tooltip("Extra health while Corrupted (1 = +100%). Only used when overriding.")]
        public float corruptedHealthBonus = 1f;

        public float GetDamageTier(EnemyDamageKind kind)
        {
            return kind == EnemyDamageKind.Projectile ? projectileDamageTier : contactDamageTier;
        }

        public float GetCorruptedHealthBonus(float globalBonus)
        {
            return overrideCorruptedHealthBonus ? corruptedHealthBonus : globalBonus;
        }
    }

}