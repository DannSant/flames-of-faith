using Game.Control;
using Game.Effects;
using Game.Effects.Abilities;
using Game.Metaprogression;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Guards the class skill trees (upgrade lines of the kit items) and the ability tier tables.</summary>
    public class SkillTreeTests
    {
        private static readonly (CharacterClass characterClass, string prefabPath)[] Classes =
        {
            (CharacterClass.Warrior, "Assets/Prefabs/Player/PlayerWarrior.prefab"),
            (CharacterClass.Archer, "Assets/Prefabs/Player/PlayerArcher.prefab"),
            (CharacterClass.Mage, "Assets/Prefabs/Player/PlayerAugur.prefab"),
        };

        private static SkillTreeDefinition GetTree(CharacterClass characterClass)
        {
            var tree = Resources.LoadAll<SkillTreeDefinition>(SkillTrees.ResourcesPath)
                .FirstOrDefault(t => t.CharacterClass == characterClass);
            Assert.IsNotNull(tree, $"No skill tree for {characterClass} in Resources/{SkillTrees.ResourcesPath}");
            return tree;
        }


        // --- Trees -------------------------------------------------------------------------

        [Test]
        public void EveryClassHasExactlyOneTree()
        {
            var trees = Resources.LoadAll<SkillTreeDefinition>(SkillTrees.ResourcesPath);
            foreach (var (characterClass, _) in Classes)
            {
                Assert.AreEqual(1, trees.Count(t => t.CharacterClass == characterClass), $"{characterClass} trees");
            }
        }

        [Test]
        public void Nodes_HaveUniqueIdsAndOrderedTiers()
        {
            foreach (var (characterClass, _) in Classes)
            {
                var tree = GetTree(characterClass);
                var ids = new HashSet<string>();
                foreach (var node in tree.Nodes)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(node.nodeId), $"{tree.name}: a node has no id");
                    Assert.IsTrue(ids.Add(node.nodeId), $"{tree.name}: duplicate node id {node.nodeId}");
                    Assert.IsNotEmpty(node.tiers, $"{tree.name}/{node.nodeId}: no tiers");

                    for (int i = 0; i < node.tiers.Count; i++)
                    {
                        var item = node.tiers[i];
                        Assert.IsNotNull(item, $"{tree.name}/{node.nodeId}: tier {i} is empty");
                        Assert.AreEqual(i, item.Tier, $"{tree.name}/{node.nodeId}: {item.name} is in slot {i} but its Tier is {item.Tier}");
                        Assert.IsTrue(item.IsStartingOnly, $"{tree.name}/{node.nodeId}: {item.name} isn't Starting Only");
                    }
                }
            }
        }

        [Test]
        public void NoItemIsInTwoNodes()
        {
            var seen = new Dictionary<string, string>();
            foreach (var (characterClass, _) in Classes)
            {
                var tree = GetTree(characterClass);
                foreach (var node in tree.Nodes)
                {
                    foreach (var item in node.tiers.Where(i => i != null))
                    {
                        string where = $"{tree.name}/{node.nodeId}";
                        Assert.IsFalse(seen.TryGetValue(item.EffectID, out var other),
                            $"{item.name} is in both {other} and {where}");
                        seen[item.EffectID] = where;
                    }
                }
            }
        }

        [Test]
        public void ClassPrefabs_MatchTheirTreeClass()
        {
            foreach (var (characterClass, prefabPath) in Classes)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.IsNotNull(prefab, $"Missing prefab {prefabPath}");
                var classManager = prefab.GetComponentInChildren<Game.Progression.PlayerClassManager>(true);
                Assert.IsNotNull(classManager, $"{prefabPath} has no PlayerClassManager");
                Assert.AreEqual(characterClass, classManager.CharacterClass, $"{prefabPath} plays the wrong class");
                GetTree(characterClass);
            }
        }

        [Test]
        public void OnlyTheWarriorStartsUnlocked_OthersCostFavors()
        {
            foreach (var (characterClass, _) in Classes)
            {
                var tree = GetTree(characterClass);
                bool shouldStartUnlocked = characterClass == CharacterClass.Warrior;
                Assert.AreEqual(shouldStartUnlocked, tree.UnlockedByDefault, $"{tree.name}: unlocked by default");
                if (!shouldStartUnlocked)
                {
                    Assert.Greater(tree.UnlockCost, 0, $"{tree.name}: a locked class must cost Favors");
                }
            }
        }

        [Test]
        public void Nodes_HaveACostForEveryUpgrade()
        {
            foreach (var (characterClass, _) in Classes)
            {
                var tree = GetTree(characterClass);
                foreach (var node in tree.Nodes)
                {
                    string where = $"{tree.name}/{node.nodeId}";
                    Assert.AreEqual(node.tiers.Count - 1, node.upgradeCosts.Count, $"{where}: one cost per upgrade");
                    for (int i = 0; i < node.upgradeCosts.Count; i++)
                    {
                        Assert.Greater(node.upgradeCosts[i], 0, $"{where}: upgrade to tier {i + 1} is free");
                        if (i > 0)
                        {
                            Assert.GreaterOrEqual(node.upgradeCosts[i], node.upgradeCosts[i - 1], $"{where}: tier {i + 1} is cheaper than tier {i}");
                        }
                    }
                }
            }
        }

        [Test]
        public void AbilityLines_ShareOneAbilityWithEnoughTierRows()
        {
            foreach (var (characterClass, _) in Classes)
            {
                var tree = GetTree(characterClass);
                foreach (var node in tree.Nodes)
                {
                    var abilities = node.tiers.Where(i => i != null)
                        .Select(i => i.Behaviors.OfType<AbilityBehavior>().FirstOrDefault())
                        .ToList();
                    var first = abilities.FirstOrDefault();
                    if (first == null)
                    {
                        Assert.IsTrue(abilities.All(a => a == null), $"{tree.name}/{node.nodeId}: only some tiers carry an ability");
                        continue;
                    }

                    Assert.IsTrue(abilities.All(a => a == first),
                        $"{tree.name}/{node.nodeId}: every tier must use the same ability asset ({first.name})");
                    // Otherwise the top tiers would silently reuse the last row.
                    Assert.GreaterOrEqual(first.TierCount, node.tiers.Count,
                        $"{first.name} has {first.TierCount} tier rows but {tree.name}/{node.nodeId} has {node.tiers.Count} tiers");
                }
            }
        }

        // --- Ability tier tables -----------------------------------------------------------

        [Test]
        public void AbilityTiers_NeverGetWeaker()
        {
            var abilities = AssetDatabase.FindAssets("t:AbilityBehavior")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AbilityBehavior>)
                .Where(a => a != null)
                .ToList();
            Assert.IsNotEmpty(abilities, "No ability assets found");

            foreach (var ability in abilities)
            {
                var rows = ((IEnumerable)ability.GetType().GetProperty("Tiers").GetValue(ability)).Cast<object>().ToList();
                for (int i = 1; i < rows.Count; i++)
                {
                    foreach (var field in rows[i].GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (field.FieldType != typeof(int) && field.FieldType != typeof(float)) continue;

                        double previous = Convert.ToDouble(field.GetValue(rows[i - 1]));
                        double current = Convert.ToDouble(field.GetValue(rows[i]));
                        bool lowerIsBetter = field.GetCustomAttribute<LowerIsBetterAttribute>() != null;
                        bool weaker = lowerIsBetter ? current > previous : current < previous;
                        Assert.IsFalse(weaker, $"{ability.name}: {field.Name} gets weaker at tier {i} ({previous} -> {current})");
                    }
                }
            }
        }
    }
}
