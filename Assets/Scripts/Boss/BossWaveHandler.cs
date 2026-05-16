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
            StartPhaseOne();
        }

        private void Initialize()
        {
            currentFlameTime = 0f;
            spawnTimer = enemySpawnInterval;
            bossAlive = true;
            OnBossFightStarted?.Invoke();
        }

        private void StartPhaseOne()
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
            else if (isPhaseTwo)
            {
                // Optional: monitor boss death here if needed
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
            currentFlameTime = Mathf.Max(0f, currentFlameTime - damageAmount);
            OnFlameProgressChanged?.Invoke(currentFlameTime, flameDuration);
        }
    }

}