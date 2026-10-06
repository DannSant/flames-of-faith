using Game.AI;
using Game.Combat;
using Game.Common;
using Game.Level;
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
        private CameraFocusController cameraFocus;
        private Coroutine phaseRoutine;

        private GameObject corruptor;
        private EnemyHealth corruptorHealth;
        private bool corruptorDied;

        public event Action<Transform> OnCorruptorSpawned;
        public event Action<float, Transform> OnPhaseStarted;   // Sends the phase duration and the player (for UI anchored to them)
        public event Action<int> OnCorruptionChanged;           // Corruption the Corruptor grants if killed now
        public event Action<float> OnTimerUpdated;      // Sends the remaining time
        public event Action OnPhaseEnded;

        public bool InProgress => phaseRoutine != null;

        public void Initialize(EnemySpawnCoordinator enemySpawnCoordinator)
        {
            this.enemySpawnCoordinator = enemySpawnCoordinator;
            cameraFocus = gameObject.AddComponent<CameraFocusController>();
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
            EndFocus();
            UnsubscribeFromCorruptor();
            OnPhaseEnded?.Invoke();
        }

        // Always safe to call: releases the freeze and puts the camera back
        private void EndFocus()
        {
            if (cameraFocus != null)
            {
                cameraFocus.Restore();
            }
            GameplayFreeze.End(this);
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

            // Show the player where the Corruptor is. Everything holds still meanwhile, and the timer only starts after
            if (settings.FocusCameraOnCorruptor)
            {
                GameplayFreeze.Begin(this);
                yield return cameraFocus.FocusRoutine(corruptor.transform, settings.FocusZoomMultiplier,
                    settings.FocusPanInDuration, settings.FocusHoldDuration, settings.FocusPanOutDuration);
                EndFocus();

                if (corruptor == null || IsPlayerDead())
                {
                    AbortPhase();
                    yield break;
                }
            }

            float duration = settings.CorruptorPhaseDuration;
            float elapsed = 0f;
            var playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            int currentCorruption = -1;
            OnPhaseStarted?.Invoke(duration, playerCorruption != null ? playerCorruption.transform : null);

            // corruptor == null covers it being destroyed by something other than a kill
            while (!corruptorDied && corruptor != null && elapsed < duration)
            {
                if (IsPlayerDead())
                {
                    AbortPhase();
                    yield break;
                }
                // The longer the Corruptor lives, the more Corruption it will grant - show it as it grows
                int corruptionNow = settings.GetCorruptorKillCorruption(elapsed);
                if (corruptionNow != currentCorruption)
                {
                    currentCorruption = corruptionNow;
                    if (playerCorruption != null)
                    {
                        playerCorruption.SetCorruptorCorruption(currentCorruption);
                    }
                    OnCorruptionChanged?.Invoke(currentCorruption);
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
            EndFocus();
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
            EndFocus();
            UnsubscribeFromCorruptor();
        }
    }
}
