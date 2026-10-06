using Game.Waves;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Attaches the world space Corruptor bar to the player while the Corruptor phase runs and feeds it the timer
    /// and Corruption values. Parenting it to the player keeps it steady while the camera follows them.
    /// </summary>
    public class CorruptorPhaseBarPresenter : MonoBehaviour
    {
        [Tooltip("World Space canvas prefab with a CorruptorPhaseBarUI.")]
        [SerializeField] private CorruptorPhaseBarUI barPrefab;
        [Tooltip("Position relative to the player.")]
        [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.8f, 0f);

        private CorruptorPhaseBarUI bar;

        private void Start()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnCorruptorPhaseStarted += HandlePhaseStarted;
                WaveSpawner.Instance.OnCorruptorTimerUpdated += HandleTimerUpdated;
                WaveSpawner.Instance.OnCorruptorCorruptionChanged += HandleCorruptionChanged;
                WaveSpawner.Instance.OnCorruptorPhaseEnded += RemoveBar;
            }
        }

        private void OnDestroy()
        {
            if (WaveSpawner.Instance != null)
            {
                WaveSpawner.Instance.OnCorruptorPhaseStarted -= HandlePhaseStarted;
                WaveSpawner.Instance.OnCorruptorTimerUpdated -= HandleTimerUpdated;
                WaveSpawner.Instance.OnCorruptorCorruptionChanged -= HandleCorruptionChanged;
                WaveSpawner.Instance.OnCorruptorPhaseEnded -= RemoveBar;
            }

            // The player outlives this UI scene, so don't leave a bar attached to them
            RemoveBar();
        }

        private void HandlePhaseStarted(float duration, Transform player)
        {
            RemoveBar();
            if (barPrefab == null || player == null) return;

            bar = Instantiate(barPrefab, player);
            bar.transform.localPosition = localOffset;
            bar.Show(duration);
        }

        private void HandleTimerUpdated(float remaining)
        {
            if (bar != null) bar.SetRemaining(remaining);
        }

        private void HandleCorruptionChanged(int corruption)
        {
            if (bar != null) bar.SetCorruption(corruption);
        }

        private void RemoveBar()
        {
            if (bar != null)
            {
                Destroy(bar.gameObject);
                bar = null;
            }
        }
    }
}
