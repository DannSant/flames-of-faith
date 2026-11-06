using Game.Control;
using Game.Progression;
using Game.UI;
using Game.UI.Style;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// The class select's Abilities / Lore / Playstyle tabs (Nov 2026): the class texts, the view the menu
    /// builds, the prefab (tabs, masked list, row templates, tooltip), the gamepad links and the scene wiring.
    /// </summary>
    public class ClassDetailsTabsTests
    {
        private const string ClassSelectPath = "Assets/Prefabs/UI/Menus/CharacterSelect.prefab";
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private static readonly CharacterClass[] Classes = { CharacterClass.Crusader, CharacterClass.Sentinel, CharacterClass.Augur };
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
            created.Clear();
        }

        private static CharacterClassData Load(CharacterClass cls)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterClassData>($"Assets/Resources/CharacterData/{cls}.asset");
            Assert.IsNotNull(data, $"Missing CharacterData/{cls}");
            return data;
        }

        // --- Texts ---------------------------------------------------------------------------------------------

        [TestCase(CharacterClass.Crusader, "Ayteel Alamoh, Crusader of the Faith")]
        [TestCase(CharacterClass.Sentinel, "Faelin Vidal, Sentinel of Abidor")]
        [TestCase(CharacterClass.Augur, "Werlem Feth, Augur")]
        public void EveryClass_HasLoreAndPlaystyle(CharacterClass cls, string opening)
        {
            var data = Load(cls);
            Assert.IsFalse(string.IsNullOrWhiteSpace(data.lore), $"{cls} has no lore");
            Assert.IsFalse(string.IsNullOrWhiteSpace(data.playstyle), $"{cls} has no playstyle");
            StringAssert.StartsWith(opening, data.lore, "The lore starts with the pilgrim's name and title");

            var paragraphs = ClassDetailsTabsUI.Paragraphs(data.lore);
            Assert.That(paragraphs.Count, Is.InRange(3, 4), "The name line, then two or three short paragraphs");
            foreach (string text in new[] { data.lore, data.playstyle })
            {
                // The font has no em or en dash
                Assert.IsFalse(text.Contains('—') || text.Contains('–'), $"{cls}: no em or en dash");
            }
        }

        // --- Pure ----------------------------------------------------------------------------------------------

        [Test]
        public void Paragraphs_SplitOnBlankLines()
        {
            CollectionAssert.AreEqual(new[] { "Title", "One\nstill one.", "Two" },
                ClassDetailsTabsUI.Paragraphs("Title\r\n\r\nOne\nstill one.\n\n\n\n  Two  \n"));
            CollectionAssert.IsEmpty(ClassDetailsTabsUI.Paragraphs("  "));
            CollectionAssert.IsEmpty(ClassDetailsTabsUI.Paragraphs(null));
        }

        [Test]
        public void StatLine_NamesTheStatThenWhy()
        {
            Assert.AreEqual("<b>Armor</b>: Take less damage.",
                ClassDetailsTabsUI.FormatStat(new ClassStatEntry { statName = "Armor", justification = " Take less damage. " }));
            Assert.AreEqual("<b>Luck</b>", ClassDetailsTabsUI.FormatStat(new ClassStatEntry { statName = "Luck" }));
        }

        [Test]
        public void Details_ShowTheKitAtItsTier_TheTextsAndTheStatGuide()
        {
            foreach (var cls in Classes)
            {
                var data = Load(cls);
                var view = MainMenuController.BuildDetails(data);
                Assert.AreEqual(6, view.kit.Count, $"{cls}: weapon, off-hand, Emblem and three Glyphs");
                Assert.IsTrue(view.kit.All(k => k.icon != null && !string.IsNullOrEmpty(k.name) && !string.IsNullOrEmpty(k.description)),
                    $"{cls}: every kit item has an icon, a name and a description");
                Assert.AreEqual(data.lore, view.lore);
                Assert.AreEqual(data.playstyle, view.playstyle);

                Assert.AreEqual(StatRole.Key, view.stats[0].role, $"{cls}: the Key stat comes first");
                Assert.IsTrue(view.stats.Skip(1).All(s => s.role == StatRole.Complementary), $"{cls}: then the Complementary ones");
                Assert.AreEqual(data.statPriorities.Count(p => p.role != StatRole.None), view.stats.Count);
            }
        }

        [Test]
        public void Navigation_LinksCardsTabsRowsAndButtons()
        {
            Selectable Make(string name)
            {
                var go = new GameObject(name);
                created.Add(go);
                return go.AddComponent<Button>();
            }
            var cards = new[] { Make("Card0"), Make("Card1"), Make("Card2") };
            var tabs = new[] { Make("Abilities"), Make("Lore"), Make("Playstyle") };
            var rows = new[] { Make("Row0"), Make("Row1") };
            var tree = Make("SkillTree");
            var back = Make("Back");
            var start = Make("Start");

            ClassSelectNavigation.Link(cards, 1, tabs, 2, rows, tree, back, start);

            Assert.AreSame(cards[1], cards[0].navigation.selectOnDown);
            Assert.AreSame(back, cards[2].navigation.selectOnDown, "Down from the last card reaches Back");
            Assert.AreSame(tabs[2], cards[0].navigation.selectOnRight, "Right from a card goes to the current tab");
            Assert.AreSame(cards[1], tabs[0].navigation.selectOnLeft, "Left from the tabs returns to the chosen card");
            Assert.AreSame(tabs[1], tabs[0].navigation.selectOnRight);
            Assert.AreSame(rows[0], tabs[1].navigation.selectOnDown, "Down from a tab enters its list");
            Assert.AreSame(tabs[2], rows[0].navigation.selectOnUp, "Up from the first row returns to the current tab");
            Assert.AreSame(tree, rows[1].navigation.selectOnDown);
            Assert.AreSame(rows[1], tree.navigation.selectOnUp);
            Assert.AreSame(back, tree.navigation.selectOnDown);
            Assert.AreSame(start, back.navigation.selectOnDown);
            Assert.AreSame(back, start.navigation.selectOnUp, "From Start Run back up");
            Assert.AreSame(cards[1], start.navigation.selectOnLeft);
            foreach (var s in cards.Concat(tabs).Concat(rows).Concat(new[] { tree, back, start }))
            {
                Assert.AreEqual(UnityEngine.UI.Navigation.Mode.Explicit, s.navigation.mode, $"{s.name}: Automatic would reach the hidden panels");
            }

            ClassSelectNavigation.Link(cards, 0, tabs, 1, new Selectable[0], tree, back, start);
            Assert.AreSame(tree, tabs[1].navigation.selectOnDown, "A tab with no rows goes down to Skill Tree");
            Assert.AreSame(tabs[1], tree.navigation.selectOnUp);
        }

        // --- Prefab ----------------------------------------------------------------------------------------

        private ClassDetailsTabsUI Instantiate(out GameObject root)
        {
            root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ClassSelectPath));
            created.Add(root);
            var tabs = root.GetComponentInChildren<ClassDetailsTabsUI>(true);
            Assert.IsNotNull(tabs);
            return tabs;
        }

        [Test]
        public void Prefab_HasThreeTabs_AMaskedList_AndRowTemplates()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ClassSelectPath);
            var tabsUi = prefab.GetComponentInChildren<ClassDetailsTabsUI>(true);
            var so = new SerializedObject(tabsUi);

            var tabButtons = so.FindProperty("tabButtons");
            Assert.AreEqual(3, tabButtons.arraySize);
            string[] labels = { "Abilities", "Lore", "Playstyle" };
            for (int i = 0; i < 3; i++)
            {
                var button = (Button)tabButtons.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.IsNotNull(button, labels[i]);
                Assert.AreEqual(labels[i], button.GetComponentInChildren<TMP_Text>(true).text);
                Assert.AreEqual(UIFontSize.Small, button.GetComponentInChildren<UIStyleText>(true).Size);
            }

            var scroll = (ScrollRect)so.FindProperty("scrollRect").objectReferenceValue;
            Assert.IsNotNull(scroll);
            Assert.IsNotNull(scroll.viewport.GetComponent<RectMask2D>(), "The list is masked");
            Assert.AreSame(so.FindProperty("content").objectReferenceValue, scroll.content);
            Assert.AreEqual(1f, scroll.GetComponent<LayoutElement>().flexibleHeight, "The list takes the room left, so it can't push the buttons");
            Assert.AreEqual(0f, scroll.GetComponent<LayoutElement>().preferredHeight);

            foreach (string template in new[] { "kitRowTemplate", "textRowTemplate", "statRowTemplate" })
            {
                var row = (ClassDetailsRowUI)so.FindProperty(template).objectReferenceValue;
                Assert.IsNotNull(row, template);
                Assert.IsFalse(row.gameObject.activeInHierarchy, $"{template} is an inactive template");
                Assert.IsNotNull(row.GetComponent<Selectable>(), $"{template}: rows are selectable for the gamepad");
                foreach (var graphic in row.GetComponentsInChildren<MaskableGraphic>(true))
                {
                    Assert.IsTrue(graphic.maskable, $"{template}/{graphic.name} draws outside the list");
                }
            }
            Assert.IsNotNull(new SerializedObject(so.FindProperty("kitRowTemplate").objectReferenceValue).FindProperty("icon").objectReferenceValue);
            Assert.IsNotNull(new SerializedObject(so.FindProperty("statRowTemplate").objectReferenceValue).FindProperty("star").objectReferenceValue);

            Assert.AreEqual(3, so.FindProperty("cards").arraySize, "The cards are linked for the gamepad");
            foreach (string field in new[] { "skillTreeButton", "backButton", "startRunButton" })
            {
                Assert.IsNotNull(so.FindProperty(field).objectReferenceValue, field);
            }
        }

        [Test]
        public void Tabs_KeepTheirTabWhenTheClassChanges()
        {
            var tabs = Instantiate(out _);
            var crusader = MainMenuController.BuildDetails(Load(CharacterClass.Crusader));
            var augur = MainMenuController.BuildDetails(Load(CharacterClass.Augur));

            tabs.Show(crusader);
            Assert.AreEqual(ClassDetailsTab.Abilities, tabs.CurrentTab, "Abilities is the default tab");
            Assert.AreEqual(6, tabs.Rows.Count, "A row per kit item");

            tabs.SelectTab(ClassDetailsTab.Lore);
            tabs.Show(augur);
            Assert.AreEqual(ClassDetailsTab.Lore, tabs.CurrentTab, "The tab stays while another class is shown");
            CollectionAssert.AreEqual(ClassDetailsTabsUI.Paragraphs(augur.lore), tabs.Rows.Select(r => r.Text).ToList());

            tabs.SelectTab(ClassDetailsTab.Playstyle);
            Assert.AreEqual(ClassDetailsTabsUI.Paragraphs(augur.playstyle).Count + 1 + augur.stats.Count, tabs.Rows.Count,
                "The playstyle, a header, then a row per recommended stat");
        }

        // --- Scene -----------------------------------------------------------------------------------------

        [Test]
        public void Scene_HasTheSharedTooltip_AndWiresTheTabs()
        {
            string yaml = System.IO.File.ReadAllText(ScenePath);
            string paneGuid = AssetDatabase.AssetPathToGUID("Assets/Prefabs/UI/Misc/GeneralTooltipPane.prefab");
            StringAssert.Contains(paneGuid, yaml, "The main menu has no shared tooltip");
            StringAssert.IsMatch(@"propertyPath: tooltipPane\s+value:\s+objectReference: \{fileID: [1-9]", yaml, "The tabs have no tooltip pane");
            StringAssert.Contains("detailsTabs: {fileID: ", yaml);
            StringAssert.DoesNotMatch(@"detailsTabs: \{fileID: 0\}", yaml, "MainMenuController.detailsTabs is not wired");
        }
    }
}
