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

        private static EventRunner Run(EventEncounterData data, CharacterClass cls = CharacterClass.Warrior, bool checkSucceeds = true) =>
            new EventRunner(data, cls, _ => checkSucceeds, NoActions);

        private static EventEncounterData Event()
        {
            var data = ScriptableObject.CreateInstance<EventEncounterData>();
            data.title = "The Weeping Chest";
            data.fullText = "A chest.";
            data.classIntroLines = new List<ClassText>
            {
                new ClassText { characterClass = CharacterClass.Mage, text = "Werlem knows this." }
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
                        classResultLines = new List<ClassText> { new ClassText { characterClass = CharacterClass.Warrior, text = "Ayteel frowns." } }
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
                    onlyForClass = CharacterClass.Mage,
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
            var warrior = Run(Event(), CharacterClass.Warrior).BuildScreen();
            Assert.AreEqual("The Weeping Chest", warrior.title);
            Assert.AreEqual("A chest.", warrior.text);
            CollectionAssert.AreEqual(new[] { "Take the gold.", "Reach beneath." }, warrior.choices.Select(c => c.label));
            Assert.AreEqual("Gold is gold.", warrior.choices[0].hint);

            var augur = Run(Event(), CharacterClass.Mage).BuildScreen();
            Assert.AreEqual("A chest.\n\nWerlem knows this.", augur.text);
            CollectionAssert.AreEqual(new[] { "Take the gold.", "Reach beneath.", "Read the runes." }, augur.choices.Select(c => c.label));
        }

        [Test]
        public void ClassLabel_ReplacesTheLabelForItsClass()
        {
            var data = Event();
            data.options[0].classLabels = new List<ClassText> { new ClassText { characterClass = CharacterClass.Warrior, text = "Tell her who you are." } };
            Assert.AreEqual("Tell her who you are.", Run(data, CharacterClass.Warrior).BuildScreen().choices[0].label);
            Assert.AreEqual("Take the gold.", Run(data, CharacterClass.Archer).BuildScreen().choices[0].label);
        }

        [Test]
        public void FixedOption_EndsWithItsResultAndClassLine()
        {
            var runner = Run(Event(), CharacterClass.Warrior);
            Assert.IsTrue(runner.Choose(0));
            Assert.IsTrue(runner.IsFinished);
            Assert.AreEqual(1, runner.Resolution.parts.Count);
            Assert.AreEqual("You scoop it out.\n\nAyteel frowns.", runner.Resolution.parts[0].text);

            var archer = Run(Event(), CharacterClass.Archer);
            archer.Choose(0);
            Assert.AreEqual("You scoop it out.", archer.Resolution.parts[0].text);
        }

        [Test]
        public void ClassOption_IsPickedByItsShownIndex()
        {
            var runner = Run(Event(), CharacterClass.Mage);
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
            var runner = new EventRunner(data, CharacterClass.Warrior, _ => false, actions =>
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
            var runner = Run(Event(), CharacterClass.Warrior);
            Assert.IsFalse(runner.Choose(2), "The Augur's option isn't shown to the Warrior");
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

        [Test]
        public void LegacyEvent_OffersItsOldOptionAssets()
        {
            var data = ScriptableObject.CreateInstance<EventEncounterData>();
            data.fullText = "Old.";
            var old = ScriptableObject.CreateInstance<EventOptionsList>();
            data.legacyOptions = new List<EventOptionsList> { old, old };

            var runner = Run(data);
            Assert.AreEqual(2, runner.BuildScreen().choices.Count);
            Assert.IsTrue(runner.Choose(1));
            Assert.IsTrue(runner.IsFinished);
            Assert.IsTrue(string.IsNullOrEmpty(runner.Resolution.parts[0].text));
        }
    }
}
