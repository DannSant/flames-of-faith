using Game.Common;
using UnityEngine;

namespace Game.GameSettings
{
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

            if (isPaused)
            {
                Time.timeScale = 0f; // Pause the game
            }
            else
            {
                Time.timeScale = 1f; // Resume the game
            }
        }
    }
}