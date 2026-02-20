using Game.AI;
using Game.Common;
using Game.Scene;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Waves
{
    public struct SpawnInfo
    {
        public GameObject EnemyToSpawn;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
        public Transform WaveSpawnerTransform;
        public int WaveNumber;

    }
    public class EnemySpawnPortal : MonoBehaviour, ISceneCleanupHandler
    {
        [SerializeField] private float spawnDelay = .5f;
        [SerializeField] private float destroyDelay = .5f;

        public event Action<EnemySpawnPortal, GameObject> onEnemySpawnedEvent;

        private bool shouldSpawn = true;

        public void Cleanup()
        {
            shouldSpawn = false;
            onEnemySpawnedEvent = null;
            StopAllCoroutines();
            Destroy(gameObject);
        }

        public void Initialize(SpawnInfo spawnInfo)
        {
            StartCoroutine(SpawnRoutine(spawnInfo));
        }

        private void OnDestroy()
        {
            shouldSpawn = false;
            onEnemySpawnedEvent = null;
            StopAllCoroutines();
        }

        private IEnumerator SpawnRoutine(SpawnInfo spawnInfo)
        {
            yield return new WaitForSeconds(spawnDelay);
            if (!shouldSpawn) yield break;
            if (PlayerManager.Instance.IsPlayerOnMap)
            {
                yield break;
            }
            var enemyObj = Instantiate(spawnInfo.EnemyToSpawn, spawnInfo.SpawnPosition, spawnInfo.SpawnRotation, spawnInfo.WaveSpawnerTransform);
            Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
            onEnemySpawnedEvent?.Invoke(this, enemyObj);
            enemyComponent.Initialize(spawnInfo.WaveNumber);
            Destroy(gameObject, destroyDelay);

        }
    }

}