using Game.Overworld;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Game.Tests
{
    /// <summary>
    /// Moving on an act's map, and the developer cheats that bend it (reveal all, free travel).
    /// Map: start -> a -> boss, start -> shop, and a Blocked side node (an RNG-disabled path).
    /// </summary>
    public class MapTravelRulesTests
    {
        private RunMapGraph graph;

        private static RunNode Node(string id, RunNodeState state, params string[] to) => new RunNode
        {
            id = id,
            state = state,
            outgoingEdges = to.Select(t => new RunEdge { fromNodeId = id, toNodeId = t, enabled = true, revealOnClear = true }).ToList(),
        };

        [SetUp]
        public void SetUp()
        {
            var nodes = new[]
            {
                Node("start", RunNodeState.Cleared, "a", "shop", "side"),
                Node("a", RunNodeState.Revealed, "boss"),
                Node("shop", RunNodeState.LockedHidden),
                Node("boss", RunNodeState.LockedHidden),
                Node("side", RunNodeState.Blocked),
            };
            graph = new RunMapGraph { nodes = nodes.ToDictionary(n => n.id), currentNodeId = "start" };
        }

        private RunNode N(string id) => graph.nodes[id];

        [Test]
        public void Travel_FollowsEnabledEdgesBothWays()
        {
            Assert.IsTrue(MapTravelRules.CanTravel(graph, N("start"), N("a")));
            Assert.IsTrue(MapTravelRules.CanTravel(graph, N("a"), N("start")), "Going back along an edge is allowed");
        }

        [Test]
        public void Travel_NeedsARevealedNode()
        {
            N("boss").state = RunNodeState.Revealed;
            Assert.IsFalse(MapTravelRules.CanTravel(graph, N("start"), N("boss")), "No edge from start to the boss");
            Assert.IsFalse(MapTravelRules.CanTravel(graph, N("start"), N("shop")), "Hidden nodes can't be reached");
        }

        [Test]
        public void Travel_IgnoresDisabledEdges()
        {
            N("start").outgoingEdges.First(e => e.toNodeId == "a").enabled = false;
            Assert.IsFalse(MapTravelRules.CanTravel(graph, N("start"), N("a")));
        }

        [Test]
        public void FreeTravel_ReachesAnyRevealedNode()
        {
            N("boss").state = RunNodeState.Revealed;
            Assert.IsTrue(MapTravelRules.CanTravel(graph, N("start"), N("boss"), freeTravel: true));
            Assert.IsTrue(MapTravelRules.CanTravel(graph, N("a"), N("start"), freeTravel: true));
        }

        [Test]
        public void FreeTravel_StillSkipsHiddenBlockedAndTheCurrentNode()
        {
            Assert.IsFalse(MapTravelRules.CanTravel(graph, N("start"), N("shop"), freeTravel: true), "Hidden");
            Assert.IsFalse(MapTravelRules.CanTravel(graph, N("start"), N("side"), freeTravel: true), "Blocked");
            Assert.IsFalse(MapTravelRules.CanTravel(graph, N("start"), N("start"), freeTravel: true), "Already there");
        }

        [Test]
        public void RevealAll_RevealsTheFogButNotBlockedPaths()
        {
            List<RunNode> revealed = MapTravelRules.RevealAll(graph);

            CollectionAssert.AreEquivalent(new[] { "shop", "boss" }, revealed.Select(n => n.id));
            Assert.AreEqual(RunNodeState.Revealed, N("shop").state);
            Assert.AreEqual(RunNodeState.Revealed, N("boss").state);
            Assert.AreEqual(RunNodeState.Blocked, N("side").state, "RNG-disabled paths don't exist in this run");
            Assert.AreEqual(RunNodeState.Cleared, N("start").state, "Cleared nodes stay cleared");
            Assert.IsEmpty(MapTravelRules.RevealAll(graph), "Nothing left to reveal");
        }

        [Test]
        public void RevealAll_ThenFreeTravel_GoesStraightToTheBoss()
        {
            MapTravelRules.RevealAll(graph);
            Assert.IsTrue(MapTravelRules.CanTravel(graph, N("start"), N("boss"), freeTravel: true));
            Assert.IsTrue(MapTravelRules.CanTravel(graph, N("start"), N("shop"), freeTravel: true));
        }
    }
}
