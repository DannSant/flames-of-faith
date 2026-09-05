using Game.AI;
using Game.Combat;
using Game.Common;
using Game.Enemies;
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Waves {
    public class WaveSpawner : Singleton<WaveSpawner>
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

        private EnemySpawnCoordinator enemySpawnCoordinator;
        private WaveEndSequenceController waveEndSequenceController;
        private WaveLifecycleHandler waveLifecycleHandler;

        // Events
        public event Action<int> OnWaveStarted;               // Sends wave number
        public event Action<float> OnWaveTimerUpdated;        // Sends remaining time
        public event Action OnWaveCompleteStarted;            // First part of wave complete process
        public event Action OnWaveCompleteEnded;               // Second part of wave complete process
        public event Action OnWaveGroupFinished;              // Triggered when all waves are complete
        public event Action OnAllLevelsFinished;               // Triggered when all waves are complete and the game should end


        // Timers
        private int currentWaveIndex = -1;
        private float waveTimer;

        private bool waveInProgress = false;

        public int CurrentWaveIndex => currentWaveIndex;
        public bool WaveInProgress => waveInProgress;
        public bool EndingWave => waveEndSequenceController != null && waveEndSequenceController.EndingWave;

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

            enemySpawnCoordinator = gameObject.AddComponent<EnemySpawnCoordinator>();
            enemySpawnCoordinator.Initialize(enemyPrefabs, spawnZones, minSpawnDistanceFromPlayer, spawnPortalPrefab, transform);

            waveEndSequenceController = gameObject.AddComponent<WaveEndSequenceController>();
            waveEndSequenceController.Initialize(graceRemovedPerWave, enemySpawnCoordinator);
            waveEndSequenceController.OnWaveCompleteStarted += () => OnWaveCompleteStarted?.Invoke();
            waveEndSequenceController.OnWaveCompleteEnded += () => OnWaveCompleteEnded?.Invoke();

            waveLifecycleHandler = gameObject.AddComponent<WaveLifecycleHandler>();
            waveLifecycleHandler.Initialize(enemySpawnCoordinator, waveEndSequenceController);
        }

        private void Start()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += StopWaveProgressionOnDeath;
            }
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
                playerHealth.onDeath -= StopWaveProgressionOnDeath;
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

        private void StopWaveProgressionOnDeath()
        {
            waveInProgress = false;
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

            enemySpawnCoordinator.StartSpawning(waveData, currentWaveIndex + 1);
        }

        private void EndCurrentWave()
        {
            waveInProgress = false;

            enemySpawnCoordinator.StopSpawning();
            waveEndSequenceController.BeginEndSequence();
        }

        public void InvokeOnWaveComplete()
        {
            waveEndSequenceController.InvokeOnWaveComplete();
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
            enemySpawnCoordinator.SpawnEnemy(type, currentWaveIndex + 1);
        }

    }
}
