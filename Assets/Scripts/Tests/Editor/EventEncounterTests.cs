using Game.RunEncounters;
using Game.UI.RunEncounters;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Guards the run encounter events: every option is a real choice (no empty "Ignore"), and option
    /// actions do what their names say (the Arcane Box lever's "Lose1To5Grace" used to give +3 Grace).
    /// </summary>
    public class EventEncounterTests
    {
        private const string EventsFolder = "Assets/Resources/RunEncounters/EventEncounter";
        private const string ActionsFolder = EventsFolder + "/OptionActions";
        private const string RandomPoolPrefab = "Assets/Prefabs/RunEncounters/EventEncounter.prefab";
        private const string EventPanePrefab = "Assets/Prefabs/UI/RunEncounters/EventEncounterPane.prefab";

        private static List<T> LoadAll<T>(string folder) where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(a => a != null)
                .ToList();

        [Test]
        public void EveryEvent_HasTextAndAtLeastTwoOptions()
        {
            var events = LoadAll<EventEncounterData>(EventsFolder);
            Assert.IsNotEmpty(events, "No events found");
            foreach (var e in events)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.title), $"{e.name} has no title");
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.fullText), $"{e.name} has no text");
                Assert.IsNotNull(e.options, $"{e.name} has no options");
                Assert.GreaterOrEqual(e.options.Count, 2, $"{e.name} needs at least two options to be a choice");
                Assert.IsTrue(e.options.All(o => o != null), $"{e.name} has an empty option slot");
            }
        }

        [Test]
        public void EveryOption_DoesSomething()
        {
            foreach (var option in LoadAll<EventOptionsList>(EventsFolder))
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(option.OptionDescription), $"{option.name} has no label");
                Assert.IsNotEmpty(option.OptionActionsList, $"{option.name} has no actions: an option must cost or give something");
                Assert.IsTrue(option.OptionActionsList.All(a => a != null), $"{option.name} has an empty action slot");
                StringAssert.DoesNotContain("ignore", option.OptionDescription.ToLowerInvariant(),
                    $"{option.name}: Ignore options were removed");
            }
        }

        [Test]
        public void Actions_DoWhatTheirNamesSay()
        {
            var actions = LoadAll<EventOptionActionBase>(ActionsFolder)
                .Where(a => !(a is GiveRandomItemOptionAction))
                .ToList();
            Assert.IsNotEmpty(actions, "No option actions found");

            foreach (var a in actions)
            {
                float lowest = a.IsPlainReward ? a.Amount : a.AmountRange.x;
                float highest = a.IsPlainReward ? a.Amount : a.AmountRange.y;
                if (!a.IsPlainReward)
                {
                    Assert.LessOrEqual(a.AmountRange.x, a.AmountRange.y, $"{a.name}: range min is above max");
                }

                if (a.name.StartsWith("Lose"))
                {
                    Assert.Less(highest, 0f, $"{a.name} is named Lose... but can give a positive amount");
                }
                else if (a.name.StartsWith("Gain"))
                {
                    Assert.Greater(lowest, 0f, $"{a.name} is named Gain... but can take a negative amount");
                }
                else
                {
                    Assert.Fail($"{a.name}: name option actions Lose... or Gain..., so this test can check them");
                }
            }
        }

        [Test]
        public void RandomPool_HoldsTheEvents()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RandomPoolPrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {RandomPoolPrefab}");
            var controller = prefab.GetComponentInChildren<EventEncounterController>(true);
            Assert.IsNotNull(controller, "The random event prefab has no EventEncounterController");

            var list = new SerializedObject(controller).FindProperty("encountersList");
            var names = Enumerable.Range(0, list.arraySize)
                .Select(i => list.GetArrayElementAtIndex(i).objectReferenceValue as EventEncounterData)
                .ToList();
            Assert.IsTrue(names.All(e => e != null), "The random event pool has an empty slot");
            CollectionAssert.IsSubsetOf(
                new[] { "ArcaneBoxEncounter", "BanditsEncounter", "CursedChestEncounter", "FaraclarianEncounter", "HouseOfStarsEncounter" },
                names.Select(e => e.name).ToList());
        }

        [Test]
        public void EventPane_HasAPlainBackground()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EventPanePrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {EventPanePrefab}");
            var ui = prefab.GetComponentInChildren<EventEncounterUI>(true);
            Assert.IsNotNull(ui, "The event pane has no EventEncounterUI");

            var so = new SerializedObject(ui);
            Assert.IsNull(so.FindProperty("genericBackground")?.objectReferenceValue,
                "Events use a plain background, like the other windows (no art)");
            Color plain = so.FindProperty("plainBackgroundColor").colorValue;
            Assert.Greater(plain.a, 0.5f, "The plain background should be mostly opaque so the text reads");
        }
    }
}
