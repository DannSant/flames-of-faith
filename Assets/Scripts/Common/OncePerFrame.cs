namespace Game.Common
{
    /// <summary>
    /// Lets a call through once per frame. Animation events on the clips of a blend tree fire once per
    /// clip, all in the same frame: the Warrior's 8-direction attack fired its start event 8 times, so the
    /// cleave hit every enemy 8 times per swing.
    /// </summary>
    public class OncePerFrame
    {
        private int lastFrame = -1;

        /// <summary>True the first time it is called for <paramref name="frame"/>, false after that.</summary>
        public bool TryPass(int frame)
        {
            if (frame == lastFrame) return false;
            lastFrame = frame;
            return true;
        }
    }
}
