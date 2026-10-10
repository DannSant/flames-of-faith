using Game.AI;
using UnityEngine;

namespace Game.Enemies
{
    [CreateAssetMenu(menuName = "Enemies/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public EnemyType enemyType;
        public GameObject enemyPrefab;
        public int healthBase = 10;
        [Tooltip("Damage tier of touch and melee attacks (see EnemyDamageSettings). " +
            "Tier N kills a player with the expected stats in the tier's number of hits. Fractions interpolate; 0 = no damage.")]
        [Min(0f)]
        public float contactDamageTier = 1f;
        [Tooltip("Damage tier of projectiles and explosions this enemy spawns. 0 = no damage.")]
        [Min(0f)]
        public float projectileDamageTier = 1f;
        public float speedBase = 1f;
        public float attackRangeBase = 1f;
        [Tooltip("Highest experience token denomination this enemy can drop (see ExperienceSettings). 1 = regular tokens only.")]
        [Min(1)]
        public int xpTier = 1;
        public int healthPerWave = 30; // Base health increase per wave
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