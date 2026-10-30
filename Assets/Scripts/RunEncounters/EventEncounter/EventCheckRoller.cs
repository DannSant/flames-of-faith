using Game.Progression;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>
    /// Rolls an event check against the player: reads the check's value (a stat, the class's Key stat or current
    /// Grace) from the event context and turns it into a chance with EventCheckSettings.
    /// </summary>
    public static class EventCheckRoller
    {
        /// <summary>The stat a check reads: its own stat, or the class's Key stat (Luck when the class has none).</summary>
        public static StatType ResolveStat(EventCheck check, System.Collections.Generic.IReadOnlyList<ClassStatPriority> priorities)
        {
            if (check.source != EventCheckSource.KeyStat) return check.stat;
            if (ClassStatGuide.TryGetKeyStat(priorities, out StatType key)) return key;

            Debug.LogWarning("[EventCheck] The class has no Key stat; the check uses Luck.");
            return StatType.Luck;
        }

        public static float GetChance(EventCheck check, EventContext context, EventCheckSettings settings)
        {
            if (check.source == EventCheckSource.CurrentGrace)
            {
                float grace = context.playerGrace != null ? context.playerGrace.CurrentGrace : 0f;
                return settings.GetGraceChance(check.difficulty, grace);
            }

            StatType stat = ResolveStat(check, context.classData != null ? context.classData.statPriorities : null);
            float points = context.playerProgression != null ? context.playerProgression.GetStatTotal(stat) : 0f;
            return settings.GetStatChance(stat, check.difficulty, points);
        }

        public static bool Roll(EventCheck check, EventContext context, EventCheckSettings settings)
        {
            float chance = GetChance(check, context, settings);
            float roll = Random.value;
            bool success = EventCheckMath.Succeeds(chance, roll);
            if (settings.logChecks)
            {
                Debug.Log($"[EventCheck] {check.source} {(check.source == EventCheckSource.Stat ? check.stat.ToString() : "")} " +
                    $"{check.difficulty}: chance {chance:P0}, roll {roll:F2} -> {(success ? "Success" : "Failure")}");
            }
            return success;
        }
    }
}
