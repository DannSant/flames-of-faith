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
        private CorruptorPhaseController corruptorPhaseController;

        public void Initialize(EnemySpawnCoordinator enemySpawnCoordinator, WaveEndSequenceController waveEndSequenceController,
            CorruptorPhaseController corruptorPhaseController)
        {
            this.enemySpawnCoordinator = enemySpawnCoordinator;
            this.waveEndSequenceController = waveEndSequenceController;
            this.corruptorPhaseController = corruptorPhaseController;
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
            corruptorPhaseController.StopPhase();
            enemySpawnCoordinator.StopSpawning();
            enemySpawnCoordinator.DestroyAllTrackedEnemiesSilently();
        }

        public void Cleanup()
        {
            StopAllCoroutines();
            corruptorPhaseController.StopPhase();
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
