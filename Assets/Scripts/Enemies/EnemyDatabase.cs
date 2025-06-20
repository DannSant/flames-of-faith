using Game.AI;
using Game.Enemies;

using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemies
{
    [CreateAssetMenu(menuName = "Enemies/EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        public List<EnemyData> enemies = new();
        private Dictionary<EnemyType, EnemyData> enemyDictionary;

        /// <summary>
        /// Initializes the internal dictionary based on the list of EnemyData.
        /// </summary>
        public void Initialize()
        {
            enemyDictionary = new Dictionary<EnemyType, EnemyData>();

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                if (!enemyDictionary.ContainsKey(enemy.enemyType))
                {
                    enemyDictionary.Add(enemy.enemyType, enemy);
                }
                else
                {
                    Debug.LogWarning($"Duplicate enemy type found in EnemyDatabase: {enemy.enemyType}. Ignoring duplicate.");
                }
            }
        }

        /// <summary>
        /// Gets the EnemyData associated with a given EnemyType.
        /// </summary>
        /// <param name="type">The enemy type to look up.</param>
        /// <returns>The corresponding EnemyData, or null if not found.</returns>
        public EnemyData GetEnemyData(EnemyType type)
        {
            if (enemyDictionary == null)
            {
                Debug.LogWarning("EnemyDatabase not initialized. Call Initialize() before accessing data.");
                return null;
            }

            if (enemyDictionary.TryGetValue(type, out var data))
            {
                return data;
            }

            Debug.LogWarning($"Enemy type '{type}' not found in EnemyDatabase.");
            return null;
        }

        public Dictionary<EnemyType, EnemyData> GetAllEnemies()
        {
            if (enemyDictionary == null)
            {
                Debug.LogWarning("EnemyDatabase not initialized. Call Initialize() before accessing data.");
                return null;
            }
            return new Dictionary<EnemyType, EnemyData>(enemyDictionary);
        }
    }

}