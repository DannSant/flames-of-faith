using Game.Control;
using Game.Metaprogression;
using Game.UI;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The classes were renamed in Nov 2026 to Crusader, Sentinel and (internally) Augur; the former names
    /// are in CharacterClassNames. Saves made before still load, and no old name is left in assets or code.
    /// </summary>
    public class ClassRenameTests
    {
        // Names the classes had before; spelled in pieces so this file doesn't trip its own scan.
        private static readonly string[] FormerNames = { "War" + "rior", "Arch" + "er", "Ma" + "ge" };

        [Test]
        public void Enum_KeepsItsIndexes()
        {
            // Assets store the enum by index (class data, skill trees, event class lines).
            Assert.AreEqual(1, (int)CharacterClass.Crusader);
            Assert.AreEqual(2, (int)CharacterClass.Augur);
            Assert.AreEqual(3, (int)CharacterClass.Sentinel);
            Assert.AreEqual(4, (int)CharacterClass.Rogue);
        }

        [Test]
        public void FormerNames_MapToTheNewClasses()
        {
            Assert.AreEqual("Crusader", CharacterClassNames.Normalize(FormerNames[0]));
            Assert.AreEqual("Sentinel", CharacterClassNames.Normalize(FormerNames[1]));
            Assert.AreEqual("Augur", CharacterClassNames.Normalize(FormerNames[2]));
            Assert.AreEqual("Augur", CharacterClassNames.Normalize("Augur"));
            Assert.AreEqual("Rogue", CharacterClassNames.Normalize("Rogue"));
            Assert.IsNull(CharacterClassNames.Normalize(null));
        }

        [Test]
        public void OldMetaSave_KeepsFavorsUnlocksAndTiers()
        {
            string w = FormerNames[0], a = FormerNames[1], m = FormerNames[2];
            string json = "{\"saveVersion\":1,\"playerName\":\"Player\"," +
                $"\"favors\":[{{\"characterClass\":\"{w}\",\"amount\":12}},{{\"characterClass\":\"{a}\",\"amount\":5}},{{\"characterClass\":\"{m}\",\"amount\":3}}]," +
                $"\"unlockedClasses\":[\"{a}\",\"{m}\"]," +
                $"\"nodeTiers\":[{{\"characterClass\":\"{w}\",\"nodeId\":\"weapon\",\"tier\":2}},{{\"characterClass\":\"{a}\",\"nodeId\":\"dash\",\"tier\":1}}]}}";

            var state = JsonUtility.FromJson<MetaState>(json);
            state.MigrateClassNames();

            Assert.AreEqual(12, state.favors.Single(f => f.characterClass == "Crusader").amount);
            Assert.AreEqual(5, state.favors.Single(f => f.characterClass == "Sentinel").amount);
            Assert.AreEqual(3, state.favors.Single(f => f.characterClass == "Augur").amount);
            CollectionAssert.AreEquivalent(new[] { "Sentinel", "Augur" }, state.unlockedClasses);
            Assert.AreEqual(2, state.nodeTiers.Single(n => n.characterClass == "Crusader" && n.nodeId == "weapon").tier);
            Assert.AreEqual(1, state.nodeTiers.Single(n => n.characterClass == "Sentinel" && n.nodeId == "dash").tier);

            // The next save writes only the new names.
            string saved = JsonUtility.ToJson(state);
            foreach (string former in FormerNames) StringAssert.DoesNotContain($"\"{former}\"", saved);
        }

        [Test]
        public void MetaSave_WithBothNames_Merges()
        {
            var state = new MetaState();
            state.favors.Add(new ClassFavors { characterClass = FormerNames[0], amount = 4 });
            state.favors.Add(new ClassFavors { characterClass = "Crusader", amount = 6 });
            state.unlockedClasses.AddRange(new[] { FormerNames[1], "Sentinel" });
            state.nodeTiers.Add(new NodeTierState { characterClass = FormerNames[0], nodeId = "special", tier = 3 });
            state.nodeTiers.Add(new NodeTierState { characterClass = "Crusader", nodeId = "special", tier = 1 });

            state.MigrateClassNames();

            Assert.AreEqual(10, state.favors.Single().amount, "Favors add up");
            CollectionAssert.AreEqual(new[] { "Sentinel" }, state.unlockedClasses);
            Assert.AreEqual(3, state.nodeTiers.Single().tier, "A node keeps its highest tier");
        }

        [Test]
        public void NoAssetOrFolder_IsNamedAfterAFormerClass()
        {
            var found = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && !p.StartsWith("Assets/AssetPackages/"))
                .Where(p => Regex.IsMatch(Path.GetFileNameWithoutExtension(p), "War" + "rior|Arch" + "er|Centinel", RegexOptions.IgnoreCase))
                .ToList();
            Assert.IsEmpty(found, "Rename these (keep their GUIDs):\n" + string.Join("\n", found));
        }

        [Test]
        public void NoScript_NamesAFormerClass()
        {
            // The alias table is the one place that must still spell the old names.
            var pattern = new Regex(@"War" + "rior|Arch" + @"er|\bMa" + @"ge\b", RegexOptions.IgnoreCase);
            var found = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Replace('\\', '/').EndsWith("Control/CharacterClassData.cs"))
                .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (f, i, line)))
                .Where(x => pattern.IsMatch(x.line))
                .Select(x => $"{x.f}:{x.i + 1}: {x.line.Trim()}")
                .ToList();
            Assert.IsEmpty(found, string.Join("\n", found));
        }

        [Test]
        public void ClassCards_CallExistingMethods()
        {
            // A persistent listener stores the method's name: after a rename it would silently do nothing.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Menus/CharacterSelect.prefab");
            Assert.IsNotNull(prefab);
            var cards = prefab.GetComponentsInChildren<ClassSelectHoverButton>(true);
            Assert.AreEqual(3, cards.Length);
            foreach (var card in cards)
            {
                var click = card.GetComponent<Button>().onClick;
                Assert.Greater(click.GetPersistentEventCount(), 0, card.name);
                for (int i = 0; i < click.GetPersistentEventCount(); i++)
                {
                    string method = click.GetPersistentMethodName(i);
                    Assert.IsNotNull(typeof(MainMenuController).GetMethod(method, BindingFlags.Public | BindingFlags.Instance, null, System.Type.EmptyTypes, null),
                        $"{card.name} calls MainMenuController.{method}, which doesn't exist");
                    StringAssert.EndsWith(card.ClassData.characterClass.ToString(), method, $"{card.name} chooses another class");
                }
            }
        }

        [Test]
        public void ClassData_UsesTheNewNames()
        {
            foreach (var cls in new[] { CharacterClass.Crusader, CharacterClass.Sentinel, CharacterClass.Augur })
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterClassData>($"Assets/Resources/CharacterData/{cls}.asset");
                Assert.IsNotNull(data, $"Missing CharacterData/{cls}.asset");
                Assert.AreEqual(cls, data.characterClass);
                Assert.AreEqual(cls.ToString(), data.characterName);
            }
        }
    }
}
