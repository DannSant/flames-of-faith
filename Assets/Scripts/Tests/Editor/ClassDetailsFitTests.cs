using Game.Control;
using Game.UI;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>
    /// Every class's Abilities, Lore and Playstyle tab fits the class select at once, with no scrolling
    /// (owner, Nov 2026). Lays the real prefab out at the 1920 x 1080 reference size and compares each tab's
    /// rows with the list's visible height.
    /// </summary>
    public class ClassDetailsFitTests
    {
        private const string ClassSelectPath = "Assets/Prefabs/UI/Menus/CharacterSelect.prefab";
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
            created.Clear();
        }

        [Test]
        public void EveryTab_FitsWithoutScrolling()
        {
            var screen = new GameObject("Screen", typeof(RectTransform), typeof(Canvas));
            created.Add(screen);
            var screenRect = (RectTransform)screen.transform;
            screenRect.sizeDelta = new Vector2(1920f, 1080f);

            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ClassSelectPath), screen.transform);
            var tabs = root.GetComponentInChildren<ClassDetailsTabsUI>(true);
            var scroll = root.GetComponentInChildren<ScrollRect>(true);
            var classDesc = System.Linq.Enumerable.First(root.GetComponentsInChildren<TMPro.TMP_Text>(true), t => t.name == "ClassDesc");

            var report = new List<string>();
            var tooTall = new List<string>();
            foreach (var cls in new[] { CharacterClass.Crusader, CharacterClass.Sentinel, CharacterClass.Augur })
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterClassData>($"Assets/Resources/CharacterData/{cls}.asset");
                classDesc.text = Game.Combat.DamageTypeColorHelper.ColorizeMetaTags(data.classDescription);
                tabs.Show(MainMenuController.BuildDetails(data));
                foreach (ClassDetailsTab tab in System.Enum.GetValues(typeof(ClassDetailsTab)))
                {
                    tabs.SelectTab(tab);
                    Canvas.ForceUpdateCanvases();
                    for (int i = 0; i < 3; i++) LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root.transform);
                    float needed = LayoutUtility.GetPreferredHeight(scroll.content);
                    float room = scroll.viewport.rect.height;
                    report.Add($"{cls}/{tab}: needs {needed:0} of {room:0}");
                    if (needed > room + 0.5f) tooTall.Add($"{cls}/{tab}: needs {needed:0}, has {room:0}");
                }
            }
            var details = (RectTransform)tabs.transform;
            report.Add($"Details {details.rect.height:0} x {details.rect.width:0}");
            foreach (RectTransform child in details) report.Add($"  {child.name}: {child.rect.height:0} (pref {LayoutUtility.GetPreferredHeight(child):0})");
            TestContext.WriteLine(string.Join("\n", report));
            Assert.IsEmpty(tooTall, "These tabs would scroll (shorten the text, or give the list more room in MainMenuRestyler):\n" + string.Join("\n", tooTall));
        }
    }
}
