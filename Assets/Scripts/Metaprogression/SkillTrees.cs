using Game.Control;
using Game.Effects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Metaprogression
{
    /// <summary>Loads the class skill trees from Resources and answers "which upgrade line is this item in".</summary>
    public static class SkillTrees
    {
        public const string ResourcesPath = "Metaprogression/SkillTrees";

        private static List<SkillTreeDefinition> trees;

        public static IReadOnlyList<SkillTreeDefinition> All
        {
            get
            {
                trees ??= Resources.LoadAll<SkillTreeDefinition>(ResourcesPath).ToList();
                return trees;
            }
        }

        public static SkillTreeDefinition Get(CharacterClass characterClass) =>
            All.FirstOrDefault(t => t != null && t.CharacterClass == characterClass);

        /// <summary>The tree and node whose tiers contain <paramref name="item"/>, and the item's tier.</summary>
        public static bool TryFindNode(Effect item, out SkillTreeDefinition tree, out SkillTreeNode node, out int tier)
        {
            foreach (var candidate in All)
            {
                if (candidate != null && candidate.TryFindNode(item, out node, out tier))
                {
                    tree = candidate;
                    return true;
                }
            }

            tree = null;
            node = null;
            tier = -1;
            return false;
        }

        /// <summary>
        /// Gives the player <paramref name="item"/>, first removing any other tier of the same
        /// upgrade line they own, so a kit never holds two tiers of one item. Returns false (and
        /// does nothing) when the item isn't part of any skill tree.
        /// </summary>
        public static bool EquipTier(EffectStore store, Effect item)
        {
            if (store == null || !TryFindNode(item, out _, out var node, out _)) return false;

            var owned = store.ActiveEffects
                .Select(ei => ei.effect)
                .Where(e => e != null && node.IndexOf(e) >= 0)
                .ToList();
            foreach (var effect in owned)
            {
                store.RemoveEffect(effect);
            }

            store.AddEffect(item);
            return true;
        }
    }
}
