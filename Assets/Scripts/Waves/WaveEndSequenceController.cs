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
        private float graceRemovedPerWave;
        private EnemySpawnCoordinator enemySpawnCoordinator;

        private bool endingWave = false;
        private Coroutine endSequenceCoroutine;

        public event Action OnWaveCompleteStarted;
        public event Action OnWaveCompleteEnded;

        public bool EndingWave => endingWave;

        public void Initialize(float graceRemovedPerWave, EnemySpawnCoordinator enemySpawnCoordinator)
        {
            this.graceRemovedPerWave = graceRemovedPerWave;
            this.enemySpawnCoordinator = enemySpawnCoordinator;
        }

        public void BeginEndSequence()
        {
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
            if (playerVisual != null)
            {
                playerVisual.PlayCleanseAnimation();
            }

            // Wait for a short duration to allow the animation to play
            yield return new WaitForSeconds(1.7f);

            // Reduce grace when wave ends
            var playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (playerGrace != null)
            {
                playerGrace.RemoveGrace(graceRemovedPerWave); // Remove 1 grace point
            }

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
