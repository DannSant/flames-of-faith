using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>How much a stat matters to a class: shown as a gold (Key) or silver (Complementary) star.</summary>
    public enum StatRole
    {
        None,
        Key,
        Complementary
    }

    /// <summary>One of a class's recommended stats, authored on its CharacterClassData.</summary>
    [Serializable]
    public class ClassStatPriority
    {
        public StatType stat;
        public StatRole role = StatRole.Complementary;
        [Tooltip("Why this stat matters for the class. Shown in the stat's tooltip.")]
        [TextArea(2, 4)] public string justification;
    }

    /// <summary>
    /// Pure rules for a class's recommended stats: their role, the Stats pane order and tab, and the
    /// tooltip line. Shared by the Stats pane and the upgrade choices.
    /// </summary>
    public static class ClassStatGuide
    {
        public static ClassStatPriority Find(IReadOnlyList<ClassStatPriority> priorities, StatType stat)
        {
            if (priorities == null) return null;
            foreach (var priority in priorities)
            {
                if (priority != null && priority.role != StatRole.None && priority.stat == stat) return priority;
            }
            return null;
        }

        public static StatRole GetRole(IReadOnlyList<ClassStatPriority> priorities, StatType stat) =>
            Find(priorities, stat)?.role ?? StatRole.None;

        /// <summary>The class's Key stat (the first one listed); false when it has none.</summary>
        public static bool TryGetKeyStat(IReadOnlyList<ClassStatPriority> priorities, out StatType stat)
        {
            stat = default;
            if (priorities == null) return false;
            foreach (var priority in priorities)
            {
                if (priority != null && priority.role == StatRole.Key)
                {
                    stat = priority.stat;
                    return true;
                }
            }
            return false;
        }

        /// <summary>A class's Key and Complementary stats always show on the Primary tab, whatever their category.</summary>
        public static bool ShowsOnPrimaryTab(StatCategory category, StatRole role) =>
            role != StatRole.None || category == StatCategory.Primary;

        /// <summary>
        /// Key stats first, then Complementary ones, each in the order the class lists them; every other
        /// stat keeps its order from <paramref name="stats"/>.
        /// </summary>
        public static List<StatType> SortForDisplay(IEnumerable<StatType> stats, IReadOnlyList<ClassStatPriority> priorities)
        {
            var input = stats.ToList();
            var result = new List<StatType>(input.Count);
            foreach (StatRole role in new[] { StatRole.Key, StatRole.Complementary })
            {
                if (priorities == null) break;
                foreach (var priority in priorities)
                {
                    if (priority == null || priority.role != role) continue;
                    if (input.Contains(priority.stat) && !result.Contains(priority.stat)) result.Add(priority.stat);
                }
            }
            foreach (var stat in input)
            {
                if (!result.Contains(stat)) result.Add(stat);
            }
            return result;
        }

        /// <summary>"Key stat for the Warrior: ..." for the tooltip; null for a stat without a role.</summary>
        public static string DescribeRole(ClassStatPriority priority, string className)
        {
            if (priority == null || priority.role == StatRole.None) return null;
            string label = priority.role == StatRole.Key ? "Key stat" : "Complementary stat";
            string forClass = string.IsNullOrEmpty(className) ? label : $"{label} for the {className}";
            return string.IsNullOrWhiteSpace(priority.justification) ? forClass : $"{forClass}: {priority.justification.Trim()}";
        }
    }
}
