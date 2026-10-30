using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>An old option asset: a label and its actions. Being replaced by EventOption inside the event asset.</summary>
    [CreateAssetMenu(menuName = "RunEncounters/EventOptions/Options List", fileName = "OptionsList")]
    public class EventOptionsList : ScriptableObject
    {
        [SerializeField] private List<EventOptionActionBase> optionActionsList = new List<EventOptionActionBase>();
        [SerializeField] private string optionDescription = "";

        public List<EventOptionActionBase> OptionActionsList => optionActionsList;

        public string OptionDescription => optionDescription;

        public List<EventActionResult> Apply(EventContext context) => EventActions.ApplyAll(optionActionsList, context);
    }

    public static class EventActions
    {
        /// <summary>Runs every action in order, skipping empty slots, and collects what each one did.</summary>
        public static List<EventActionResult> ApplyAll(List<EventOptionActionBase> actions, EventContext context)
        {
            var results = new List<EventActionResult>();
            if (actions == null) return results;
            foreach (var action in actions)
            {
                if (action != null) results.Add(action.Apply(context));
            }
            return results;
        }
    }
}
