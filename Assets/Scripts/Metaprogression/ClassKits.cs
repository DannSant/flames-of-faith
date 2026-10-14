using Game.Control;
using Game.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Metaprogression
{
    /// <summary>
    /// A class's starting items: for each node of its skill tree, the tier the player has bought
    /// (tier 0 without meta progress). The single source of starting items.
    /// </summary>
    public static class ClassKits
    {
        public const string WeaponNodeId = "weapon";

        public static List<Effect> Resolve(CharacterClass characterClass)
        {
            var kit = new List<Effect>();
            var tree = SkillTrees.Get(characterClass);
            if (tree == null)
            {
                Debug.LogError($"[ClassKits] No skill tree for {characterClass}: the player starts with no items.");
                return kit;
            }

            foreach (var node in tree.Nodes)
            {
                var item = GetCurrentItem(characterClass, node);
                if (item != null) kit.Add(item);
            }
            return kit;
        }

        /// <summary>The item of <paramref name="node"/> at the tier the player has bought.</summary>
        public static Effect GetCurrentItem(CharacterClass characterClass, SkillTreeNode node)
        {
            if (node == null) return null;
            var meta = MetaProgressionManager.Instance;
            int tier = meta != null ? meta.GetNodeTier(characterClass, node) : 0;
            return node.GetTier(tier);
        }

        /// <summary>The class's weapon at its current tier (for the class select screen).</summary>
        public static Effect GetWeapon(CharacterClass characterClass)
        {
            var tree = SkillTrees.Get(characterClass);
            if (tree == null) return null;
            foreach (var node in tree.Nodes)
            {
                if (node.nodeId == WeaponNodeId) return GetCurrentItem(characterClass, node);
            }
            return null;
        }
    }
}
