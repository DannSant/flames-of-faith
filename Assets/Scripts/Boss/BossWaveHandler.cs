using Game.Combat;
using Game.Scene;
using Game.Waves;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Boss
{
    public class BossWaveHandler : MonoBehaviour
    {
        public event Action OnBossFightStarted;
        public event Action<float, float> OnFlameProgressChanged; // current, max
        public event Action OnPhaseOneStarted;
        public event Action OnPhaseTwoStarted;
        public event Action OnBossFightEnded;

        [SerializeField] private float flameDuration = 60f;
        [SerializeField] private float enemySpawnInterval = 5f;

        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private GameObject[] enemyPrefabs;
        [Header("Spawn prefabs")]
        [SerializeField] private EnemySpawnPortal spawnPortalPrefab;

        private float currentFlameTime;
        private float spawnTimer;

        private bool isPhaseOne = false;
        private bool isPhaseTwo = false;
        private bool bossAlive = true;

        private List<GameObject> activeEnemies = new List<GameObject>();

        private void Start()
        {
            Initialize();
            //StartPhaseOne();
        }

        private void OnEnable()
        {
            if (PlayerManager.Instance == null) return;

            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (PlayerManager.Instance == null) return;

            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= HandlePlayerDeath;
            }
        }

        private void HandlePlayerDeath()
        {
            // Update() only drives the flame timer and add spawning while isPhaseOne is true, so
            // this alone halts both. isPhaseTwo is cleared too for state hygiene even though
            // nothing currently reads it in an Update loop.
            isPhaseOne = false;
            isPhaseTwo = false;

            DestroyAllTrackedEnemiesSilently();
        }

        /// <summary>
        /// Destroys every add this handler spawned, with no death VFX/loot/animation - mirrors
        /// EnemySpawnCoordinator.DestroyAllTrackedEnemiesSilently, used the same way on player death.
        /// </summary>
        public void DestroyAllTrackedEnemiesSilently()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
        }

        private void Initialize()
        {
            currentFlameTime = 0f;
            spawnTimer = enemySpawnInterval;
            bossAlive = true;
            OnBossFightStarted?.Invoke();
        }

        public void StartPhaseOne()
        {
            isPhaseOne = true;
            isPhaseTwo = false;

            currentFlameTime = 0f;
            spawnTimer = enemySpawnInterval;

            OnPhaseOneStarted?.Invoke();
        }

        private void Update()
        {
            if (isPhaseOne)
            {
                UpdatePhaseOne();
            }
        }

        private void UpdatePhaseOne()
        {
            //  Flame progression
            currentFlameTime += Time.deltaTime;

            OnFlameProgressChanged?.Invoke(currentFlameTime, flameDuration);

            // Enemy spawning
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = enemySpawnInterval;
            }

            //  Transition condition
            if (currentFlameTime >= flameDuration)
            {
                StartPhaseTwo();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs.Length == 0 || enemySpawnPoints.Length == 0)
                return;

            var prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            var spawnPoint = enemySpawnPoints[UnityEngine.Random.Range(0, enemySpawnPoints.Length)];

            //Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            EnemySpawnPortal portal = Instantiate(spawnPortalPrefab, spawnPoint.position, Quaternion.identity);
            portal.Initialize(new SpawnInfo
            {
                EnemyToSpawn = prefab,
                SpawnPosition = spawnPoint.position,
                SpawnRotation = Quaternion.identity,
                WaveSpawnerTransform = this.transform,
                WaveNumber =  1
            });
            portal.onEnemySpawnedEvent += OnEnemySpawned;
        }

        private void OnEnemySpawned(EnemySpawnPortal portal, GameObject enemy)
        {
            activeEnemies.Add(enemy);

            portal.onEnemySpawnedEvent -= OnEnemySpawned;
        }

        private void StartPhaseTwo()
        {
            isPhaseOne = false;
            isPhaseTwo = true;

            

            OnPhaseTwoStarted?.Invoke();
        }

        public void NotifyBossDied()
        {
            bossAlive = false;
            OnBossFightEnded?.Invoke();
        }

        public void ReduceFlameProgress(float damageAmount)
        {
            if (!isPhaseOne) return;
            if(currentFlameTime>=flameDuration) return;
            currentFlameTime = Mathf.Max(0f, currentFlameTime - damageAmount);
            OnFlameProgressChanged?.Invoke(currentFlameTime, flameDuration);
        }
    }

}