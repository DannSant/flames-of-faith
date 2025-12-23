using Game.AI;
using Game.Combat;
using Game.Common;
using Game.Enemies;
using Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Waves {
    public class WaveSpawner : Singleton<WaveSpawner>, ISceneCleanupHandler
    {

        [Header("Spawn Area")]
        //[SerializeField] private Vector2 minPosition = new Vector2(14, 5);
        //[SerializeField] private Vector2 maxPosition = new Vector2(-13, -8);
        [SerializeField] private List<SpawnZone> spawnZones = new List<SpawnZone>();
        [SerializeField] private float minSpawnDistanceFromPlayer = 5f;

        [Header("Wave Settings")]
        [SerializeField] private WaveDatabase waveDatabase;
        [SerializeField] private float graceRemovedPerWave = 1f;

        [Header("Testing Settings")]
        [SerializeField]
        private bool testMode = false;
       // [SerializeField] private bool shortDurationWaves = false;

        private Dictionary<EnemyType, GameObject> enemyPrefabs;

        // Events
        public event Action<int> OnWaveStarted;               // Sends wave number
        public event Action<float> OnWaveTimerUpdated;        // Sends remaining time
        public event Action<float> OnCooldownTimerUpdated;    // Sends cooldown time
        public event Action OnWaveComplete;                   // Triggered when wave ends
        public event Action OnWaveGroupFinished;              // Triggered when all waves are complete
        public event Action OnAllLevelsFinished;               // Triggered when all waves are complete and the game should end


        // Timers
        //private float spawnCooldown = 1f; // per-enemy delay        
        private int currentWaveIndex = -1;
        private float waveTimer;
        
        private bool waveInProgress = false;


        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine waveCoroutine;

        public int CurrentWaveIndex => currentWaveIndex;

        protected override void Awake()
        {
            base.Awake();
            var enemyDatabase = EnemyDatabaseProvider.Instance.EnemyDatabase;
            enemyPrefabs = new Dictionary<EnemyType, GameObject>();
            foreach (var entry in enemyDatabase.GetAllEnemies())
            {
                if (!enemyPrefabs.ContainsKey(entry.Key))
                {
                    enemyPrefabs.Add(entry.Key, entry.Value.enemyPrefab);
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
                MainSceneController.Instance.OnGameplayInitialSetup += ResetWaveSpawnerState;
            }

            if (waveDatabase == null)
            {               
                return;
            }

            if (testMode)
            {
                SpawnTestEnemies();
            }
            else
            {
                StartNextWave();
            }
           
        }

        private void SpawnTestEnemies()
        {            
          
           

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
                MainSceneController.Instance.OnGameplayInitialSetup -= ResetWaveSpawnerState;
            }
        }

        private void Update()
        {
            if (testMode) return;

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

        public void GoToNextLevel()
        {
            if (waveDatabase!=null && waveDatabase.lastWave)
            {
                //Show endscreen
                OnAllLevelsFinished?.Invoke();
            }else
            {
                // Save state and load level selector scene
                OnWaveGroupFinished?.Invoke();
            }
        }

        private void StartNextWave()
        {
            if (testMode) return;
            if (waveDatabase == null){ return; }

            

            currentWaveIndex++;

            if (currentWaveIndex >= waveDatabase.waves.Count)
            {
                //Call the event to save data and load level selector scene
                GoToNextLevel();
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
                float spawnCooldown = activeEnemies.Count >= waveData.amountOfEnemiesWithLongCooldown ? waveData.regularCooldown : waveData.longCooldown;
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

            // Reduce grace when wave ends
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.RemoveGrace(graceRemovedPerWave); // Remove 1 grace point
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
            if (waveDatabase == null) { return; }
            if (!waveInProgress && currentWaveIndex < waveDatabase.waves.Count)
            {                
                StartNextWave();
            }
        }

        public void SpawnEnemy(EnemyType type)
        {
            if (!enemyPrefabs.TryGetValue(type, out var prefab))
            {
                Debug.LogWarning($"No prefab found for enemy type: {type}");
                return;
            }

            var validZones = spawnZones.Where(zone => !zone.IsPlayerInside).ToList();

            if (validZones.Count == 0)
            {
                Debug.LogWarning("No valid spawn zones available!");
                return;
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
                Debug.LogWarning("Could not find spawn point far enough from player.");
            }

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

        public void Cleanup()
        {
            StopAllCoroutines();
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= OnPlayerDeathDisableWave;
            }

            if (MainSceneController.Instance != null)
            {
                MainSceneController.Instance.OnGameplayInitialSetup -= ResetWaveSpawnerState;
            }
            activeEnemies.Clear();
            Destroy(gameObject);
        }

    }
}
