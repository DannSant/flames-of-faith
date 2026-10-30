using Game.Control;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>One choice on the event window: its label and its in-world hint (the tooltip).</summary>
    public class EventChoice
    {
        public string label;
        public string hint;
    }

    /// <summary>What the event window shows: plain data plus the callback that picks a choice (by index).</summary>
    public class EventScreen
    {
        public string title;
        public string text;
        public Sprite art;
        public List<EventChoice> choices = new();
        public Action<int> choose;
    }

    /// <summary>
    /// Walks one event: the first screen, then each step a branch leads to, until a branch ends the chain. Each
    /// landed branch adds its narration (check intro line, result text, class line) and what its actions did to
    /// the resolution. Checks and actions come in as delegates, so tests can drive it without a player.
    /// </summary>
    public class EventRunner
    {
        private readonly EventEncounterData data;
        private readonly CharacterClass characterClass;
        private readonly Func<EventCheck, bool> rollCheck;
        private readonly Func<List<EventOptionActionBase>, List<EventActionResult>> applyActions;

        private readonly List<EventOption> shownOptions = new();
        private EventStep currentStep;

        public EventResolution Resolution { get; } = new();
        public bool IsFinished { get; private set; }
        public EventStep CurrentStep => currentStep;

        public EventRunner(EventEncounterData data, CharacterClass characterClass, Func<EventCheck, bool> rollCheck,
            Func<List<EventOptionActionBase>, List<EventActionResult>> applyActions)
        {
            this.data = data;
            this.characterClass = characterClass;
            this.rollCheck = rollCheck;
            this.applyActions = applyActions;
            ShowOptions(data.options);
        }

        /// <summary>The options offered on the current screen, in order: class-only ones only for their class.</summary>
        public IReadOnlyList<EventOption> ShownOptions => shownOptions;

        public EventScreen BuildScreen()
        {
            var screen = new EventScreen
            {
                title = data.title,
                text = currentStep != null ? currentStep.prompt : data.GetIntro(characterClass),
                art = data.art
            };

            foreach (var option in shownOptions)
            {
                screen.choices.Add(new EventChoice { label = option.GetLabel(characterClass), hint = option.hint });
            }
            return screen;
        }

        /// <summary>Picks a choice of the current screen; returns false when the index isn't on it.</summary>
        public bool Choose(int index)
        {
            if (IsFinished) return false;

            if (index < 0 || index >= shownOptions.Count) return false;
            var option = shownOptions[index];

            EventBranch branch = option.result;
            string intro = null;
            if (option.outcome == EventOutcomeKind.Check)
            {
                branch = rollCheck(option.check) ? option.success : option.failure;
                intro = option.check.introLine;
            }

            Land(branch, intro);
            return true;
        }

        private void Land(EventBranch branch, string intro)
        {
            branch ??= new EventBranch();
            Resolution.parts.Add(new EventNarration
            {
                text = JoinParagraphs(intro, branch.resultText, ClassText.Find(branch.classResultLines, characterClass)),
                results = applyActions(branch.actions)
            });

            var next = branch.HasNextStep ? data.FindStep(branch.nextStepId) : null;
            if (branch.HasNextStep && next == null)
            {
                Debug.LogError($"[EventRunner] {data.name}: next step '{branch.nextStepId}' doesn't exist; ending the event.");
            }

            if (next == null)
            {
                IsFinished = true;
                return;
            }

            currentStep = next;
            ShowOptions(next.options);
            if (shownOptions.Count == 0)
            {
                Debug.LogError($"[EventRunner] {data.name}: step '{next.id}' offers nothing to this class; ending the event.");
                IsFinished = true;
            }
        }

        private void ShowOptions(List<EventOption> options)
        {
            shownOptions.Clear();
            if (options == null) return;

            foreach (var option in options)
            {
                if (option != null && option.IsOfferedTo(characterClass)) shownOptions.Add(option);
            }
        }

        public static string JoinParagraphs(params string[] paragraphs)
        {
            var kept = new List<string>();
            foreach (var p in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(p)) kept.Add(p.Trim());
            }
            return string.Join("\n\n", kept);
        }
    }
}
