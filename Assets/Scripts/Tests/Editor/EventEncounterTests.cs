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
                Assert.GreaterOrEqual(e.options.Count(o => o != null && o.onlyForClass == CharacterClass.None), 2,
                    $"{e.name} needs at least two options for everyone to be a choice");
                Assert.IsTrue(e.options.All(o => o != null), $"{e.name} has an empty option slot");
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
        public void EveryOption_HasALabelAndNoEmptyActionSlot()
        {
            foreach (var e in LoadAll<EventEncounterData>(EventsFolder))
            {
                foreach (var (option, _, _) in AllOptions(e))
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(option.label), $"{e.name} has an option with no label");
                    StringAssert.DoesNotContain("ignore", option.label.ToLowerInvariant(), $"{e.name}: Ignore options were removed");
                    foreach (var branch in option.Branches())
                    {
                        Assert.IsTrue(branch.actions.All(a => a != null), $"{e.name} '{option.label}' has an empty action slot");
                    }
                }
            }
        }

        [Test]
        public void EveryAction_IsUsedByAnEvent()
        {
            var used = new HashSet<EventOptionActionBase>(LoadAll<EventEncounterData>(EventsFolder)
                .SelectMany(AllOptions)
                .SelectMany(o => o.option.Branches())
                .SelectMany(b => b.actions));
            foreach (var action in LoadAll<EventOptionActionBase>(ActionsFolder))
            {
                Assert.IsTrue(used.Contains(action), $"{action.name} is used by no event: delete it (no dead data)");
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
            CollectionAssert.AreEquivalent(
                new[] { "arcaneBox", "bandits", "cursedChest", "faraclarianClan", "houseOfStars", "liah", "aylinean", "starReader", "lastTorch" },
                names.Select(e => e.eventName).ToList(),
                "Act I's random pool holds the 9 events; the Wayside Shrine has its own node");
        }

        // --- The authored events (spec: docs/features/run-encounter-events.md) -----------------------

        private const string ShrineId = "holyAltar";
        private const string AylineanId = "aylinean";
        private static readonly System.Text.RegularExpressions.Regex Digit = new(@"\d");

        private static List<EventEncounterData> Events() => LoadAll<EventEncounterData>(EventsFolder);

        /// <summary>Every option of an event, first screen and steps, with where it sits.</summary>
        private static IEnumerable<(EventOption option, string where, bool firstScreen)> AllOptions(EventEncounterData e)
        {
            foreach (var o in e.options.Where(o => o != null)) yield return (o, $"{e.name} '{o.label}'", true);
            foreach (var s in e.steps.Where(s => s != null))
            {
                foreach (var o in s.options.Where(o => o != null)) yield return (o, $"{e.name} step '{s.id}' '{o.label}'", false);
            }
        }

        private static bool IsCost(EventOptionActionBase a) =>
            a.Kind != EventResultKind.Item && (a.IsPlainReward ? a.Amount < 0f : a.AmountRange.x < 0f);

        /// <summary>True when some outcome of the option, or of the steps it leads to, costs something.</summary>
        private static bool RisksACost(EventEncounterData e, EventOption option, HashSet<string> visited = null)
        {
            visited ??= new HashSet<string>();
            foreach (var branch in option.Branches())
            {
                if (branch.actions.Any(a => a != null && IsCost(a))) return true;
                if (!branch.HasNextStep || !visited.Add(branch.nextStepId)) continue;
                var step = e.FindStep(branch.nextStepId);
                if (step != null && step.options.Any(o => o != null && RisksACost(e, o, visited))) return true;
            }
            return false;
        }

        [Test]
        public void EveryEvent_HasAnId()
        {
            foreach (var e in Events())
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.eventName), $"{e.name} has no event id");
            }
        }

        [Test]
        public void EveryOption_HasALabelAndAnInWorldHint_WithoutNumbers()
        {
            foreach (var e in Events())
            {
                foreach (var (o, where, _) in AllOptions(e))
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(o.label), $"{where} has no label");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(o.hint), $"{where} has no hint");
                    Assert.IsFalse(Digit.IsMatch(o.hint), $"{where}: no numbers before choosing (hint)");
                    Assert.IsFalse(Digit.IsMatch(o.label), $"{where}: no numbers before choosing (label)");
                }
            }
        }

        [Test]
        public void EveryOutcome_EndsWithANarratedResult_OrLeadsToAStep()
        {
            foreach (var e in Events())
            {
                foreach (var (o, where, _) in AllOptions(e))
                {
                    foreach (var b in o.Branches())
                    {
                        Assert.IsTrue(b.actions.All(a => a != null), $"{where} has an empty action slot");
                        if (b.HasNextStep) continue;
                        Assert.IsFalse(string.IsNullOrWhiteSpace(b.resultText), $"{where}: an outcome ends without a result text");
                        Assert.IsNotEmpty(b.actions, $"{where}: an outcome ends without costing or giving anything");
                    }
                }
            }
        }

        [Test]
        public void Placeholders_MatchTheActionsOfTheirBranch()
        {
            foreach (var e in Events())
            {
                foreach (var (o, where, _) in AllOptions(e))
                {
                    foreach (var b in o.Branches())
                    {
                        var texts = new List<string> { b.resultText };
                        if (o.outcome == EventOutcomeKind.Check) texts.Add(o.check.introLine);
                        texts.AddRange(b.classResultLines.Select(l => l.text));
                        var placeholders = new HashSet<EventResultKind>(texts.SelectMany(EventResultFormatter.PlaceholdersIn));
                        var kinds = new HashSet<EventResultKind>(b.actions.Where(a => a != null).Select(a => a.Kind));

                        foreach (var p in placeholders)
                        {
                            Assert.IsTrue(kinds.Contains(p), $"{where}: {EventResultFormatter.Placeholder(p)} has no action in its branch");
                        }
                        foreach (var k in kinds)
                        {
                            Assert.IsTrue(placeholders.Contains(k), $"{where}: an action changes {k} but the result never says how much");
                        }
                    }
                }
            }
        }

        [Test]
        public void EveryResultText_FormatsCleanly_WithAndWithoutAnItem()
        {
            var item = ScriptableObject.CreateInstance<Game.Effects.Effect>();
            item.effectName = "Relic";
            item.Quality = Game.Effects.EffectQuality.Rare;
            foreach (var e in Events())
            {
                foreach (var (o, where, _) in AllOptions(e))
                {
                    foreach (var b in o.Branches())
                    {
                        foreach (bool itemDrops in new[] { true, false })
                        {
                            var results = b.actions.Where(a => a != null).Select(a => a.Kind == EventResultKind.Item
                                ? EventActionResult.Item(itemDrops ? item : null)
                                : EventActionResult.Of(a.Kind, IsCost(a) ? -5f : 5f)).ToList();
                            string text = EventResultFormatter.Format(b.resultText, results);
                            foreach (string leftover in new[] { "{", "}", "[", "]", " ,", " .", ",." })
                            {
                                StringAssert.DoesNotContain(leftover, text, $"{where} (item: {itemDrops}): '{text}'");
                            }
                            // A missing item fills {item} with "nothing": its mention belongs in an optional [sentence]
                            if (!itemDrops && b.actions.Any(a => a != null && a.Kind == EventResultKind.Item))
                            {
                                int Nothings(string s) => System.Text.RegularExpressions.Regex.Matches(s, "nothing").Count;
                                Assert.LessOrEqual(Nothings(text), Nothings(b.resultText),
                                    $"{where}: an item mention should be an optional [sentence]: '{text}'");
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void FirstScreenOptions_AlwaysRiskACost_ExceptTheShrinePrayer()
        {
            foreach (var e in Events())
            {
                foreach (var (o, where, firstScreen) in AllOptions(e))
                {
                    if (!firstScreen) continue;
                    if (e.eventName == ShrineId && o.label == "Kneel and pray.") continue;
                    Assert.IsTrue(RisksACost(e, o), $"{where}: a first-step option must risk a cost (pure gains are earned in a step)");
                }
            }
        }

        [Test]
        public void FirstScreen_HasTwoOrThreeOptionsForEveryone_AndAtMostOneClassOption()
        {
            foreach (var e in Events())
            {
                int forEveryone = e.options.Count(o => o.onlyForClass == CharacterClass.None);
                Assert.That(forEveryone, Is.InRange(2, 3), $"{e.name}: options for everyone");
                foreach (var cls in new[] { CharacterClass.Warrior, CharacterClass.Archer, CharacterClass.Mage })
                {
                    Assert.LessOrEqual(e.options.Count(o => o.onlyForClass == cls), 1, $"{e.name}: class options for {cls}");
                }
            }
        }

        [Test]
        public void ClassOnlyOptions_AreWhereTheSpecPutsThem()
        {
            var events = Events();
            CharacterClass[] ClassesIn(string id) => events.Single(e => e.eventName == id).options
                .Select(o => o.onlyForClass).Where(c => c != CharacterClass.None).ToArray();

            CollectionAssert.AreEquivalent(new[] { CharacterClass.Mage }, ClassesIn("arcaneBox"));
            CollectionAssert.AreEquivalent(new[] { CharacterClass.Mage }, ClassesIn("bandits"));
            CollectionAssert.AreEquivalent(new[] { CharacterClass.Warrior }, ClassesIn("faraclarianClan"));
            CollectionAssert.AreEquivalent(new[] { CharacterClass.Archer, CharacterClass.Mage }, ClassesIn("houseOfStars"));
            CollectionAssert.AreEquivalent(new[] { CharacterClass.Archer }, ClassesIn("lastTorch"));

            foreach (var cls in new[] { CharacterClass.Warrior, CharacterClass.Archer, CharacterClass.Mage })
            {
                Assert.IsTrue(events.Any(e => AllOptions(e).Any(x => x.option.onlyForClass == cls)), $"{cls} has no class-only option");
            }
        }

        [Test]
        public void ClassTexts_AreForARealClass()
        {
            var classes = new[] { CharacterClass.Warrior, CharacterClass.Archer, CharacterClass.Mage };
            foreach (var e in Events())
            {
                var texts = e.classIntroLines.Select(t => (t, "intro")).ToList();
                foreach (var (o, where, _) in AllOptions(e))
                {
                    texts.AddRange(o.classLabels.Select(t => (t, where + " label")));
                    foreach (var b in o.Branches()) texts.AddRange(b.classResultLines.Select(t => (t, where + " result")));
                    Assert.IsTrue(o.onlyForClass == CharacterClass.None || classes.Contains(o.onlyForClass), $"{where}: class filter");
                }
                foreach (var (t, where) in texts)
                {
                    CollectionAssert.Contains(classes, t.characterClass, $"{e.name} {where}: class text without a playable class");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(t.text), $"{e.name} {where}: empty class text");
                }
                Assert.AreEqual(texts.Count(x => x.Item2 == "intro"), e.classIntroLines.Select(t => t.characterClass).Distinct().Count(),
                    $"{e.name}: two intro lines for the same class");
            }
        }

        [Test]
        public void EveryEventButTheAylinean_HasANonLuckCheck()
        {
            foreach (var e in Events())
            {
                var checks = AllOptions(e).Where(x => x.option.outcome == EventOutcomeKind.Check).Select(x => x.option.check).ToList();
                bool IsLuck(EventCheck c) => c.source == EventCheckSource.Stat && c.stat == Game.Progression.StatType.Luck;
                if (e.eventName == AylineanId)
                {
                    Assert.IsTrue(checks.All(IsLuck), "Laughter in the Leaves is Luck-only on purpose");
                    continue;
                }
                Assert.IsTrue(checks.Any(c => !IsLuck(c)), $"{e.name} needs at least one non-Luck check");
            }
        }

        [Test]
        public void OnlyLiah_IsOncePerPilgrimage()
        {
            CollectionAssert.AreEquivalent(new[] { "liah" }, Events().Where(e => e.oncePerPilgrimage).Select(e => e.eventName));
        }

        [Test]
        public void TheShrine_IsTheWaysideShrineOfMalien()
        {
            Assert.AreEqual("Wayside Shrine of Malien", Events().Single(e => e.eventName == ShrineId).title);
        }

        [Test]
        public void PlayerFacingTexts_UseTheGlossarySpellings()
        {
            var banned = new[] { "Centinel", "Jester Forest", "Uf'Faraclario", "Fara Uf'clario", "Dawner", "Elven Shop" };
            foreach (var e in Events())
            {
                var all = new List<string> { e.title, e.fullText };
                all.AddRange(e.classIntroLines.Select(t => t.text));
                all.AddRange(e.steps.Select(s => s.prompt));
                foreach (var (o, _, _) in AllOptions(e))
                {
                    all.Add(o.label);
                    all.Add(o.hint);
                    all.AddRange(o.classLabels.Select(t => t.text));
                    if (o.outcome == EventOutcomeKind.Check) all.Add(o.check.introLine);
                    foreach (var b in o.Branches())
                    {
                        all.Add(b.resultText);
                        all.AddRange(b.classResultLines.Select(t => t.text));
                    }
                }
                foreach (var text in all.Where(t => !string.IsNullOrEmpty(t)))
                {
                    foreach (var word in banned) StringAssert.DoesNotContain(word, text, $"{e.name}: '{word}' isn't in the glossary");
                    StringAssert.DoesNotContain("—", text, $"{e.name}: the font has no em dash");
                }
            }
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
