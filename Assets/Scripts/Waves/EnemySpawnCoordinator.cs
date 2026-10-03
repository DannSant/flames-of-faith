using Game.AI;
using Game.Combat;
using Game.Misc;
using Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Waves
{
    public class EnemySpawnCoordinator : MonoBehaviour
    {
        private Dictionary<EnemyType, GameObject> enemyPrefabs;
        private List<SpawnZone> spawnZones;
        private float minSpawnDistanceFromPlayer;
        private EnemySpawnPortal spawnPortalPrefab;
        private Transform waveSpawnerTransform;

        private readonly List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine spawnCoroutine;
        private WaveData currentWaveData;

        public void Initialize(Dictionary<EnemyType, GameObject> enemyPrefabs, List<SpawnZone> spawnZones,
            float minSpawnDistanceFromPlayer, EnemySpawnPortal spawnPortalPrefab, Transform waveSpawnerTransform)
        {
            this.enemyPrefabs = enemyPrefabs;
            this.spawnZones = spawnZones;
            this.minSpawnDistanceFromPlayer = minSpawnDistanceFromPlayer;
            this.spawnPortalPrefab = spawnPortalPrefab;
            this.waveSpawnerTransform = waveSpawnerTransform;
        }

        public void StartSpawning(WaveData waveData, int waveNumber)
        {
            StopSpawning();
            currentWaveData = waveData;
            spawnCoroutine = StartCoroutine(SpawnDuringWaveRoutine(waveData, waveNumber));
        }

        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnDuringWaveRoutine(WaveData waveData, int waveNumber)
        {
            while (true)
            {
                var randomType = GetRandomEnemyFromPool(waveData.enemyPool);
                SpawnEnemy(randomType, waveNumber);
                float spawnCooldown = activeEnemies.Count >= waveData.amountOfEnemiesWithLongCooldown ? waveData.regularCooldown : waveData.longCooldown;
                yield return new WaitForSeconds(spawnCooldown);
            }
        }

        /// <summary>
        /// Requests a spawn through a portal. Returns false when the spawn could not be requested.
        /// onSpawned is called once the enemy comes out of the portal.
        /// </summary>
        public bool SpawnEnemy(EnemyType type, int waveNumber, Action<GameObject> onSpawned = null)
        {
            if (PlayerManager.Instance.IsPlayerOnMap)
            {
                return false;
            }
            if (!enemyPrefabs.TryGetValue(type, out var prefab) || prefab == null)
            {
                Debug.LogWarning($"No prefab found for enemy type: {type}");
                return false;
            }

            var validZones = spawnZones.Where(zone => !zone.IsPlayerInside).ToList();

            if (validZones.Count == 0)
            {
                Debug.LogWarning("No valid spawn zones available!");
                return false;
            }

            Vector2 playerPos = PlayerManager.Instance.transform.position;

            Vector2 spawnPos = Vector2.zero;
            int attempts = 0;
            const int maxAttempts = 10;
            bool found = false;

            while (!found && attempts < maxAttempts)
            {
                var zone = validZones[UnityEngine.Random.Range(0, validZones.Count)];

                spawnPos = zone.GetRandomPointInside();
                if (Vector2.Distance(spawnPos, playerPos) > minSpawnDistanceFromPlayer)
                {
                    found = true;
                }
                attempts++;
            }

            if (!found)
            {
                Debug.LogWarning("Could not find spawn point far enough from player. Skipping this spawn.");
                return false;
            }

            EnemySpawnPortal portal = Instantiate(spawnPortalPrefab, spawnPos, Quaternion.identity);
            portal.Initialize(new SpawnInfo
            {
                EnemyToSpawn = prefab,
                SpawnPosition = spawnPos,
                SpawnRotation = Quaternion.identity,
                WaveSpawnerTransform = waveSpawnerTransform,
                WaveNumber = waveNumber,
                IsCorrupted = RollCorrupted()
            });
            portal.onEnemySpawnedEvent += OnEnemySpawned;

            if (onSpawned != null)
            {
                Action<EnemySpawnPortal, GameObject> spawnedHandler = null;
                spawnedHandler = (spawnPortal, enemy) =>
                {
                    spawnPortal.onEnemySpawnedEvent -= spawnedHandler;
                    onSpawned(enemy);
                };
                portal.onEnemySpawnedEvent += spawnedHandler;
            }
            return true;
        }

        // The lower the player's Grace goes below 0, the more likely each spawned enemy is Corrupted
        private bool RollCorrupted()
        {
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace == null)
            {
                return false;
            }

            float waveMultiplier = currentWaveData != null ? currentWaveData.corruptedChanceMultiplier : 1f;
            float chance = CorruptionSettings.Instance.GetCorruptedSpawnChance(playerGrace.CorruptedLevel, waveMultiplier);
            return chance > 0f && UnityEngine.Random.value < chance;
        }

        private void OnEnemySpawned(EnemySpawnPortal portal, GameObject enemy)
        {
            activeEnemies.Add(enemy);
            portal.onEnemySpawnedEvent -= OnEnemySpawned;
        }

        private EnemyType GetRandomEnemyFromPool(List<WaveEnemyPoolEntry> pool)
        {
            int totalWeight = pool.Sum(e => e.weight);
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var entry in pool)
            {
                cumulative += entry.weight;
                if (roll < cumulative)
                    return entry.type;
            }

            return pool[0].type; // fallback
        }

        public void KillAllTrackedEnemiesWithEffects(Transform knockbackOrigin)
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null) continue;

                var enemyKnockback = enemy.GetComponent<Knockback>();
                if (enemyKnockback != null)
                {
                    float knockbackForce = UnityEngine.Random.Range(10f, 15f);
                    enemyKnockback.ApplyKnockback(knockbackOrigin, knockbackForce);
                }

                var enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(new DamageRequest(9999f, WeaponClass.None, false) { silent = true });
                }
            }
            activeEnemies.Clear();
        }

        public void DestroyAllTrackedEnemiesSilently()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }
            activeEnemies.Clear();
        }

        public void StopAllSpawningAndTracking()
        {
            StopSpawning();
            activeEnemies.Clear();
        }
    }
}
