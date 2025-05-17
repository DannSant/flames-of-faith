using Game.AI;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Waves {
    [System.Serializable]
    public class WaveEnemyPoolEntry
    {
        public EnemyType type;
        public int weight = 1; // higher = more likely
    }

    [CreateAssetMenu(fileName = "NewWave", menuName = "Waves/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Tooltip("Duration of the wave in seconds.")]
        public float waveDuration = 30f;

        [Tooltip("Cooldown duration after wave ends.")]
        public float cooldownDuration = 20f;

        [Tooltip("Enemy types and how many to spawn.")]
        public List<EnemySpawnInfo> enemies = new();

        public List<WaveEnemyPoolEntry> enemyPool;

        [Serializable]
        public class EnemySpawnInfo
        {
            public EnemyType type;
            public int amount;
        }

        public int GetTotalEnemies()
        {
            int total = 0;
            foreach (var e in enemies)
            {
                total += e.amount;
            }
            return total;
        }
    }
}