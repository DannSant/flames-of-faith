using System.Collections.Generic;
using UnityEngine;

namespace Game.Metaprogression
{
    /// <summary>
    /// Plain data <see cref="SkillTreeController"/> pushes to the skill tree UI. The UI never
    /// reads meta-progression itself, so what it shows and what an upgrade costs can't drift apart.
    /// </summary>
    public class SkillTreeView
    {
        public List<SkillTreeClassTabView> tabs = new();
        public int selectedTab;

        public string className;
        public string classDescription;
        public Sprite classIcon;

        public bool classUnlocked;
        public int classUnlockCost;
        public bool canUnlockClass;
        /// <summary>Favors that can pay for the class unlock (every class's, combined).</summary>
        public int favorsForUnlock;

        /// <summary>Favors this class can spend on upgrades.</summary>
        public int favors;
        public bool sharedFavorPool;

        public List<SkillTreeNodeView> nodes = new();
    }

    public class SkillTreeClassTabView
    {
        public string name;
        public bool unlocked;
    }

    public class SkillTreeNodeView
    {
        public string nodeId;
        public string title;
        public Sprite icon;

        public int tier;
        public int maxTier;

        public string currentName;
        public string currentDescription;
        /// <summary>Null at the top tier.</summary>
        public string nextName;
        public string nextDescription;

        /// <summary>Favors for the next tier, -1 at the top tier.</summary>
        public int upgradeCost;
        public bool canUpgrade;

        public bool IsMaxed => upgradeCost < 0;
    }
}
