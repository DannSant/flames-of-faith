namespace Game.Waves
{
    /// <summary>
    /// How the Corruptor phase of a wave ended.
    /// </summary>
    public struct CorruptorResult
    {
        public bool spawned;
        public bool killed;
        public float killTime;
        public int corruption;

        public static CorruptorResult None => new CorruptorResult();
    }

    /// <summary>
    /// Plain data describing the end of wave resolution (Grace + Grace Affinity - Corruption), for the UI.
    /// </summary>
    public struct WaveCorruptionResult
    {
        public CorruptorResult corruptor;
        public float corruptedDamageTaken;
        public int corruptionFromDamage;
        public int totalCorruption;
        public float graceAffinity;
        public float graceBefore;
        public float graceAfter;
    }
}
