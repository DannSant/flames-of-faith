using Game.Common;
using UnityEngine;

namespace Game.GameSettings
{
    public class PauseManager : Singleton<PauseManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public void SetPause(bool isPaused)
        {
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