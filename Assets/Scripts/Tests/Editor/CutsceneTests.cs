using Game.Cutscenes;
using Game.Metaprogression;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>The cutscene rules (what plays, what counts as seen, hold to skip), the meta save, and the real assets.</summary>
    public class CutsceneTests
    {
        private static List<CutsceneData> LoadAll() => AssetDatabase.FindAssets("t:CutsceneData")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<CutsceneData>)
            .ToList();

        private static CutsceneData Make(string id, params CutscenePage[] pages)
        {
            var data = ScriptableObject.CreateInstance<CutsceneData>();
            var so = new SerializedObject(data);
            so.FindProperty("cutsceneId").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (pages.Length > 0)
            {
                ((List<CutscenePage>)typeof(CutsceneData)
                    .GetField("pages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(data)).AddRange(pages);
            }
            return data;
        }

        // --- Assets ------------------------------------------------------------------------------------

        [Test]
        public void EveryCutscene_IsInResources_WithAUniqueId()
        {
            var all = LoadAll();
            Assert.IsNotEmpty(all);
            foreach (var cutscene in all)
            {
                string path = AssetDatabase.GetAssetPath(cutscene);
                StringAssert.StartsWith($"Assets/Resources/{CutsceneDatabaseProvider.ResourceFolder}/", path, "The provider only loads this folder");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cutscene.CutsceneId), $"{cutscene.name} has no id");
                Assert.IsFalse(string.IsNullOrWhiteSpace(cutscene.Title), $"{cutscene.name} has no title");
            }
            var ids = all.Select(c => c.CutsceneId).ToList();
            CollectionAssert.AllItemsAreUnique(ids, "Repeated cutsceneId");
        }

        [Test]
        public void EveryPage_HasTextAndArt()
        {
            foreach (var cutscene in LoadAll())
            {
                Assert.IsNotEmpty(cutscene.Pages, $"{cutscene.name} has no pages");
                for (int i = 0; i < cutscene.Pages.Count; i++)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(cutscene.Pages[i].text), $"{cutscene.name} page {i + 1} has no text");
                    Assert.IsNotNull(cutscene.Pages[i].image, $"{cutscene.name} page {i + 1} has no art");
                }
            }
        }

        /// <summary>The font lacks these characters; players read Pilgrimage, never "run" (CLAUDE.md glossary).</summary>
        [Test]
        public void CutsceneText_FollowsTheTextRules()
        {
            foreach (var cutscene in LoadAll())
            {
                foreach (string text in cutscene.Pages.Select(p => p.text).Append(cutscene.Title))
                {
                    foreach (char forbidden in new[] { '—', '·', '|' })
                    {
                        Assert.IsFalse(text.Contains(forbidden), $"{cutscene.name}: '{forbidden}' in \"{text}\"");
                    }
                    Assert.IsFalse(Regex.IsMatch(text, @"\bruns?\b", RegexOptions.IgnoreCase), $"{cutscene.name}: \"run\" in \"{text}\"");
                }
            }
        }

        [Test]
        public void Opening_IsTheSevenPageScene()
        {
            var opening = LoadAll().FirstOrDefault(c => c.CutsceneId == "opening");
            Assert.IsNotNull(opening, "No scene with id 'opening'");
            Assert.AreEqual("The Pilgrimage of the Flame", opening.Title);
            Assert.AreEqual(7, opening.Pages.Count);
            Assert.AreEqual("The Pilgrimage begins.", opening.Pages[6].text.Split('\n').Last());
            Assert.AreEqual(4, opening.Pages[5].text.Split('\n').Length, "Page 6's lines appear together");
        }

        /// <summary>The string Start Run passes must name a real scene, or the opening never plays (and only logs).</summary>
        [Test]
        public void TheMainMenu_StartsTheOpeningById()
        {
            var field = typeof(Game.UI.MainMenuController).GetField("openingCutsceneId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(field);
            var go = new GameObject("Menu");
            go.SetActive(false);
            try
            {
                var controller = go.AddComponent<Game.UI.MainMenuController>();
                string id = (string)field.GetValue(controller);
                Assert.IsTrue(LoadAll().Any(c => c.CutsceneId == id), $"No scene with id '{id}'");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>A load waits on this callback, so a scene that can't play must still report its end.</summary>
        [Test]
        public void AMissingScene_LogsAndStillEnds()
        {
            var go = new GameObject("Player");
            try
            {
                var player = go.AddComponent<CutscenePlayer>();
                bool ended = false;
                LogAssert.Expect(LogType.Warning, new Regex(@"\[Cutscene\] Can't play 'nope'"));
                player.Play("nope", () => ended = true);
                Assert.IsTrue(ended);
                Assert.IsFalse(player.IsPlaying);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // --- Rules -------------------------------------------------------------------------------------

        [Test]
        public void EmptyPagesAreDropped_AndNoPagesMeansNothingPlays()
        {
            Assert.IsEmpty(CutsceneRules.GetPlayablePages(null));
            Assert.IsEmpty(CutsceneRules.GetPlayablePages(new List<CutscenePage>()));
            Assert.IsEmpty(CutsceneRules.GetPlayablePages(new List<CutscenePage> { null, new CutscenePage(), new CutscenePage { text = "   " } }));

            var page = new CutscenePage { text = "Text" };
            CollectionAssert.AreEqual(new[] { page }, CutsceneRules.GetPlayablePages(new List<CutscenePage> { new CutscenePage(), page }));
        }

        [Test]
        public void Seen_AddsOnce()
        {
            var seen = new List<string>();
            Assert.IsFalse(CutsceneRules.HasSeen(seen, "opening"));
            Assert.IsTrue(CutsceneRules.AddSeen(seen, "opening"));
            Assert.IsTrue(CutsceneRules.HasSeen(seen, "opening"));
            Assert.IsFalse(CutsceneRules.AddSeen(seen, "opening"));
            Assert.IsFalse(CutsceneRules.AddSeen(seen, ""));
            Assert.AreEqual(1, seen.Count);
        }

        [Test]
        public void Chronicle_ListsOnlySeenScenes_InTheOrderSeen()
        {
            var first = Make("first");
            var second = Make("second");
            var unseen = Make("unseen");
            try
            {
                var all = new[] { first, second, unseen };
                CollectionAssert.AreEqual(new[] { second, first }, CutsceneRules.SelectSeen(all, new[] { "second", "first", "removed-from-the-game" }));
                Assert.IsEmpty(CutsceneRules.SelectSeen(all, new string[0]));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(unseen);
            }
        }

        // --- Hold to skip ------------------------------------------------------------------------------

        [Test]
        public void Skip_FiresOnceAfterAFullHold()
        {
            var hold = new SkipHold(1f);
            hold.Tick(false, 0.1f);
            Assert.IsFalse(hold.Tick(true, 0.5f));
            Assert.AreEqual(0.5f, hold.Progress, 0.001f);
            Assert.IsTrue(hold.Tick(true, 0.5f));
            Assert.IsFalse(hold.Tick(true, 0.5f), "It fires only once");
        }

        [Test]
        public void Skip_ATapDoesNothing_AndLettingGoStartsOver()
        {
            var hold = new SkipHold(1f);
            hold.Tick(false, 0.1f);
            for (int i = 0; i < 5; i++) Assert.IsFalse(hold.Tick(true, 0.1f));
            Assert.IsFalse(hold.Tick(false, 0.1f));
            Assert.AreEqual(0f, hold.Progress);
            Assert.IsFalse(hold.Tick(true, 0.9f));
        }

        [Test]
        public void Skip_AButtonAlreadyHeldWhenTheSceneOpensDoesntCount()
        {
            var hold = new SkipHold(1f);
            Assert.IsFalse(hold.Tick(true, 5f));
            Assert.AreEqual(0f, hold.Progress);
            hold.Tick(false, 0.1f);
            Assert.IsTrue(hold.Tick(true, 1f));
        }

        // --- Meta save ---------------------------------------------------------------------------------

        [Test]
        public void OldMetaFile_LoadsWithAnEmptySeenList()
        {
            // A file from before the seen list, with its explicit null too
            foreach (string json in new[] { "{\"saveVersion\":1,\"playerName\":\"Player\"}", "{\"seenCutscenes\":null}" })
            {
                var state = JsonUtility.FromJson<MetaState>(json);
                state.EnsureLists();
                Assert.IsNotNull(state.seenCutscenes);
                Assert.IsEmpty(state.seenCutscenes);
            }
        }

        [Test]
        public void SeenList_SurvivesTheSaveFile()
        {
            var state = new MetaState();
            CutsceneRules.AddSeen(state.seenCutscenes, "opening");
            var loaded = JsonUtility.FromJson<MetaState>(JsonUtility.ToJson(state));
            CollectionAssert.AreEqual(new[] { "opening" }, loaded.seenCutscenes);
        }
    }
}
