using Game.Saving;
using Game.AI;
using Game.Combat;
using Game.Common;
using Game.Enemies;
using Game.Progression;
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
        private CorruptorPhaseController corruptorPhaseController;

        // Events
        public event Action<int> OnWaveStarted;               // Sends wave number
        public event Action<float> OnWaveTimerUpdated;        // Sends remaining time
        public event Action OnWaveCompleteStarted;            // First part of wave complete process
        public event Action OnWaveCompleteEnded;               // Second part of wave complete process
        public event Action OnWaveGroupFinished;              // Triggered when all waves are complete
        public event Action OnAllLevelsFinished;               // Triggered when all waves are complete and the game should end
        public event Action<Transform> OnCorruptorSpawned;    // Sends the Corruptor so the UI can point at it
        public event Action<float, Transform> OnCorruptorPhaseStarted;   // Sends the Corruptor phase duration and the player
        public event Action<int> OnCorruptorCorruptionChanged;          // Corruption the Corruptor grants if killed now
        public event Action<float> OnCorruptorTimerUpdated;   // Sends the remaining Corruptor time
        public event Action OnCorruptorPhaseEnded;
        // The UI calls the Action once it finished presenting the result; the end of wave sequence waits for it
        public event Action<WaveCorruptionResult, Action> OnWaveCorruptionResolved;


        // Timers
        private int currentWaveIndex = -1;
        private float waveTimer;

        private bool waveInProgress = false;

        // Experience
        private WaveExperienceEstimate currentWaveExperience;
        private int experienceDropsThisWave;
        private float experienceDroppedThisWave;

        public int CurrentWaveIndex => currentWaveIndex;
        public bool WaveInProgress => waveInProgress;
        public bool EndingWave => waveEndSequenceController != null && waveEndSequenceController.EndingWave;
        // XP of a regular experience token during the current wave. 0 when no wave has started.
        public float CurrentWaveBaseXp => currentWaveExperience.BaseXp;

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
            waveEndSequenceController.Initialize(enemySpawnCoordinator);
            waveEndSequenceController.OnWaveCompleteStarted += () => OnWaveCompleteStarted?.Invoke();
            waveEndSequenceController.OnWaveCompleteEnded += () =>
            {
                // Logged here so it includes the Corruptor and the enemies killed by the end sequence
                LogWaveExperience();
                OnWaveCompleteEnded?.Invoke();
            };
            waveEndSequenceController.OnWaveCorruptionResolved += (result, onPresented) =>
            {
                if (OnWaveCorruptionResolved != null)
                {
                    OnWaveCorruptionResolved.Invoke(result, onPresented);
                }
                else
                {
                    onPresented();
                }
            };

            corruptorPhaseController = gameObject.AddComponent<CorruptorPhaseController>();
            corruptorPhaseController.Initialize(enemySpawnCoordinator);
            corruptorPhaseController.OnCorruptorSpawned += corruptor => OnCorruptorSpawned?.Invoke(corruptor);
            corruptorPhaseController.OnPhaseStarted += (duration, player) => OnCorruptorPhaseStarted?.Invoke(duration, player);
            corruptorPhaseController.OnCorruptionChanged += corruption => OnCorruptorCorruptionChanged?.Invoke(corruption);
            corruptorPhaseController.OnTimerUpdated += remaining => OnCorruptorTimerUpdated?.Invoke(remaining);
            corruptorPhaseController.OnPhaseEnded += () => OnCorruptorPhaseEnded?.Invoke();

            waveLifecycleHandler = gameObject.AddComponent<WaveLifecycleHandler>();
            waveLifecycleHandler.Initialize(enemySpawnCoordinator, waveEndSequenceController, corruptorPhaseController);
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

            // PlayerHealth.onDeath only fires after the death animation, so stop the timer as soon as the player dies
            if (IsPlayerDead())
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
                RunSaveService.DeleteSave();
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

            // Corruption only lives for one wave
            var playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            if (playerCorruption != null)
            {
                playerCorruption.ResetCorruption();
            }

            CalculateWaveExperience(waveData);

            OnWaveStarted?.Invoke(currentWaveIndex + 1);

            enemySpawnCoordinator.StartSpawning(waveData, currentWaveIndex + 1);
        }

        private void EndCurrentWave()
        {
            waveInProgress = false;

            enemySpawnCoordinator.StopSpawning();

            var waveData = waveDatabase.waves[currentWaveIndex];
            if (waveData.spawnCorruptor)
            {
                var corruptorType = waveData.overrideCorruptor ? waveData.corruptorOverride : waveDatabase.corruptorType;
                corruptorPhaseController.BeginPhase(corruptorType, currentWaveIndex + 1, OnCorruptorPhaseFinished);
            }
            else
            {
                waveEndSequenceController.BeginEndSequence(CorruptorResult.None);
            }
        }

        private void OnCorruptorPhaseFinished(CorruptorResult result)
        {
            if (IsPlayerDead())
            {
                return;
            }

            var playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            if (playerCorruption != null)
            {
                playerCorruption.SetCorruptorCorruption(result.corruption);
            }

            waveEndSequenceController.BeginEndSequence(result);
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

        private void CalculateWaveExperience(WaveData waveData)
        {
            experienceDropsThisWave = 0;
            experienceDroppedThisWave = 0f;
            currentWaveExperience = default;

            var playerExperience = PlayerManager.Instance.GetPlayerComponent<PlayerExperience>();
            if (playerExperience == null)
            {
                return;
            }

            var playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>();
            float reductionPoints = playerProgression != null ? playerProgression.GetStatTotal(StatType.ExperienceToLevelUpReduction) : 0f;

            currentWaveExperience = WaveExperienceCalculator.CalculateBaseXp(waveData, playerExperience.GetCurrentLevel(), reductionPoints,
                playerExperience, ExperienceSettings.Instance);
        }

        public void RegisterExperienceDrop(float amount)
        {
            experienceDropsThisWave++;
            experienceDroppedThisWave += amount;
        }

        private void LogWaveExperience()
        {
            if (!ExperienceSettings.Instance.LogWaveXpDebug)
            {
                return;
            }

            var e = currentWaveExperience;
            Debug.Log($"[WaveXP] Wave {currentWaveIndex + 1}: expected spawns {e.ExpectedSpawns:F1} (actual {enemySpawnCoordinator.SpawnedThisWave}), " +
                      $"expected drops {e.ExpectedDrops:F1} (actual {experienceDropsThisWave}), base XP {e.BaseXp:F2}, " +
                      $"target XP {e.TargetXp:F1}, dropped XP {experienceDroppedThisWave:F1} ({experienceDroppedThisWave / Mathf.Max(1f, e.TargetXp):P0} of target)");
        }

        private bool IsPlayerDead()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            return playerHealth != null && playerHealth.IsDead();
        }

        public bool CorruptorPhaseInProgress => corruptorPhaseController != null && corruptorPhaseController.InProgress;

    }
}
