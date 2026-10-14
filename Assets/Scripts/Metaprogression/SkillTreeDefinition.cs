using Game.Control;
using Game.Effects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Metaprogression
{
    /// <summary>
    /// A class's skill tree. Each node is one upgrade line of a class kit item: tier 0 is the base
    /// item, then "+1", "+2", ... Exactly one tier of each node is in the starting kit.
    ///
    /// Assets live in Resources/Metaprogression/SkillTrees and are read through <see cref="SkillTrees"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTree", menuName = "Game/Skill Tree")]
    public class SkillTreeDefinition : ScriptableObject
    {
        [SerializeField] private CharacterClass characterClass = CharacterClass.Warrior;
        [Header("Class Unlock (the tree's root)")]
        [Tooltip("Playable from the start, without buying it.")]
        [SerializeField] private bool unlockedByDefault = false;
        [Tooltip("Favors to unlock the class. Paid from every class's Favors combined (a locked class can't earn its own).")]
        [Min(0)]
        [SerializeField] private int unlockCost = 15;
        [SerializeField] private List<SkillTreeNode> nodes = new();

        public CharacterClass CharacterClass => characterClass;
        public bool UnlockedByDefault => unlockedByDefault;
        public int UnlockCost => unlockCost;
        public IReadOnlyList<SkillTreeNode> Nodes => nodes;

        /// <summary>The node whose tiers contain <paramref name="item"/>, and that item's tier in it.</summary>
        public bool TryFindNode(Effect item, out SkillTreeNode node, out int tier)
        {
            if (item != null)
            {
                foreach (var candidate in nodes)
                {
                    if (candidate == null) continue;
                    int index = candidate.IndexOf(item);
                    if (index >= 0)
                    {
                        node = candidate;
                        tier = index;
                        return true;
                    }
                }
            }

            node = null;
            tier = -1;
            return false;
        }
    }

    [Serializable]
    public class SkillTreeNode
    {
        [Tooltip("Stable id of this node; saved progress refers to it, so don't rename it once used.")]
        public string nodeId;
        [Tooltip("The item at each tier: element 0 is the base item, element 1 is \"+1\", ...")]
        public List<Effect> tiers = new();
        [Tooltip("Favors to upgrade to each next tier: element 0 buys tier 1, element 1 buys tier 2, ... " +
            "Tiers are bought in order, so each one requires the one before it.")]
        public List<int> upgradeCosts = new();

        public int MaxTier => tiers.Count - 1;

        /// <summary>Favors to go from <paramref name="currentTier"/> to the next tier, or -1 at the top.</summary>
        public int GetUpgradeCost(int currentTier) => SkillTreeRules.GetUpgradeCost(upgradeCosts, tiers.Count, currentTier);

        public Effect GetTier(int tier) => tier >= 0 && tier < tiers.Count ? tiers[tier] : null;

        /// <summary>The tier of <paramref name="item"/> in this node, or -1. Matched by EffectID.</summary>
        public int IndexOf(Effect item)
        {
            if (item == null) return -1;
            for (int i = 0; i < tiers.Count; i++)
            {
                if (tiers[i] != null && tiers[i].EffectID == item.EffectID) return i;
            }
            return -1;
        }
    }
}
