using Game.Control;
using Game.RunEncounters;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Walking an event: screens, class content, checks, step chains, and the old option assets. Checks and
    /// actions are stubbed, so no player is needed.
    /// </summary>
    public class EventRunnerTests
    {
        private static List<EventActionResult> NoActions(List<EventOptionActionBase> actions) => new();

        private static EventRunner Run(EventEncounterData data, CharacterClass cls = CharacterClass.Crusader, bool checkSucceeds = true) =>
            new EventRunner(data, cls, _ => checkSucceeds, NoActions);

        private static EventEncounterData Event()
        {
            var data = ScriptableObject.CreateInstance<EventEncounterData>();
            data.title = "The Weeping Chest";
            data.fullText = "A chest.";
            data.classIntroLines = new List<ClassText>
            {
                new ClassText { characterClass = CharacterClass.Augur, text = "Werlem knows this." }
            };
            data.options = new List<EventOption>
            {
                new EventOption
                {
                    label = "Take the gold.",
                    hint = "Gold is gold.",
                    result = new EventBranch
                    {
                        resultText = "You scoop it out.",
                        classResultLines = new List<ClassText> { new ClassText { characterClass = CharacterClass.Crusader, text = "Ayteel frowns." } }
                    }
                },
                new EventOption
                {
                    label = "Reach beneath.",
                    outcome = EventOutcomeKind.Check,
                    check = new EventCheck { source = EventCheckSource.CurrentGrace, introLine = "You reach down." },
                    success = new EventBranch { resultText = "Your fingers close on it." },
                    failure = new EventBranch { nextStepId = "whisper" }
                },
                new EventOption
                {
                    label = "Read the runes.",
                    onlyForClass = CharacterClass.Augur,
                    result = new EventBranch { resultText = "The runes answer." }
                }
            };
            data.steps = new List<EventStep>
            {
                new EventStep
                {
                    id = "whisper",
                    prompt = "Say it.",
                    options = new List<EventOption>
                    {
                        new EventOption { label = "Say the words.", result = new EventBranch { resultText = "You say it." } },
                        new EventOption { label = "Pray.", result = new EventBranch { resultText = "The voice shrieks." } }
                    }
                }
            };
            return data;
        }

        [Test]
        public void FirstScreen_ShowsTheIntroAndTheOptionsForThisClass()
        {
            var crusader = Run(Event(), CharacterClass.Crusader).BuildScreen();
            Assert.AreEqual("The Weeping Chest", crusader.title);
            Assert.AreEqual("A chest.", crusader.text);
            CollectionAssert.AreEqual(new[] { "Take the gold.", "Reach beneath." }, crusader.choices.Select(c => c.label));
            Assert.AreEqual("Gold is gold.", crusader.choices[0].hint);

            var augur = Run(Event(), CharacterClass.Augur).BuildScreen();
            Assert.AreEqual("A chest.\n\nWerlem knows this.", augur.text);
            CollectionAssert.AreEqual(new[] { "Take the gold.", "Reach beneath.", "Read the runes." }, augur.choices.Select(c => c.label));
        }

        [Test]
        public void ClassLabel_ReplacesTheLabelForItsClass()
        {
            var data = Event();
            data.options[0].classLabels = new List<ClassText> { new ClassText { characterClass = CharacterClass.Crusader, text = "Tell her who you are." } };
            Assert.AreEqual("Tell her who you are.", Run(data, CharacterClass.Crusader).BuildScreen().choices[0].label);
            Assert.AreEqual("Take the gold.", Run(data, CharacterClass.Sentinel).BuildScreen().choices[0].label);
        }

        [Test]
        public void FixedOption_EndsWithItsResultAndClassLine()
        {
            var runner = Run(Event(), CharacterClass.Crusader);
            Assert.IsTrue(runner.Choose(0));
            Assert.IsTrue(runner.IsFinished);
            Assert.AreEqual(1, runner.Resolution.parts.Count);
            Assert.AreEqual("You scoop it out.\n\nAyteel frowns.", runner.Resolution.parts[0].text);

            var sentinel = Run(Event(), CharacterClass.Sentinel);
            sentinel.Choose(0);
            Assert.AreEqual("You scoop it out.", sentinel.Resolution.parts[0].text);
        }

        [Test]
        public void ClassOption_IsPickedByItsShownIndex()
        {
            var runner = Run(Event(), CharacterClass.Augur);
            runner.Choose(2);
            Assert.AreEqual("The runes answer.", runner.Resolution.parts[0].text);
        }

        [Test]
        public void Check_Success_LandsOnSuccess_WithItsIntroLine()
        {
            var runner = Run(Event(), checkSucceeds: true);
            runner.Choose(1);
            Assert.IsTrue(runner.IsFinished);
            Assert.AreEqual("You reach down.\n\nYour fingers close on it.", runner.Resolution.parts[0].text);
        }

        [Test]
        public void Check_Failure_GoesToTheStep_ThenTheChainEnds()
        {
            var runner = Run(Event(), checkSucceeds: false);
            runner.Choose(1);
            Assert.IsFalse(runner.IsFinished);

            var step = runner.BuildScreen();
            Assert.AreEqual("Say it.", step.text);
            Assert.AreEqual("The Weeping Chest", step.title);
            CollectionAssert.AreEqual(new[] { "Say the words.", "Pray." }, step.choices.Select(c => c.label));

            runner.Choose(1);
            Assert.IsTrue(runner.IsFinished);
            CollectionAssert.AreEqual(new[] { "You reach down.", "The voice shrieks." }, runner.Resolution.parts.Select(p => p.text));
        }

        [Test]
        public void ActionsOfEveryLandedBranch_AreApplied()
        {
            var data = Event();
            var applied = new List<int>();
            var runner = new EventRunner(data, CharacterClass.Crusader, _ => false, actions =>
            {
                applied.Add(actions.Count);
                return new List<EventActionResult> { EventActionResult.Of(EventResultKind.Grace, 1f) };
            });
            runner.Choose(1);
            runner.Choose(0);
            Assert.AreEqual(2, applied.Count, "The failure branch and the step's option both run their actions");
            Assert.AreEqual(2, runner.Resolution.AllResults.Count());
        }

        [Test]
        public void InvalidChoice_OrAfterTheEnd_DoesNothing()
        {
            var runner = Run(Event(), CharacterClass.Crusader);
            Assert.IsFalse(runner.Choose(2), "The Augur's option isn't shown to the Crusader");
            Assert.IsFalse(runner.Choose(-1));
            runner.Choose(0);
            Assert.IsFalse(runner.Choose(0), "The event is over");
        }

        [Test]
        public void MissingStep_EndsTheEvent()
        {
            var data = Event();
            data.options[0].result.nextStepId = "nowhere";
            var runner = Run(data);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("doesn't exist"));
            runner.Choose(0);
            Assert.IsTrue(runner.IsFinished);
        }
    }
}
