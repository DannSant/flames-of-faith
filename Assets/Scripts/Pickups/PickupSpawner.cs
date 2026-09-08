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

        [Header("Overlap Avoidance")]
        [Tooltip("Radius used to check whether a spot is already occupied by another pickup.")]
        [SerializeField] private float occupiedCheckRadius = 0.5f;
        [Tooltip("Max distance from the death position to try when the original spot is occupied.")]
        [SerializeField] private float scatterRadius = 1f;
        [Tooltip("How many random spots to try before giving up and forcing a placement.")]
        [SerializeField] private int maxPlacementAttempts = 3;

        private EnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        // Subscribed in Start (not OnEnable) so this always runs after BehaviorController's death
        // behaviors - both listen to the same event, Start() runs in component order, and
        // BehaviorController is listed earlier on EnemyBase.prefab. That means the experience token
        // drop (if any) already exists in the scene by the time we check for occupied spots below.
        private void Start()
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
                Instantiate(selectedPickup, FindSpawnPosition(), Quaternion.identity);
            }
        }

        /// <summary>
        /// Prefers the exact death position, and only steps away from it if something is already
        /// there. Generic to any IWorldPickup, so this avoids stacking on an experience token, on
        /// another PickupSpawner's drop, or on any future pickup type - no special-casing needed.
        /// </summary>
        private Vector3 FindSpawnPosition()
        {
            Vector3 origin = transform.position;

            if (!IsOccupied(origin))
                return origin;

            Vector3 candidate = origin;
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                candidate = origin + (Vector3)(Random.insideUnitCircle * scatterRadius);
                if (!IsOccupied(candidate))
                    return candidate;
            }

            // Every attempt was occupied - settle for the last spot, nudged so it isn't a dead stack.
            return candidate + (Vector3)(Random.insideUnitCircle.normalized * occupiedCheckRadius);
        }

        private bool IsOccupied(Vector3 position)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, occupiedCheckRadius);
            foreach (var hit in hits)
            {
                if (hit.GetComponent<IWorldPickup>() != null)
                    return true;
            }
            return false;
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
