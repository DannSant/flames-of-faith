using System;

namespace Game.Common
{
    /// <summary>
    /// Reports a cooldown's progress (time left, duration) every tick while it runs, plus one last
    /// report of 0 time left on the tick it stops. Without that last report, listeners such as the
    /// HUD cooldown sweep stay on the value from the frame before it ended (a thin sliver).
    /// </summary>
    public class CooldownProgressReporter
    {
        private bool wasRunning;
        private float lastDuration;

        public void Tick(bool running, float timeLeft, float duration, Action<float, float> report)
        {
            if (running)
            {
                wasRunning = true;
                lastDuration = duration;
                report?.Invoke(timeLeft, duration);
                return;
            }

            if (!wasRunning) return;
            wasRunning = false;
            report?.Invoke(0f, lastDuration);
        }
    }
}
