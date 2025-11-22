using Game.Combat;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Pickups
{
    [System.Serializable]
    public class PickupEntry
    {
        public BasePickup pickupPrefab;
        [Range(0f, 1f)] public float weight = 1f;
    }

    public class PickupSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float spawnChance = 0.1f;

        [Header("Pickup Pool")]
        [SerializeField] private List<PickupEntry> pickups = new List<PickupEntry>();

        private EnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
                enemyHealth.onDeath += OnEnemyDeath;
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
                enemyHealth.onDeath -= OnEnemyDeath;
        }

        private void OnEnemyDeath()
        {
            // Roll for spawn chance
            if (Random.value > spawnChance || pickups.Count == 0)
                return;

            // Choose pickup using weighted probability
            BasePickup selectedPickup = GetWeightedPickup();

            if (selectedPickup != null)
            {
                Instantiate(selectedPickup, transform.position, Quaternion.identity);
            }
        }

        private BasePickup GetWeightedPickup()
        {
            float totalWeight = pickups.Sum(p => p.weight);
            float randomRoll = Random.value * totalWeight;
            float cumulative = 0f;

            foreach (var entry in pickups)
            {
                cumulative += entry.weight;
                if (randomRoll <= cumulative)
                    return entry.pickupPrefab;
            }

            return null;
        }
    }
}
