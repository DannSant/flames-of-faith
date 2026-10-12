using System;
using System.Collections.Generic;

namespace Game.Progression
{
    /// <summary>
    /// Run saves store stats by enum NAME, so renaming a StatType would make old saves fail to
    /// load (and get deleted). Old names stay readable through this alias table.
    /// </summary>
    public static class StatTypeNames
    {
        private static readonly Dictionary<string, StatType> legacyNames = new()
        {
            { "PierceAmount", StatType.Pierce }, // renamed Oct 2026 (Pierce rework)
        };

        /// <summary>
        /// Parses a stat name as stored in a save: current enum names, then legacy names.
        /// Numbers and unknown names are rejected.
        /// </summary>
        public static bool TryParse(string name, out StatType stat)
        {
            stat = default;
            if (string.IsNullOrWhiteSpace(name)) return false;

            // Enum.TryParse also accepts "11" or "999"; saves only ever store names.
            if (char.IsDigit(name[0]) || name[0] == '-') return false;

            if (Enum.TryParse(name, out stat) && Enum.IsDefined(typeof(StatType), stat)) return true;
            return legacyNames.TryGetValue(name, out stat);
        }
    }
}
