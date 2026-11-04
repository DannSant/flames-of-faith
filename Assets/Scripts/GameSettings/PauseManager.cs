using Game.Common;
using Game.Feel;
using UnityEngine;

namespace Game.GameSettings
{
    /// <summary>
    /// Pauses the game, and is the only place that writes Time.timeScale: paused ? 0 : TimeDilation's scale
    /// (hit stops and slow motion). It reapplies every frame, so a hit stop ends on time, and resuming after a
    /// pause gives back whatever is still running, or 1.
    /// </summary>
    public class PauseManager : Singleton<PauseManager>
    {
        public event System.Action<bool> onPauseToggled;

        public bool IsPaused { get; private set; }

        protected override void Awake()
        {
            base.Awake();
        }

        public void SetPause(bool isPaused)
        {
            IsPaused = isPaused;

            // Notify subscribers about the pause state change
            onPauseToggled?.Invoke(isPaused);

            ApplyTimeScale();
        }

        private void Update()
        {
            ApplyTimeScale();
        }

        private void ApplyTimeScale()
        {
            float scale = TimeDilationRules.TimeScale(IsPaused, TimeDilation.CurrentScale);
            if (!Mathf.Approximately(Time.timeScale, scale)) Time.timeScale = scale;
        }
    }
}
