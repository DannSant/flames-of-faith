namespace Game.Waves
{
    /// <summary>Pure helpers about where a wave sits within its level.</summary>
    public static class WaveProgress
    {
        /// <summary>
        /// Whether the wave at <paramref name="waveIndex"/> (0-based) is the last wave of a level
        /// with <paramref name="waveCount"/> waves. Not the same as WaveDatabase.lastWave, which
        /// marks the last level of the game.
        /// </summary>
        public static bool IsLastWave(int waveIndex, int waveCount)
        {
            return waveCount > 0 && waveIndex == waveCount - 1;
        }
    }
}
