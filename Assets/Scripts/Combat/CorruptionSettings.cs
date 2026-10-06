using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Tunable values for the Corruption mechanic: Corrupted enemies, Corrupted Damage and the shared Corrupted visual.
    /// Loaded from Resources/Combat/CorruptionSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "CorruptionSettings", menuName = "Combat/Corruption Settings")]
    public class CorruptionSettings : ScriptableObject
    {
        private const string ResourcePath = "Combat/CorruptionSettings";

        [Header("Corrupted Enemies")]
        [Tooltip("Chance for a spawned enemy to be Corrupted, per point of negative Grace (0.02 = 2% per point).")]
        [SerializeField] private float corruptedChancePerNegativeGrace = 0.02f;
        [Tooltip("Base Corrupted spawn chance per Taint Level of the current level (0.05 = 5% per level).")]
        [SerializeField] private float corruptedChancePerTaintLevel = 0.05f;
        [Tooltip("Upper limit for the Corrupted spawn chance.")]
        [Range(0f, 1f)]
        [SerializeField] private float maxCorruptedChance = 0.5f;
        [Tooltip("Damage multiplier applied to everything a Corrupted enemy deals.")]
        [SerializeField] private float corruptedDamageMultiplier = 1.5f;
        [Tooltip("Extra health for Corrupted enemies (1 = +100%). Enemies can override it in their EnemyData.")]
        [SerializeField] private float corruptedHealthBonus = 1f;

        [Header("Corrupted Damage")]
        [Tooltip("Corrupted Damage the player must take to gain 1 Corruption.")]
        [SerializeField] private float corruptedDamagePerCorruption = 10f;
        [Tooltip("Maximum Corruption gained per wave from Corrupted Damage.")]
        [SerializeField] private int maxCorruptionFromDamage = 5;

        [Header("Grace Pickups")]
        [Tooltip("Chance for a killed enemy to drop a Grace pickup with 0 Grace Affinity.")]
        [Range(0f, 1f)]
        [SerializeField] private float gracePickupBaseChance = 0f;
        [Tooltip("Extra Grace pickup drop chance per point of Grace Affinity (0.01 = 1% per point).")]
        [SerializeField] private float gracePickupChancePerAffinity = 0.01f;
        [Tooltip("Upper limit for the Grace pickup drop chance.")]
        [Range(0f, 1f)]
        [SerializeField] private float gracePickupMaxChance = 0.2f;

        [Header("Corruptor")]
        [Tooltip("Seconds to wait for the Corruptor to come out of its spawn portal before skipping the phase.")]
        [SerializeField] private float corruptorSpawnTimeout = 3f;
        [Tooltip("Seconds the player has to kill the Corruptor before it escapes.")]
        [SerializeField] private float corruptorPhaseDuration = 25f;
        [Tooltip("Killing the Corruptor within this many seconds only grants the base Corruption.")]
        [SerializeField] private float corruptorFastKillTime = 8f;
        [Tooltip("Corruption gained when the Corruptor is killed within the fast kill time.")]
        [SerializeField] private int corruptorBaseCorruption = 1;
        [Tooltip("After the fast kill time, +1 Corruption for every this many seconds (rounded up).")]
        [SerializeField] private float corruptorSecondsPerExtraCorruption = 3f;
        [Tooltip("Maximum Corruption the Corruptor can grant when killed.")]
        [SerializeField] private int corruptorMaxCorruption = 5;
        [Tooltip("Corruption gained when the Corruptor escapes (not killed in time).")]
        [SerializeField] private int corruptorEscapeCorruption = 5;

        [Header("Corruptor Camera Focus")]
        [Tooltip("Move the camera to the Corruptor when it spawns. Gameplay is frozen while the camera moves.")]
        [SerializeField] private bool focusCameraOnCorruptor = true;
        [Tooltip("Zoom while focused, as a multiplier of the current camera size (0.7 = 30% closer).")]
        [SerializeField] private float focusZoomMultiplier = 0.7f;
        [SerializeField] private float focusPanInDuration = 0.6f;
        [SerializeField] private float focusHoldDuration = 1f;
        [SerializeField] private float focusPanOutDuration = 0.5f;

        [Header("Wave Summary")]
        [Tooltip("Longest time the end of wave sequence waits for the Corruption summary before moving on. " +
            "0 = wait until the player closes it.")]
        [SerializeField] private float waveSummaryMaxWait = 0f;

        [Header("Visuals")]
        [Tooltip("Prefab spawned on top of anything Corrupted (enemies, the boss and the player).")]
        [SerializeField] private GameObject corruptedVfxPrefab;

        public float CorruptedDamageMultiplier => corruptedDamageMultiplier;
        public float CorruptedHealthBonus => corruptedHealthBonus;
        public float CorruptedDamagePerCorruption => corruptedDamagePerCorruption;
        public int MaxCorruptionFromDamage => maxCorruptionFromDamage;
        public GameObject CorruptedVfxPrefab => corruptedVfxPrefab;
        public bool FocusCameraOnCorruptor => focusCameraOnCorruptor;
        public float FocusZoomMultiplier => focusZoomMultiplier;
        public float FocusPanInDuration => focusPanInDuration;
        public float FocusHoldDuration => focusHoldDuration;
        public float FocusPanOutDuration => focusPanOutDuration;
        public float WaveSummaryMaxWait => waveSummaryMaxWait;
        public float CorruptorSpawnTimeout => corruptorSpawnTimeout;
        public float CorruptorPhaseDuration => corruptorPhaseDuration;
        public int CorruptorEscapeCorruption => corruptorEscapeCorruption;

        public int GetCorruptorKillCorruption(float killTime)
        {
            if (killTime <= corruptorFastKillTime || corruptorSecondsPerExtraCorruption <= 0f)
            {
                return corruptorBaseCorruption;
            }

            int extra = Mathf.CeilToInt((killTime - corruptorFastKillTime) / corruptorSecondsPerExtraCorruption);
            return Mathf.Min(corruptorMaxCorruption, corruptorBaseCorruption + extra);
        }

        /// <summary>
        /// Chance for a spawned enemy to be Corrupted: the level's Taint gives a base chance and every point of
        /// negative Grace adds to it. The wave multiplier scales the total.
        /// </summary>
        public float GetCorruptedSpawnChance(float corruptedLevel, int taintLevel, float waveMultiplier)
        {
            float chance = taintLevel * corruptedChancePerTaintLevel + corruptedLevel * corruptedChancePerNegativeGrace;
            return Mathf.Clamp(chance * waveMultiplier, 0f, maxCorruptedChance);
        }

        public float GetGracePickupChance(float graceAffinity)
        {
            float chance = gracePickupBaseChance + Mathf.Max(0f, graceAffinity) * gracePickupChancePerAffinity;
            return Mathf.Clamp(chance, 0f, gracePickupMaxChance);
        }

        public int GetCorruptionFromDamage(float corruptedDamage)
        {
            if (corruptedDamagePerCorruption <= 0f)
            {
                return 0;
            }
            return Mathf.Min(maxCorruptionFromDamage, Mathf.FloorToInt(corruptedDamage / corruptedDamagePerCorruption));
        }

        private static CorruptionSettings instance;

        public static CorruptionSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<CorruptionSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"CorruptionSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<CorruptionSettings>();
                    }
                }
                return instance;
            }
        }
    }
}
