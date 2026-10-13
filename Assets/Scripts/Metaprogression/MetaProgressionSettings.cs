using UnityEngine;

namespace Game.Metaprogression
{
    /// <summary>
    /// Tunable values for meta-progression: how Favors are earned and the developer switches.
    /// Loaded from Resources/Metaprogression/MetaProgressionSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "MetaProgressionSettings", menuName = "Game/Meta Progression Settings")]
    public class MetaProgressionSettings : ScriptableObject
    {
        private const string ResourcePath = "Metaprogression/MetaProgressionSettings";

        [Header("Favors")]
        [Tooltip("Favors for finishing a wave with Grace above 0 (after the wave's Corruption is resolved).")]
        [Min(0)]
        [SerializeField] private int favorsPerPositiveGraceWave = 1;
        [Tooltip("Favors for killing the boss.")]
        [Min(0)]
        [SerializeField] private int favorsPerBossKill = 5;

        [Header("Developer")]
        [Tooltip("Developer setting. On: Favors earned with any class can be spent on any class. " +
            "Off: each class spends only the Favors it earned. Favors are always stored per class, so switching loses nothing.")]
        [SerializeField] private bool sharedFavorPool = false;
        [Tooltip("Developer setting. Every class is playable without being unlocked (for tester builds).")]
        [SerializeField] private bool unlockAllClasses = true;

        private static MetaProgressionSettings instance;

        public int FavorsPerPositiveGraceWave => favorsPerPositiveGraceWave;
        public int FavorsPerBossKill => favorsPerBossKill;
        public bool SharedFavorPool => sharedFavorPool;
        public bool UnlockAllClasses => unlockAllClasses;

        public static MetaProgressionSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<MetaProgressionSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"MetaProgressionSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<MetaProgressionSettings>();
                    }
                }
                return instance;
            }
        }

        public int GetWaveFavors(float graceAfterWave) =>
            FavorRules.CalculateWaveFavors(graceAfterWave, favorsPerPositiveGraceWave);
    }
}
