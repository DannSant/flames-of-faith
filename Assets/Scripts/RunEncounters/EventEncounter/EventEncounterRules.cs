using System.Collections.Generic;

namespace Game.RunEncounters
{
    /// <summary>
    /// Structure rules for an event's step chain: unique step ids, every next step exists, no loops, at most
    /// MaxStepDepth follow-up steps, and no step that nothing leads to. Pure, so tests can run it on any event.
    /// </summary>
    public static class EventEncounterRules
    {
        /// <summary>Follow-up steps after the first screen, at most.</summary>
        public const int MaxStepDepth = 3;

        /// <summary>Every structure problem in the event, one message each; empty when it's sound.</summary>
        public static List<string> Validate(EventEncounterData data)
        {
            var problems = new List<string>();
            if (data == null)
            {
                problems.Add("no event");
                return problems;
            }

            var ids = new HashSet<string>();
            foreach (var step in data.steps ?? new List<EventStep>())
            {
                if (step == null)
                {
                    problems.Add("empty step slot");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(step.id)) problems.Add("a step has no id");
                else if (!ids.Add(step.id)) problems.Add($"step id '{step.id}' is used twice");
            }

            var reached = new HashSet<string>();
            Walk(data, data.options, 0, new List<string>(), reached, problems);

            foreach (var id in ids)
            {
                if (!reached.Contains(id)) problems.Add($"step '{id}' is never reached");
            }

            return problems;
        }

        /// <summary>How many follow-up steps the longest chain goes through (0 = no steps).</summary>
        public static int GetMaxDepth(EventEncounterData data)
        {
            if (data == null) return 0;
            return Depth(data, data.options, new HashSet<string>());
        }

        private static int Depth(EventEncounterData data, List<EventOption> options, HashSet<string> path)
        {
            int max = 0;
            foreach (var branch in AllBranches(options))
            {
                if (!branch.HasNextStep || path.Contains(branch.nextStepId)) continue;
                var step = data.FindStep(branch.nextStepId);
                if (step == null) continue;

                path.Add(step.id);
                max = System.Math.Max(max, 1 + Depth(data, step.options, path));
                path.Remove(step.id);
            }
            return max;
        }

        private static void Walk(EventEncounterData data, List<EventOption> options, int depth, List<string> path,
            HashSet<string> reached, List<string> problems)
        {
            foreach (var branch in AllBranches(options))
            {
                if (!branch.HasNextStep) continue;

                string id = branch.nextStepId;
                var step = data.FindStep(id);
                if (step == null)
                {
                    problems.Add($"next step '{id}' doesn't exist");
                    continue;
                }
                if (path.Contains(id))
                {
                    problems.Add($"step '{id}' loops back on itself ({string.Join(" > ", path)} > {id})");
                    continue;
                }
                if (depth + 1 > MaxStepDepth)
                {
                    problems.Add($"step '{id}' is {depth + 1} steps deep (max {MaxStepDepth})");
                    continue;
                }

                reached.Add(id);
                path.Add(id);
                Walk(data, step.options, depth + 1, path, reached, problems);
                path.RemoveAt(path.Count - 1);
            }
        }

        /// <summary>Every branch the options can land on, skipping empty option slots.</summary>
        public static IEnumerable<EventBranch> AllBranches(List<EventOption> options)
        {
            if (options == null) yield break;
            foreach (var option in options)
            {
                if (option == null) continue;
                foreach (var branch in option.Branches())
                {
                    if (branch != null) yield return branch;
                }
            }
        }
    }
}
