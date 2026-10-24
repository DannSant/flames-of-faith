using Game.Combat;
using Game.Loot;
using Game.Scene;
using UnityEngine;

namespace Game.Pickups
{
    /// <summary>
    /// Drops on an enemy's death: sometimes a random item or a health pickup (chance and split on LootSettings, the
    /// balance sheet's Loot tab), and separately a Grace pickup.
    /// </summary>
    public class PickupSpawner : MonoBehaviour
    {
        [Header("Pickups")]
        [Tooltip("Dropped as the item share of LootSettings' enemy drops; it rolls its item when picked up.")]
        [SerializeField] private RandomItemPickup itemPickupPrefab;
        [Tooltip("Dropped as the rest of LootSettings' enemy drops.")]
        [SerializeField] private BasePickup healthPickupPrefab;

        [Header("Grace Pickup")]
        [Tooltip("Rolled separately from the pool above, with a chance based on the player's Grace Affinity (see CorruptionSettings).")]
        [SerializeField] private BasePickup gracePickupPrefab;
        [SerializeField] private bool canDropGracePickup = true;

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
            TrySpawnDrop();
            TrySpawnGracePickup();
        }

        private void TrySpawnDrop()
        {
            var settings = LootSettings.Instance;
            if (Random.value >= settings.EnemyDropChance)
                return;

            // An item pickup only while some item can drop from enemies (min levels beaten can hold them all back early)
            BasePickup selected = Random.value < settings.EnemyItemShare && ItemProvider.CanDrop(LootSource.EnemyPickup)
                ? itemPickupPrefab
                : healthPickupPrefab;
            if (selected != null)
            {
                Instantiate(selected, FindSpawnPosition(), Quaternion.identity);
            }
        }

        // Runs after the pool roll, so FindSpawnPosition steps aside if both drop on the same death
        private void TrySpawnGracePickup()
        {
            if (!canDropGracePickup || gracePickupPrefab == null || PlayerManager.Instance == null)
                return;

            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            float affinity = playerGrace != null ? playerGrace.GraceAffinity : 0f;
            if (Random.value < CorruptionSettings.Instance.GetGracePickupChance(affinity))
            {
                Instantiate(gracePickupPrefab, FindSpawnPosition(), Quaternion.identity);
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
    }
}
