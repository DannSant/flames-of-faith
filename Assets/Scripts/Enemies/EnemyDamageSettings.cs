using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Stats a player is expected to have after beating a given number of levels.
    /// Enemy damage is sized against these, never against the real player.
    /// </summary>
    [Serializable]
    public struct ExpectedPlayerStats
    {
        [Tooltip("Combat/boss levels beaten in the run (GameSession.LevelsBeaten).")]
        public float levelsBeaten;
        [Tooltip("Max health the player is expected to have at this point.")]
        public float maxHealth;
        [Tooltip("Armor the player is expected to have at this point.")]
        public float armor;

        public ExpectedPlayerStats(float levelsBeaten, float maxHealth, float armor)
        {
            this.levelsBeaten = levelsBeaten;
            this.maxHealth = maxHealth;
            this.armor = armor;
        }
    }

    /// <summary>
    /// Tunable values for enemy -> player damage. Each enemy attack has a damage tier, and a tier
    /// means "kills a player with the expected stats in N hits". See EnemyDamageCalculator.
    /// Loaded from Resources/Combat/EnemyDamageSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDamageSettings", menuName = "Combat/Enemy Damage Settings")]
    public class EnemyDamageSettings : ScriptableObject
    {
        private const string ResourcePath = "Combat/EnemyDamageSettings";

        [Header("Expected Player")]
        [Tooltip("Expected max health and armor by levels beaten. Interpolated linearly between rows and clamped at both ends. Keep it sorted by levels beaten.")]
        [SerializeField] private List<ExpectedPlayerStats> expectedCurve = new()
        {
            new ExpectedPlayerStats(0, 25, 0),
            new ExpectedPlayerStats(2, 35, 1),
            new ExpectedPlayerStats(4, 45, 2),
            new ExpectedPlayerStats(6, 55, 4),
            new ExpectedPlayerStats(8, 65, 5),
        };
        [Tooltip("How much each wave inside a level counts as progress, in levels (0.25 = wave 3 reads the curve half a level further).")]
        [Min(0f)]
        [SerializeField] private float waveProgressStep = 0.25f;

        [Header("Damage Tiers")]
        [Tooltip("Hits a player with the expected stats survives before dying, per tier. Element 0 = tier 1. " +
            "Fractional tiers interpolate (1.5 = halfway between tier 1 and 2), tiers below 1 scale tier 1's damage down, tiers above the last are clamped.")]
        [SerializeField] private List<float> tierHitsToKill = new() { 10f, 7f, 5f, 3f, 2f };

        [Header("Armor")]
        [Tooltip("Controls how quickly armor loses effectiveness. Reduction = armor / (armor + this). " +
            "Higher values make each point of armor weaker. At 25: armor 10 = -29%, armor 25 = -50%, armor 50 = -67%. " +
            "Negative armor increases damage: armor -5 = +17%, armor -25 = +50%, never +100%.")]
        [SerializeField] private float armorEffectivenessConstant = 25f;
        [Tooltip("Armor can never reduce an attack below this fraction of its raw damage, " +
            "so stacking armor can't trivialise every enemy.")]
        [Range(0f, 1f)]
        [SerializeField] private float minDamagePercent = 0.15f;

        [Header("Limits")]
        [Tooltip("Smallest raw damage an enemy attack with a tier above 0 can deal.")]
        [Min(0f)]
        [SerializeField] private float minDamage = 0.1f;

        private static EnemyDamageSettings instance;

        public IReadOnlyList<ExpectedPlayerStats> ExpectedCurve => expectedCurve;
        public float WaveProgressStep => waveProgressStep;
        public IReadOnlyList<float> TierHitsToKill => tierHitsToKill;
        public float ArmorEffectivenessConstant => armorEffectivenessConstant;
        public float MinDamagePercent => minDamagePercent;
        public float MinDamage => minDamage;

        public static EnemyDamageSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<EnemyDamageSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"EnemyDamageSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<EnemyDamageSettings>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Replaces every value at once. Meant for tests and tooling that build their own settings
        /// instead of depending on the tuned asset.
        /// </summary>
        public void Configure(IEnumerable<ExpectedPlayerStats> curve, float waveStep, IEnumerable<float> hitsPerTier,
            float armorConstant = 25f, float minPercent = 0.15f, float minimumDamage = 0.1f)
        {
            expectedCurve = new List<ExpectedPlayerStats>(curve);
            waveProgressStep = waveStep;
            tierHitsToKill = new List<float>(hitsPerTier);
            armorEffectivenessConstant = armorConstant;
            minDamagePercent = minPercent;
            minDamage = minimumDamage;
            expectedCurve.Sort((a, b) => a.levelsBeaten.CompareTo(b.levelsBeaten));
        }
    }
}
