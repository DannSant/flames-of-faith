using Game.Combat;
using Game.Common;
using Game.Scene;
using UnityEngine;

namespace Game.Waves
{
    public class WaveLifecycleHandler : MonoBehaviour, ISceneCleanupHandler
    {
        private EnemySpawnCoordinator enemySpawnCoordinator;
        private WaveEndSequenceController waveEndSequenceController;

        public void Initialize(EnemySpawnCoordinator enemySpawnCoordinator, WaveEndSequenceController waveEndSequenceController)
        {
            this.enemySpawnCoordinator = enemySpawnCoordinator;
            this.waveEndSequenceController = waveEndSequenceController;
        }

        private void Start()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath += OnPlayerDeathDisableWave;
            }
        }

        private void OnDisable()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= OnPlayerDeathDisableWave;
            }
        }

        private void OnPlayerDeathDisableWave()
        {
            enemySpawnCoordinator.StopSpawning();
            enemySpawnCoordinator.DestroyAllTrackedEnemiesSilently();
        }

        public void Cleanup()
        {
            StopAllCoroutines();
            enemySpawnCoordinator.StopAllSpawningAndTracking();
            waveEndSequenceController.StopEndSequence();

            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.onDeath -= OnPlayerDeathDisableWave;
            }

            Destroy(gameObject);
        }
    }
}
