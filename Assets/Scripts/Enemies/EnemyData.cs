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
        public int damageBase = 1;
        public int damagePerWave = 1;
        public int damagePerLevel = 2;
        public float speedBase = 1f;
        public float attackRangeBase = 1f;
        public int xpBase = 1;
        public int xpPerLevel = 1;
        public int projectileDamageBase;
        public int projectileDamagePerWave;
        public int healthPerWave = 30; // Base health increase per wave
        public int healthPerLevel = 40; // Health increase per level
        [Tooltip("Always spawns Corrupted, regardless of the player's Grace (e.g. Corruptors).")]
        public bool alwaysCorrupted = false;
    }

}