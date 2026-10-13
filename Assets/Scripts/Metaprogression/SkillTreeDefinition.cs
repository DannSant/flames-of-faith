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
        [SerializeField] private List<SkillTreeNode> nodes = new();

        public CharacterClass CharacterClass => characterClass;
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

        public int MaxTier => tiers.Count - 1;

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
