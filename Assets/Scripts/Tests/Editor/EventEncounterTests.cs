using Game.Control;
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
                if (e.UsesOptions)
                {
                    Assert.GreaterOrEqual(e.options.Count(o => o != null && o.onlyForClass == CharacterClass.None), 2,
                        $"{e.name} needs at least two options for everyone to be a choice");
                    Assert.IsTrue(e.options.All(o => o != null), $"{e.name} has an empty option slot");
                    continue;
                }

                Assert.IsNotNull(e.legacyOptions, $"{e.name} has no options");
                Assert.GreaterOrEqual(e.legacyOptions.Count, 2, $"{e.name} needs at least two options to be a choice");
                Assert.IsTrue(e.legacyOptions.All(o => o != null), $"{e.name} has an empty option slot");
            }
        }

        [Test]
        public void EveryEvent_HasASoundStepChain()
        {
            foreach (var e in LoadAll<EventEncounterData>(EventsFolder))
            {
                var problems = EventEncounterRules.Validate(e);
                Assert.IsEmpty(problems, $"{e.name}: {string.Join("; ", problems)}");
            }
        }

        // --- Step chain rules (pure, on events built in memory) ----------------------------------------

        private static EventOption Option(string nextStepId = null) => new EventOption
        {
            label = "Go",
            result = new EventBranch { resultText = "Done.", nextStepId = nextStepId }
        };

        private static EventStep Step(string id, string nextStepId = null) =>
            new EventStep { id = id, prompt = id, options = new List<EventOption> { Option(nextStepId) } };

        private static EventEncounterData Event(string firstNextStep, params EventStep[] steps)
        {
            var data = ScriptableObject.CreateInstance<EventEncounterData>();
            data.options = new List<EventOption> { Option(firstNextStep), Option() };
            data.steps = steps.ToList();
            return data;
        }

        [Test]
        public void Rules_ThreeStepsDeep_IsSound()
        {
            var data = Event("a", Step("a", "b"), Step("b", "c"), Step("c"));
            Assert.IsEmpty(EventEncounterRules.Validate(data));
            Assert.AreEqual(3, EventEncounterRules.GetMaxDepth(data));
        }

        [Test]
        public void Rules_FourStepsDeep_IsTooDeep()
        {
            var data = Event("a", Step("a", "b"), Step("b", "c"), Step("c", "d"), Step("d"));
            Assert.IsTrue(EventEncounterRules.Validate(data).Any(p => p.Contains("deep")));
        }

        [Test]
        public void Rules_MissingStep_Loop_DuplicateAndUnreachable_AreReported()
        {
            Assert.IsTrue(EventEncounterRules.Validate(Event("missing")).Any(p => p.Contains("doesn't exist")));
            Assert.IsTrue(EventEncounterRules.Validate(Event("a", Step("a", "a"))).Any(p => p.Contains("loops")));
            Assert.IsTrue(EventEncounterRules.Validate(Event("a", Step("a"), Step("a"))).Any(p => p.Contains("twice")));
            Assert.IsTrue(EventEncounterRules.Validate(Event(null, Step("orphan"))).Any(p => p.Contains("never reached")));
        }

        [Test]
        public void Rules_CheckBranchesLeadToSteps()
        {
            var data = Event(null, Step("whisper"));
            data.options[0] = new EventOption
            {
                label = "Reach",
                outcome = EventOutcomeKind.Check,
                success = new EventBranch { resultText = "Got it." },
                failure = new EventBranch { nextStepId = "whisper" }
            };
            Assert.IsEmpty(EventEncounterRules.Validate(data));
            Assert.AreEqual(1, EventEncounterRules.GetMaxDepth(data));
        }

        [Test]
        public void ClassTexts_ApplyOnlyToTheirClass()
        {
            var data = ScriptableObject.CreateInstance<EventEncounterData>();
            data.fullText = "Intro.";
            data.classIntroLines = new List<ClassText> { new ClassText { characterClass = CharacterClass.Mage, text = "Werlem." } };
            Assert.AreEqual("Intro.\n\nWerlem.", data.GetIntro(CharacterClass.Mage));
            Assert.AreEqual("Intro.", data.GetIntro(CharacterClass.Warrior));

            var option = new EventOption
            {
                label = "Tell her whose sword it is.",
                classLabels = new List<ClassText> { new ClassText { characterClass = CharacterClass.Warrior, text = "Tell her who you are." } },
                onlyForClass = CharacterClass.Archer
            };
            Assert.AreEqual("Tell her who you are.", option.GetLabel(CharacterClass.Warrior));
            Assert.AreEqual("Tell her whose sword it is.", option.GetLabel(CharacterClass.Mage));
            Assert.IsTrue(option.IsOfferedTo(CharacterClass.Archer));
            Assert.IsFalse(option.IsOfferedTo(CharacterClass.Warrior));
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
        public void EventPane_UsesTheWindowFrameNotABackground()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EventPanePrefab);
            Assert.IsNotNull(prefab, $"Missing prefab {EventPanePrefab}");
            var ui = prefab.GetComponentInChildren<EventEncounterUI>(true);
            Assert.IsNotNull(ui, "The event pane has no EventEncounterUI");

            // The framed window is the background, like the other windows; event art goes in the side panel
            Assert.IsFalse(prefab.GetComponentsInChildren<Transform>(true).Any(t => t.name == "EventBackground"),
                "The old plain-color background image is still in the event pane");
            Assert.IsNotNull(new SerializedObject(ui).FindProperty("artPanel")?.objectReferenceValue, "EventEncounterUI.artPanel not set");
        }
    }
}
