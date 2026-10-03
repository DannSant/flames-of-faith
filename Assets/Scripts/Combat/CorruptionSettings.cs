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
        [Tooltip("Upper limit for the Corrupted spawn chance.")]
        [Range(0f, 1f)]
        [SerializeField] private float maxCorruptedChance = 0.5f;
        [Tooltip("Damage multiplier applied to everything a Corrupted enemy deals.")]
        [SerializeField] private float corruptedDamageMultiplier = 1.5f;

        [Header("Corrupted Damage")]
        [Tooltip("Corrupted Damage the player must take to gain 1 Corruption.")]
        [SerializeField] private float corruptedDamagePerCorruption = 10f;
        [Tooltip("Maximum Corruption gained per wave from Corrupted Damage.")]
        [SerializeField] private int maxCorruptionFromDamage = 5;

        [Header("Visuals")]
        [Tooltip("Prefab spawned on top of anything Corrupted (enemies, the boss and the player).")]
        [SerializeField] private GameObject corruptedVfxPrefab;

        public float CorruptedDamageMultiplier => corruptedDamageMultiplier;
        public float CorruptedDamagePerCorruption => corruptedDamagePerCorruption;
        public int MaxCorruptionFromDamage => maxCorruptionFromDamage;
        public GameObject CorruptedVfxPrefab => corruptedVfxPrefab;

        public float GetCorruptedSpawnChance(float corruptedLevel, float waveMultiplier)
        {
            return Mathf.Clamp(corruptedLevel * corruptedChancePerNegativeGrace * waveMultiplier, 0f, maxCorruptedChance);
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
