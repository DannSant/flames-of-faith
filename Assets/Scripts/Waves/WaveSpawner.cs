using Game.AI;
using Game.Combat;
using Game.Common;
using Game.Control;
using Game.Enemies;
using Game.Misc;
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
        [SerializeField] private List<SpawnZone> spawnZones = new List<SpawnZone>();
        [SerializeField] private float minSpawnDistanceFromPlayer = 5f;

        [Header("Wave Settings")]
        [SerializeField] private WaveDatabase waveDatabase;
        [SerializeField] private float graceRemovedPerWave = 1f;
        [SerializeField] private bool endImmediately = false;

        [Header("Spawn prefabs")]
        [SerializeField] private EnemySpawnPortal spawnPortalPrefab;

        [Header("Testing Settings")]
        [SerializeField]
        private bool testMode = false;      

        private Dictionary<EnemyType, GameObject> enemyPrefabs;

        // Events
        public event Action<int> OnWaveStarted;               // Sends wave number
        public event Action<float> OnWaveTimerUpdated;        // Sends remaining time
        public event Action<float> OnCooldownTimerUpdated;    // Sends cooldown time
        public event Action OnWaveCompleteStarted;            // First part of wave complete process
        public event Action OnWaveCompleteEnded;               // Second part of wave complete process
        public event Action OnWaveGroupFinished;              // Triggered when all waves are complete
        public event Action OnAllLevelsFinished;               // Triggered when all waves are complete and the game should end


        // Timers
        //private float spawnCooldown = 1f; // per-enemy delay        
        private int currentWaveIndex = -1;
        private float waveTimer;
        
        private bool waveInProgress = false;
        private bool endingWave = false;


        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine waveCoroutine;
        private Coroutine waveEndingCoroutine;

        public int CurrentWaveIndex => currentWaveIndex;
        public bool WaveInProgress => waveInProgress;
        public bool EndingWave => endingWave;

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

            // If test mode is disabled, start the first wave normally. If test mode is enabled we should do nothing.
            if (!testMode)
            {
                StartNextWave();
            }         
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

        public void ReduceWaveTimer()
        {
            if (waveInProgress)
            {
                waveTimer = 1f;
            }
        }

        private void StartNextWave()
        {
            if (testMode) return;
            if (waveDatabase == null){               
                return; 
            }                      

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

            if(waveEndingCoroutine != null)
            {
                StopCoroutine(waveEndingCoroutine);
            }
            waveEndingCoroutine = StartCoroutine(EndCurrentWaveRoutine());

        }

        public void InvokeOnWaveComplete()
        {
            OnWaveCompleteEnded?.Invoke();
        }

        private IEnumerator EndCurrentWaveRoutine()
        {
            endingWave = true;
            // Invoke wave complete started event, this will make the player invulnerable so the cleanse animation can play safely
            OnWaveCompleteStarted?.Invoke();

            // Play cleanse animation          
            var playerVisual = PlayerManager.Instance.GetPlayerChildComponent<CharacterVisual>();
            if (playerVisual != null)
            {
                playerVisual.PlayCleanseAnimation();
            }

            // Wait for a short duration to allow the animation to play           
            yield return new WaitForSeconds(1.7f);

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
                {
                    var enemyKnockback = enemy.GetComponent<Knockback>();
                   
                    if (enemyKnockback != null)
                    {
                        float knockbackForce = UnityEngine.Random.Range(10f, 15f);
                        enemyKnockback.ApplyKnockback(playerVisual.transform, knockbackForce);
                    }

                    var enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(new DamageRequest(9999f, WeaponClass.None, false));
                    }
                }

            }
            activeEnemies.Clear();

            // Wait for the enemies to be fully destroyed          
            yield return new WaitForSeconds(1f);


            //Clear all projectiles in the scene
            var projectiles = FindObjectsByType<EnemyTriggerDamage>(FindObjectsSortMode.None);
            foreach (var proj in projectiles)
            {
                Destroy(proj.gameObject);
            }

            // Wait for the enemies to be fully destroyed          
            yield return new WaitForSeconds(1f);

            //Invoke end wave complete event           
            InvokeOnWaveComplete();
            endingWave = false;
        }

        public void ConfirmNextWave()
        { 
            if (endImmediately)
            {
                GoToNextLevel();
                return;
            }

            if (waveDatabase == null) { return; }

            if (!waveInProgress && currentWaveIndex < waveDatabase.waves.Count)
            {                
                StartNextWave();
            }
        }

        public void SpawnEnemy(EnemyType type)
        {
            if (PlayerManager.Instance.IsPlayerOnMap)
            {
                return;
            }
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

            EnemySpawnPortal portal = Instantiate(spawnPortalPrefab, spawnPos, Quaternion.identity);
            portal.Initialize(new SpawnInfo
            {
                EnemyToSpawn = prefab,
                SpawnPosition = spawnPos,
                SpawnRotation = Quaternion.identity,
                WaveSpawnerTransform = this.transform,
                WaveNumber = currentWaveIndex + 1
            });
            portal.onEnemySpawnedEvent += OnEnemySpawned;
        }

        private void OnEnemySpawned(EnemySpawnPortal portal,GameObject enemy)
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

        public void Cleanup()
        {
            StopAllCoroutines();
            endingWave = false;
            waveInProgress = false;
            waveCoroutine = null;
            waveEndingCoroutine = null;
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
