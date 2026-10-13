using Game.Misc;
using Game.Metaprogression;
using Game.Combat;
using Game.Control;
using Game.Progression;
using Game.Scene;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Waves
{
    public class WaveEndSequenceController : MonoBehaviour
    {
        [Header("Last Wave: Collect Experience")]
        [Tooltip("On the level's last wave, every XP gem left flies to the player at this speed before the summary.")]
        [SerializeField] private float xpCollectSpeed = 18f;
        [Tooltip("Longest the gems get to reach the player. Any still out after this are granted instantly.")]
        [SerializeField] private float xpCollectMaxDuration = 1.2f;

        private EnemySpawnCoordinator enemySpawnCoordinator;
        private CorruptorResult corruptorResult;

        private bool endingWave = false;
        private Coroutine endSequenceCoroutine;

        public event Action OnWaveCompleteStarted;
        public event Action OnWaveCompleteEnded;
        public event Action<WaveCorruptionResult, Action> OnWaveCorruptionResolved;
        private bool corruptionResultPresented = true;

        public bool EndingWave => endingWave;

        public void Initialize(EnemySpawnCoordinator enemySpawnCoordinator)
        {
            this.enemySpawnCoordinator = enemySpawnCoordinator;
        }

        public void BeginEndSequence(CorruptorResult corruptorResult)
        {
            this.corruptorResult = corruptorResult;
            corruptionResultPresented = true;
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

            // Last wave of the level: the gems would be lost with the level, so collect them all
            // as a reward. The killed enemies' gems exist by now. Other waves keep their gems.
            if (WaveSpawner.Instance != null && WaveSpawner.Instance.IsLastWaveOfLevel)
            {
                yield return CollectAllExperienceRoutine();
            }
            else
            {
                // Wait for the enemies to be fully destroyed
                yield return new WaitForSeconds(1f);
            }

            // Let the wave summary finish before moving on to rewards
            float waitedForSummary = 0f;
            float maxSummaryWait = CorruptionSettings.Instance.WaveSummaryMaxWait;
            while (!corruptionResultPresented && (maxSummaryWait <= 0f || waitedForSummary < maxSummaryWait))
            {
                waitedForSummary += Time.deltaTime;
                yield return null;
            }

            //Invoke end wave complete event
            InvokeOnWaveComplete();
            endingWave = false;
            endSequenceCoroutine = null;
        }

        private IEnumerator CollectAllExperienceRoutine()
        {
            ExperienceToken.AttractAll(xpCollectSpeed);

            float elapsed = 0f;
            // At least a short beat even with no gems, like the wait this replaces.
            float minDuration = Mathf.Min(0.5f, xpCollectMaxDuration);
            while (elapsed < xpCollectMaxDuration && (elapsed < minDuration || ExperienceToken.CountInScene() > 0))
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Anything that didn't make it (blocked, or the player can't collect right now) is granted anyway.
            var playerXP = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerComponent<PlayerExperience>() : null;
            ExperienceToken.CollectAllInstantly(playerXP);
        }

        // Grace + Grace Affinity - Corruption gathered this wave becomes the new Grace, then Corruption resets
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
                graceAffinity = playerGrace.GraceAffinity,
                graceBefore = playerGrace.CurrentGrace
            };
            result.graceAfter = playerGrace.ApplyWaveResolution(result.graceAffinity, result.totalCorruption);

            result.favorsEarned = MetaProgressionSettings.Instance.GetWaveFavors(result.graceAfter);
            if (result.favorsEarned > 0 && MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.AddRunFavors(result.favorsEarned);
                if (DamageNumberSpawner.Instance != null)
                {
                    DamageNumberSpawner.Instance.SpawnFavorGainedNumber(playerGrace.transform.position, result.favorsEarned);
                }
            }

            playerCorruption.ResetCorruption();

            if (OnWaveCorruptionResolved != null)
            {
                corruptionResultPresented = false;
                OnWaveCorruptionResolved.Invoke(result, () => corruptionResultPresented = true);
            }
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
