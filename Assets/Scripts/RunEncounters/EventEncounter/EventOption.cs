using Game.Control;
using Game.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>Whether an option has one result, or a stat check that picks a Success or a Failure branch.</summary>
    public enum EventOutcomeKind
    {
        Fixed,
        Check
    }

    public enum EventCheckDifficulty
    {
        Easy,
        Even,
        Hard
    }

    /// <summary>What a check reads: any stat, the chosen class's Key stat, or the player's current Grace.</summary>
    public enum EventCheckSource
    {
        Stat,
        KeyStat,
        CurrentGrace
    }

    /// <summary>A text shown only for one class (an intro line, a label, a result line).</summary>
    [Serializable]
    public class ClassText
    {
        public CharacterClass characterClass;
        [TextArea(2, 6)] public string text;

        /// <summary>The text for this class, or null when the list has none for it.</summary>
        public static string Find(List<ClassText> texts, CharacterClass characterClass)
        {
            if (texts == null) return null;
            foreach (var t in texts)
            {
                if (t != null && t.characterClass == characterClass && !string.IsNullOrWhiteSpace(t.text)) return t.text;
            }
            return null;
        }
    }

    /// <summary>
    /// What happens when an outcome lands: its actions run, its result text is narrated (with {health}, {grace},
    /// {chronos}, {favors} and {item} filled from the actions), and the chain either ends here or goes on to a step.
    /// </summary>
    [Serializable]
    public class EventBranch
    {
        [Tooltip("What this outcome costs and gives. Shared action assets in OptionActions/.")]
        public List<EventOptionActionBase> actions = new();

        [Tooltip("Narrated result. Placeholders: {health}, {grace}, {chronos}, {favors}, {item}. A sentence in " +
            "[square brackets] shows only if its action gave something.")]
        [TextArea(3, 10)] public string resultText;

        [Tooltip("A line added to the result for one class only.")]
        public List<ClassText> classResultLines = new();

        [Tooltip("Id of the step this outcome leads to. Empty = the chain ends and the result shows.")]
        public string nextStepId;

        public bool HasNextStep => !string.IsNullOrWhiteSpace(nextStepId);
    }

    [Serializable]
    public class EventCheck
    {
        public EventCheckSource source = EventCheckSource.Stat;
        [Tooltip("The stat read when Source is Stat.")]
        public StatType stat = StatType.Luck;
        public EventCheckDifficulty difficulty = EventCheckDifficulty.Even;

        [Tooltip("Optional line narrated before either branch's result (e.g. the clan turning on a liar).")]
        [TextArea(2, 6)] public string introLine;
    }

    /// <summary>
    /// One choice on an event screen. The player sees its label and, in the tooltip, its in-world hint; numbers
    /// appear only in the result.
    /// </summary>
    [Serializable]
    public class EventOption
    {
        public string label;
        [Tooltip("Replaces the label for one class.")]
        public List<ClassText> classLabels = new();

        [Tooltip("None = offered to every class. Otherwise offered only to that class.")]
        public CharacterClass onlyForClass = CharacterClass.None;

        [Tooltip("In-world hint shown in the tooltip instead of the outcome. No numbers.")]
        [TextArea(2, 6)] public string hint;

        public EventOutcomeKind outcome = EventOutcomeKind.Fixed;

        [Tooltip("The result when Outcome is Fixed.")]
        public EventBranch result = new();

        [Tooltip("The check when Outcome is Check.")]
        public EventCheck check = new();
        public EventBranch success = new();
        public EventBranch failure = new();

        public bool IsOfferedTo(CharacterClass characterClass) =>
            onlyForClass == CharacterClass.None || onlyForClass == characterClass;

        public string GetLabel(CharacterClass characterClass) => ClassText.Find(classLabels, characterClass) ?? label;

        /// <summary>The branches this option can land on: the result for Fixed, Success and Failure for a Check.</summary>
        public IEnumerable<EventBranch> Branches()
        {
            if (outcome == EventOutcomeKind.Fixed)
            {
                yield return result;
            }
            else
            {
                yield return success;
                yield return failure;
            }
        }
    }

    /// <summary>A follow-up screen: a prompt and its own options, reached through a branch's next step id.</summary>
    [Serializable]
    public class EventStep
    {
        [Tooltip("Referenced by a branch's Next Step Id. Unique within the event.")]
        public string id;
        [TextArea(3, 10)] public string prompt;
        public List<EventOption> options = new();
    }
}
