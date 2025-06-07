using Game.AI;
using Game.Combat;
using Game.Common;
using Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Waves {
    public class WaveSpawner : Singleton<WaveSpawner>
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private List<EnemyPrefabEntry> enemyPrefabEntries;

        [Header("Spawn Area")]
        [SerializeField] private Vector2 minPosition = new Vector2(14, 5);
        [SerializeField] private Vector2 maxPosition = new Vector2(-13, -8);

        [Header("Wave Settings")]
        [SerializeField] private WaveDatabase waveDatabase;
        
        private Dictionary<EnemyType, GameObject> enemyPrefabs;

        // Events
        public event Action<int> OnWaveStarted;               // Sends wave number
        public event Action<float> OnWaveTimerUpdated;        // Sends remaining time
        public event Action<float> OnCooldownTimerUpdated;    // Sends cooldown time
        public event Action OnWaveComplete;                   // Triggered when wave ends
        public event Action OnWaveGroupFinished;              // Triggered when all waves are complete


        // Timers
        private float spawnCooldown = 1f; // per-enemy delay
        private float timeBetweenWaves = 20f;
        private int currentWaveIndex = -1;
        private float waveTimer;
        private float cooldownTimer;
        private bool waveInProgress = false;


        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine waveCoroutine;

        protected override void Awake()
        {
            base.Awake();

            enemyPrefabs = new Dictionary<EnemyType, GameObject>();
            foreach (var entry in enemyPrefabEntries)
            {
                if (!enemyPrefabs.ContainsKey(entry.type))
                {
                    enemyPrefabs.Add(entry.type, entry.prefab);
                }
            }
        }

        private void Start()
        {          
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            playerHealth.onDeath += OnPlayerDeathDisableWave;
            // Suscribe to OnGameplayResetRequested to reset the state after the game reloads
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested += ResetWaveSpawnerState;
            }
            StartNextWave();
        }

        private void OnDisable()
        {          
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= OnPlayerDeathDisableWave;
            }
            
            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayResetRequested -= ResetWaveSpawnerState;
            }
        }

        private void Update()
        {
            if (!waveInProgress || currentWaveIndex >= waveDatabase.waves.Count)
                return;

            waveTimer -= Time.deltaTime;
            OnWaveTimerUpdated?.Invoke(waveTimer);

            if (waveTimer <= 0f)
            {
                EndCurrentWave();
            }
        }

        private void ResetWaveSpawnerState()
        {
            waveTimer = 0f;
            cooldownTimer = 0f;
            currentWaveIndex = -1;
            waveInProgress = false;
            StartNextWave();
        }

        private void OnPlayerDeathDisableWave()
        {
            waveInProgress = false;
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
            }

            // Destroy all remaining enemies
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }

            activeEnemies.Clear();
        }

        private void StartNextWave()
        {
            currentWaveIndex++;

            if (currentWaveIndex >= waveDatabase.waves.Count)
            {              
                //Call the event to save data and load level selector scene
                OnWaveGroupFinished?.Invoke();
                return;
            }

            var waveData = waveDatabase.waves[currentWaveIndex];
            waveTimer = waveData.waveDuration;
            waveInProgress = true;

            OnWaveStarted?.Invoke(currentWaveIndex + 1);

            waveCoroutine = StartCoroutine(SpawnDuringWave(waveData));
        }

        private IEnumerator SpawnDuringWave(WaveData waveData)
        {
            while (waveInProgress)
            {
                var randomType = GetRandomEnemyFromPool(waveData.enemyPool);
                SpawnEnemy(randomType);
                yield return new WaitForSeconds(spawnCooldown);
            }
        }

        private void EndCurrentWave()
        {
            waveInProgress = false;

            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
            }

            // Destroy all remaining enemies
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }

            activeEnemies.Clear();
            OnWaveComplete?.Invoke();
        }

        public void ConfirmNextWave()
        {
            if (!waveInProgress && currentWaveIndex < waveDatabase.waves.Count)
            {                
                StartNextWave();
            }
        }

        private void SpawnEnemy(EnemyType type)
        {
            if (!enemyPrefabs.TryGetValue(type, out var prefab))
            {
                Debug.LogWarning($"No prefab found for enemy type: {type}");
                return;
            }

            Vector2 spawnPos = new Vector2(
                UnityEngine.Random.Range(minPosition.x, maxPosition.x),
                UnityEngine.Random.Range(minPosition.y, maxPosition.y)
            );

            GameObject enemyGO = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemyGO.transform.parent = transform; // Set parent to WaveSpawner
            Enemy enemyComponent = enemyGO.GetComponent<Enemy>();
            enemyComponent.Initialize(currentWaveIndex + 1);

            activeEnemies.Add(enemyGO);
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



    }
}
