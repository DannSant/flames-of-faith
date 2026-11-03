namespace Game.Audio
{
    /// <summary>
    /// Pure pitch chain for XP pickups: each pickup within the window plays a step higher, up to the cap,
    /// and the one after the cap (or after a pause longer than the window) starts over at 1.
    /// Several pickups in the same frame (an instant sweep of every token) play a single sound.
    /// </summary>
    public class PickupPitchChain
    {
        private bool started;
        private float lastTime;
        private int lastFrame = -1;
        private float pitch = 1f;

        /// <summary>
        /// Registers a pickup at <paramref name="now"/> (seconds) on <paramref name="frame"/>.
        /// Returns false when a pickup already played this frame; otherwise the pitch to play it at.
        /// </summary>
        public bool TryNext(float now, int frame, float window, float step, float maxPitch, out float result)
        {
            result = pitch;
            if (started && frame == lastFrame) return false;

            if (!started || now - lastTime > window)
            {
                pitch = 1f;
            }
            else
            {
                float next = pitch + step;
                pitch = next > maxPitch + 0.0001f ? 1f : next;
            }

            started = true;
            lastTime = now;
            lastFrame = frame;
            result = pitch;
            return true;
        }

        public void Reset()
        {
            started = false;
            lastFrame = -1;
            pitch = 1f;
        }
    }
}
