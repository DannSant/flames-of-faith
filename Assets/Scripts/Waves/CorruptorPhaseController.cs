using Game.AI;
using Game.Combat;
using Game.Scene;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Waves
{
    /// <summary>
    /// Runs the end of wave Corruptor phase: spawns the Corruptor, gives the player a limited time to kill it and
    /// scores the Corruption gained from how long it took (or from letting it escape).
    /// </summary>
    public class CorruptorPhaseController : MonoBehaviour
    {
        private EnemySpawnCoordinator enemySpawnCoordinator;
        private Coroutine phaseRoutine;

        private GameObject corruptor;
        private EnemyHealth corruptorHealth;
        private bool corruptorDied;

        public event Action<Transform> OnCorruptorSpawned;
        public event Action<float> OnPhaseStarted;      // Sends the phase duration
        public event Action<float> OnTimerUpdated;      // Sends the remaining time
        public event Action OnPhaseEnded;

        public bool InProgress => phaseRoutine != null;

        public void Initialize(EnemySpawnCoordinator enemySpawnCoordinator)
        {
            this.enemySpawnCoordinator = enemySpawnCoordinator;
        }

        public void BeginPhase(EnemyType corruptorType, int waveNumber, Action<CorruptorResult> onFinished)
        {
            StopPhase();
            phaseRoutine = StartCoroutine(PhaseRoutine(corruptorType, waveNumber, onFinished));
        }

        /// <summary>
        /// Cancels the phase without reporting a result (player death, scene cleanup).
        /// </summary>
        public void StopPhase()
        {
            if (phaseRoutine == null)
            {
                return;
            }

            StopCoroutine(phaseRoutine);
            phaseRoutine = null;
            UnsubscribeFromCorruptor();
            OnPhaseEnded?.Invoke();
        }

        private IEnumerator PhaseRoutine(EnemyType corruptorType, int waveNumber, Action<CorruptorResult> onFinished)
        {
            var settings = CorruptionSettings.Instance;
            corruptor = null;
            corruptorDied = false;

            if (!enemySpawnCoordinator.SpawnEnemy(corruptorType, waveNumber, HandleCorruptorSpawned))
            {
                Finish(onFinished, CorruptorResult.None);
                yield break;
            }

            // The Corruptor comes out of a spawn portal after a short delay
            float waited = 0f;
            while (corruptor == null && waited < settings.CorruptorSpawnTimeout)
            {
                if (IsPlayerDead())
                {
                    AbortPhase();
                    yield break;
                }
                waited += Time.deltaTime;
                yield return null;
            }

            if (corruptor == null)
            {
                Debug.LogWarning($"CorruptorPhaseController: {corruptorType} did not spawn in time, skipping the Corruptor phase.");
                Finish(onFinished, CorruptorResult.None);
                yield break;
            }

            float duration = settings.CorruptorPhaseDuration;
            float elapsed = 0f;
            OnPhaseStarted?.Invoke(duration);

            // corruptor == null covers it being destroyed by something other than a kill
            while (!corruptorDied && corruptor != null && elapsed < duration)
            {
                if (IsPlayerDead())
                {
                    AbortPhase();
                    yield break;
                }
                elapsed += Time.deltaTime;
                OnTimerUpdated?.Invoke(Mathf.Max(0f, duration - elapsed));
                yield return null;
            }

            var result = new CorruptorResult { spawned = true };
            if (corruptorDied)
            {
                result.killed = true;
                result.killTime = elapsed;
                result.corruption = settings.GetCorruptorKillCorruption(elapsed);
            }
            else
            {
                result.corruption = settings.CorruptorEscapeCorruption;
                enemySpawnCoordinator.DespawnThroughPortal(corruptor);
            }

            Finish(onFinished, result);
        }

        private void HandleCorruptorSpawned(GameObject spawned)
        {
            corruptor = spawned;
            corruptorHealth = spawned.GetComponent<EnemyHealth>();
            if (corruptorHealth != null)
            {
                corruptorHealth.onDeath += HandleCorruptorDeath;
            }
            OnCorruptorSpawned?.Invoke(spawned.transform);
        }

        private void HandleCorruptorDeath()
        {
            corruptorDied = true;
        }

        // PlayerHealth.onDeath only fires after the death animation; checking IsDead stops the phase right away
        private bool IsPlayerDead()
        {
            var playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>();
            return playerHealth != null && playerHealth.IsDead();
        }

        // Ends the phase without reporting a result, so the wave never completes behind the death screen
        private void AbortPhase()
        {
            phaseRoutine = null;
            UnsubscribeFromCorruptor();
            OnPhaseEnded?.Invoke();
        }

        private void Finish(Action<CorruptorResult> onFinished, CorruptorResult result)
        {
            phaseRoutine = null;
            UnsubscribeFromCorruptor();
            OnPhaseEnded?.Invoke();
            onFinished?.Invoke(result);
        }

        private void UnsubscribeFromCorruptor()
        {
            if (corruptorHealth != null)
            {
                corruptorHealth.onDeath -= HandleCorruptorDeath;
            }
            corruptorHealth = null;
            corruptor = null;
        }

        private void OnDisable()
        {
            UnsubscribeFromCorruptor();
        }
    }
}
