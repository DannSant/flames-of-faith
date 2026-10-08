using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Progression
{
    [Serializable]
    public class ExperienceTokenDenomination
    {
        [Tooltip("Tier of this denomination. Enemies can drop tiers up to their EnemyData.xpTier.")]
        public int tier = 1;
        [Tooltip("Chance to promote a token from the previous tier into this one. Ignored for tier 1.")]
        [Range(0f, 1f)]
        public float promoteChance = 0.5f;
        [Tooltip("Multiplier applied to the wave's base XP.")]
        public float multiplier = 1f;
        [Tooltip("Sprite of the token at this tier. Leave empty to keep the prefab's sprite.")]
        public Sprite sprite;
    }

    /// <summary>
    /// Tunable values for experience drops. Each wave targets a number of levels; the XP needed is split
    /// across the drops the wave is expected to produce. Higher-tier enemies can promote their token into
    /// higher denominations as bonus XP on top of the target.
    /// Loaded from Resources/Progression/ExperienceSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "ExperienceSettings", menuName = "Progression/Experience Settings")]
    public class ExperienceSettings : ScriptableObject
    {
        private const string ResourcePath = "Progression/ExperienceSettings";

        [Header("Drops")]
        [Tooltip("Chance for a killed enemy to drop an experience token.")]
        [Range(0f, 1f)]
        [SerializeField] private float dropChance = 0.8f;
        [Tooltip("XP per token when there is no running wave to calculate it from (e.g. test scenes).")]
        [SerializeField] private float fallbackXp = 1f;

        [Header("Wave Target")]
        [Tooltip("Levels a player should gain per wave from base tokens alone. Enhanced tokens add on top of this.")]
        [SerializeField] private float levelsPerWaveTarget = 1f;
        [Tooltip("Share of the expected spawns the player is expected to kill before the wave ends.")]
        [Range(0.05f, 1f)]
        [SerializeField] private float expectedKillRatio = 0.9f;
        [Tooltip("Spawn interval used for the estimate: 0 = the wave's regular cooldown, 1 = its long cooldown. " +
                 "Fast killers keep few enemies alive and stay on the long cooldown more often.")]
        [Range(0f, 1f)]
        [SerializeField] private float cooldownBlend = 0.3f;
        [Tooltip("Extra XP per point of the Experience Reduction stat (0.1 = +10% per point), so each point makes the " +
                 "player level the same amount faster at every level.")]
        [SerializeField] private float xpBonusPerReductionPoint = 0.1f;

        [Header("Denominations")]
        [SerializeField] private List<ExperienceTokenDenomination> denominations = new()
        {
            new ExperienceTokenDenomination { tier = 1, promoteChance = 1f, multiplier = 1f },
            new ExperienceTokenDenomination { tier = 2, promoteChance = 0.4f, multiplier = 2f },
            new ExperienceTokenDenomination { tier = 3, promoteChance = 0.2f, multiplier = 2.5f },
        };

        [Header("Debug")]
        [Tooltip("Logs the expected and actual XP numbers of every wave.")]
        [SerializeField] private bool logWaveXpDebug = false;

        public float DropChance => dropChance;
        public float FallbackXp => fallbackXp;
        public float LevelsPerWaveTarget => levelsPerWaveTarget;
        public float ExpectedKillRatio => expectedKillRatio;
        public float CooldownBlend => cooldownBlend;
        public float XpBonusPerReductionPoint => xpBonusPerReductionPoint;
        public bool LogWaveXpDebug => logWaveXpDebug;

        /// <summary>
        /// Returns the denomination for the given tier, or null when the tier is not defined.
        /// </summary>
        public ExperienceTokenDenomination GetDenomination(int tier)
        {
            foreach (var denomination in denominations)
            {
                if (denomination.tier == tier)
                {
                    return denomination;
                }
            }
            return null;
        }

        private static ExperienceSettings instance;

        public static ExperienceSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ExperienceSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"ExperienceSettings not found at Resources/{ResourcePath}. Falling back to default values.");
                        instance = CreateInstance<ExperienceSettings>();
                    }
                }
                return instance;
            }
        }
    }
}
