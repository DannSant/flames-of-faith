using Game.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>How strongly one stat moves an event check: its points above the baseline, times the chance per point.</summary>
    [Serializable]
    public class EventCheckStatScale
    {
        public StatType stat;
        [Tooltip("Points that count as zero: a player at the baseline gets the difficulty's base chance. " +
            "Max Health starts at 20, so its baseline is 20.")]
        public float baseline;
        [Tooltip("Chance added per point above the baseline (0.03 = +3%), removed per point below it.")]
        public float chancePerPoint = 0.03f;
    }

    /// <summary>
    /// Tunables of the event stat checks (Resources/RunEncounters/EventCheckSettings.asset). A check succeeds with
    /// chance = clamp(base chance of its difficulty + (value - baseline) x chance per point, min, max).
    /// </summary>
    [CreateAssetMenu(menuName = "RunEncounters/Event Check Settings", fileName = "EventCheckSettings")]
    public class EventCheckSettings : ScriptableObject
    {
        private const string ResourcePath = "RunEncounters/EventCheckSettings";
        private static EventCheckSettings instance;

        public static EventCheckSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<EventCheckSettings>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning($"[EventCheckSettings] No asset at Resources/{ResourcePath}; using defaults.");
                        instance = CreateInstance<EventCheckSettings>();
                    }
                }
                return instance;
            }
        }

        [Header("Base chance per difficulty")]
        [Tooltip("Success chance of an Easy check for a player at the stat's baseline.")]
        [Range(0f, 1f)] public float easyChance = 0.75f;
        [Tooltip("Success chance of an Even check for a player at the stat's baseline.")]
        [Range(0f, 1f)] public float evenChance = 0.5f;
        [Tooltip("Success chance of a Hard check for a player at the stat's baseline.")]
        [Range(0f, 1f)] public float hardChance = 0.3f;

        [Header("Stats")]
        [Tooltip("Chance per stat point for stats without their own scale below (0.03 = +3% per point above 0).")]
        public float defaultChancePerPoint = 0.03f;
        [Tooltip("Stats with their own baseline and chance per point. The Key stat uses its stat's entry.")]
        public List<EventCheckStatScale> statScales = new();
        [Tooltip("Chance per point of current Grace (it can be negative, which lowers the chance).")]
        public float graceChancePerPoint = 0.02f;

        [Header("Limits")]
        [Tooltip("No check is ever less likely than this, however low the stat.")]
        [Range(0f, 1f)] public float minChance = 0.05f;
        [Tooltip("No check is ever more likely than this, however high the stat.")]
        [Range(0f, 1f)] public float maxChance = 0.95f;

        [Header("Debug")]
        [Tooltip("Log every check's chance, roll and result ([EventCheck]).")]
        public bool logChecks;

        public float GetBaseChance(EventCheckDifficulty difficulty) => difficulty switch
        {
            EventCheckDifficulty.Easy => easyChance,
            EventCheckDifficulty.Hard => hardChance,
            _ => evenChance
        };

        /// <summary>The success chance of a check on a stat, for a player with that many points.</summary>
        public float GetStatChance(StatType stat, EventCheckDifficulty difficulty, float points)
        {
            float baseline = 0f;
            float perPoint = defaultChancePerPoint;
            foreach (var scale in statScales)
            {
                if (scale != null && scale.stat == stat)
                {
                    baseline = scale.baseline;
                    perPoint = scale.chancePerPoint;
                    break;
                }
            }
            return EventCheckMath.GetChance(GetBaseChance(difficulty), points, baseline, perPoint, minChance, maxChance);
        }

        /// <summary>The success chance of a check on current Grace (baseline 0).</summary>
        public float GetGraceChance(EventCheckDifficulty difficulty, float grace) =>
            EventCheckMath.GetChance(GetBaseChance(difficulty), grace, 0f, graceChancePerPoint, minChance, maxChance);
    }

    /// <summary>The check formula, pure so tests can pass their own numbers.</summary>
    public static class EventCheckMath
    {
        public static float GetChance(float baseChance, float value, float baseline, float chancePerPoint, float min, float max)
        {
            if (max < min) (min, max) = (max, min);
            return Mathf.Clamp(baseChance + (value - baseline) * chancePerPoint, min, max);
        }

        /// <summary>A roll in [0, 1) succeeds when it's below the chance.</summary>
        public static bool Succeeds(float chance, float roll) => roll < chance;
    }
}
