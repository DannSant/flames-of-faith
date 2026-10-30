using System.Collections.Generic;

namespace Game.RunEncounters
{
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
