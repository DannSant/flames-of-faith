using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Which level each level-up choice belongs to. A wave that gains N levels gives N choice
    /// groups, in level order: from 3 to 5 the groups are levels 4 and 5.
    /// </summary>
    public static class UpgradeLevels
    {
        /// <summary>The level of the first choice group, given the level now and the levels gained.</summary>
        public static int FirstLevel(int currentLevel, int levelsGained) => currentLevel - Mathf.Max(0, levelsGained) + 1;

        /// <summary>The level of the choice group at <paramref name="choiceIndex"/> (0-based).</summary>
        public static int LevelForChoice(int firstLevel, int choiceIndex) => firstLevel + Mathf.Max(0, choiceIndex);
    }
}
