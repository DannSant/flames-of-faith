using Game.Control;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.RunEncounters
{
    /// <summary>
    /// One event: its intro, its first options and the follow-up steps those options can lead to, all in one
    /// asset. Steps refer to each other by id (EventBranch.nextStepId), so the chain needs no nested assets.
    /// </summary>
    [CreateAssetMenu(menuName = "RunEncounters/Event Encounter", fileName = "EventEncounter")]
    public class EventEncounterData : ScriptableObject
    {
        public string eventName;
        public string title;

        [TextArea(5, 10)]
        public string fullText;

        [Tooltip("A paragraph added to the intro for one class only.")]
        public List<ClassText> classIntroLines = new();

        [Tooltip("The first screen's options.")]
        public List<EventOption> options = new();

        [Tooltip("Follow-up screens, reached through a branch's Next Step Id. At most 3 deep.")]
        public List<EventStep> steps = new();

        [Tooltip("Never offered again in the same Pilgrimage, even once every other event has been seen.")]
        public bool oncePerPilgrimage;

        [Tooltip("Optional art, shown in a panel beside the event window. Empty = no panel.")]
        public Sprite art;

        [Tooltip("Old option assets, used only while Options is empty. Being replaced by Options.")]
        [FormerlySerializedAs("options")]
        public List<EventOptionsList> legacyOptions = new();

        /// <summary>True once the event is authored with the new options; false while it still runs on the old assets.</summary>
        public bool UsesOptions => options != null && options.Count > 0;

        /// <summary>The intro, plus the class's own paragraph when it has one.</summary>
        public string GetIntro(CharacterClass characterClass)
        {
            string line = ClassText.Find(classIntroLines, characterClass);
            return string.IsNullOrEmpty(line) ? fullText : $"{fullText}\n\n{line}";
        }

        public EventStep FindStep(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || steps == null) return null;
            foreach (var step in steps)
            {
                if (step != null && step.id == id) return step;
            }
            return null;
        }
    }
}
