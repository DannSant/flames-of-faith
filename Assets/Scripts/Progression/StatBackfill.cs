using System.Collections.Generic;

namespace Game.Progression
{
    /// <summary>
    /// Run saves store only the stats that existed when they were written. When a stat is added
    /// to the game later, a loaded run would be missing it, and a stat missing from
    /// PlayerProgression stays at 0 for the rest of the run (item modifiers included).
    /// This fills those gaps with the stat's initial value.
    /// </summary>
    public static class StatBackfill
    {
        /// <summary>
        /// Adds every stat from <paramref name="initialValues"/> that <paramref name="stats"/>
        /// doesn't have yet. Existing values are never changed. Returns how many were added.
        /// </summary>
        public static int FillMissing(Dictionary<StatType, int> stats, IEnumerable<KeyValuePair<StatType, int>> initialValues)
        {
            if (stats == null || initialValues == null) return 0;

            int added = 0;
            foreach (var pair in initialValues)
            {
                if (stats.ContainsKey(pair.Key)) continue;
                stats[pair.Key] = pair.Value;
                added++;
            }
            return added;
        }
    }
}
