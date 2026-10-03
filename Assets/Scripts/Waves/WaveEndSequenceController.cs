using Game.Combat;
using Game.Control;
using Game.Scene;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Waves
{
    public class WaveEndSequenceController : MonoBehaviour
    {
        private EnemySpawnCoordinator enemySpawnCoordinator;
        private CorruptorResult corruptorResult;

        private bool endingWave = false;
        private Coroutine endSequenceCoroutine;

        public event Action OnWaveCompleteStarted;
        public event Action OnWaveCompleteEnded;
        public event Action<WaveCorruptionResult> OnWaveCorruptionResolved;

        public bool EndingWave => endingWave;

        public void Initialize(EnemySpawnCoordinator enemySpawnCoordinator)
        {
            this.enemySpawnCoordinator = enemySpawnCoordinator;
        }

        public void BeginEndSequence(CorruptorResult corruptorResult)
        {
            this.corruptorResult = corruptorResult;
            if (endSequenceCoroutine != null)
            {
                StopCoroutine(endSequenceCoroutine);
            }
            endSequenceCoroutine = StartCoroutine(EndSequenceRoutine());
        }

        public void InvokeOnWaveComplete()
        {
            OnWaveCompleteEnded?.Invoke();
        }

        private IEnumerator EndSequenceRoutine()
        {
            endingWave = true;
            // Invoke wave complete started event, this will make the player invulnerable so the cleanse animation can play safely
            OnWaveCompleteStarted?.Invoke();

            // Play cleanse animation
            var playerVisual = PlayerManager.Instance.GetPlayerChildComponent<CharacterVisual>();
            float cleanseAnimationDuration = 1.7f; // Fallback if the duration can't be read from the clip
            if (playerVisual != null)
            {
                playerVisual.PlayCleanseAnimation();

                // Each class's Cleanse clip can have a different length, so read it
                // dynamically instead of assuming a single shared duration
                var duration = playerVisual.GetCleanseAnimationDuration();
                if (duration > 0f)
                {
                    cleanseAnimationDuration = duration;
                }
            }

            // Wait for the animation to finish playing
            yield return new WaitForSeconds(cleanseAnimationDuration);

            ResolveWaveCorruption();

            // Destroy all remaining enemies
            enemySpawnCoordinator.KillAllTrackedEnemiesWithEffects(playerVisual.transform);

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
            endSequenceCoroutine = null;
        }

        // Grace + Grace per wave - Corruption gathered this wave becomes the new Grace, then Corruption resets
        private void ResolveWaveCorruption()
        {
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            var playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>();
            if (playerGrace == null || playerCorruption == null)
            {
                return;
            }

            var result = new WaveCorruptionResult
            {
                corruptor = corruptorResult,
                corruptedDamageTaken = playerCorruption.CorruptedDamageTaken,
                corruptionFromDamage = playerCorruption.CorruptionFromDamage,
                totalCorruption = playerCorruption.CorruptionValue,
                gracePerWave = playerGrace.GracePerWave,
                graceBefore = playerGrace.CurrentGrace
            };
            result.graceAfter = playerGrace.ApplyWaveResolution(result.gracePerWave, result.totalCorruption);

            playerCorruption.ResetCorruption();
            OnWaveCorruptionResolved?.Invoke(result);
        }

        public void StopEndSequence()
        {
            if (endSequenceCoroutine != null)
            {
                StopCoroutine(endSequenceCoroutine);
                endSequenceCoroutine = null;
            }
            endingWave = false;
        }
    }
}
