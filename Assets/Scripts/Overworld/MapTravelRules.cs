using System.Collections.Generic;

namespace Game.Overworld
{
    /// <summary>
    /// Pure rules for moving on an act's map and for the developer cheats that bend them.
    /// </summary>
    public static class MapTravelRules
    {
        /// <summary>
        /// A node can be moved to when it's Revealed or Cleared and an enabled edge joins it to
        /// <paramref name="from"/>, either way (going back is allowed). With <paramref name="freeTravel"/>
        /// (developer cheat) the edge isn't needed, but hidden and Blocked nodes still can't be reached.
        /// </summary>
        public static bool CanTravel(RunMapGraph graph, RunNode from, RunNode to, bool freeTravel = false)
        {
            if (graph == null || from == null || to == null) return false;

            if (to.state != RunNodeState.Revealed && to.state != RunNodeState.Cleared)
                return false;

            if (freeTravel)
                return to.id != from.id;

            return HasEnabledEdge(graph, from.id, to.id) || HasEnabledEdge(graph, to.id, from.id);
        }

        /// <summary>
        /// Developer cheat: reveals every node still under the fog of war. Blocked nodes stay blocked,
        /// since those paths don't exist in this run. Returns the nodes it revealed.
        /// </summary>
        public static List<RunNode> RevealAll(RunMapGraph graph)
        {
            var revealed = new List<RunNode>();
            if (graph?.nodes == null) return revealed;

            foreach (var node in graph.nodes.Values)
            {
                if (node.state == RunNodeState.LockedHidden)
                {
                    node.state = RunNodeState.Revealed;
                    revealed.Add(node);
                }
            }
            return revealed;
        }

        private static bool HasEnabledEdge(RunMapGraph graph, string fromId, string toId)
        {
            if (!graph.nodes.TryGetValue(fromId, out var fromNode) || fromNode.outgoingEdges == null)
                return false;

            foreach (var edge in fromNode.outgoingEdges)
            {
                if (edge.enabled && edge.toNodeId == toId)
                    return true;
            }
            return false;
        }
    }
}
