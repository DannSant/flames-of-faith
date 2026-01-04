using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public static class OverworldMapGenerator
    {
        public static RunMapState GenerateRun(List<MapDefinition> mapDefinitions, int seed)
        {
            if (mapDefinitions == null || mapDefinitions.Count == 0)
            {
                Debug.LogError("[MapGenerator] No MapDefinitions provided.");
                return null;
            }

            RunMapState runState = new RunMapState
            {
                seed = seed,
                currentActIndex = 0,
                acts = new List<RunMapGraph>()
            };

            for (int actIndex = 0; actIndex < mapDefinitions.Count; actIndex++)
            {
                MapDefinition mapDef = mapDefinitions[actIndex];

                RunMapGraph actGraph = GenerateActGraph(
                    mapDef,
                    seed + actIndex * 1000 // seed isolation per act
                );

                runState.acts.Add(actGraph);
            }

            return runState;
        }
        private static RunMapGraph GenerateActGraph(MapDefinition mapDefinition, int actSeed)
        {
            // NOTE: we are NOT using RNG yet, but we keep the seed
            RunMapGraph graph = new RunMapGraph
            {
                mapId = mapDefinition.mapId,
                seed = actSeed,
                nodes = new Dictionary<string, RunNode>()
            };

            // 1️⃣ Create all nodes
            foreach (var nodeDef in mapDefinition.nodes)
            {
                if (graph.nodes.ContainsKey(nodeDef.id))
                {
                    Debug.LogError($"[MapGenerator] Duplicate node id: {nodeDef.id}");
                    continue;
                }

                RunNode runNode = new RunNode
                {
                    id = nodeDef.id,
                    nodeType = nodeDef.nodeType,
                    levelData = nodeDef.levelData,
                    worldPosition = nodeDef.worldPosition,
                    state = RunNodeState.LockedHidden,
                    outgoingEdges = new List<RunEdge>()
                };

                graph.nodes.Add(runNode.id, runNode);
            }

            // 2️⃣ Create connections (all enabled for now)
            foreach (var nodeDef in mapDefinition.nodes)
            {
                RunNode fromNode = graph.nodes[nodeDef.id];

                foreach (var connection in nodeDef.connections)
                {
                    if (!graph.nodes.TryGetValue(connection.toNodeId, out RunNode toNode))
                    {
                        Debug.LogError(
                            $"[MapGenerator] Connection from {nodeDef.id} " +
                            $"points to missing node {connection.toNodeId}"
                        );
                        continue;
                    }

                    RunEdge edge = new RunEdge
                    {
                        fromNodeId = fromNode.id,
                        toNodeId = toNode.id,
                        direction = connection.direction,
                        enabled = true, // RNG comes later
                        revealOnClear = connection.revealOnClear
                    };

                    fromNode.outgoingEdges.Add(edge);
                }
            }

            // 3️⃣ Resolve start node
            string startNodeId = mapDefinition.startNodeId;

            if (string.IsNullOrEmpty(startNodeId))
            {
                Debug.LogError("[MapGenerator] MapDefinition has no startNodeId.");
            }
            else if (!graph.nodes.TryGetValue(startNodeId, out RunNode startNode))
            {
                Debug.LogError($"[MapGenerator] Start node {startNodeId} not found.");
            }
            else
            {
                startNode.state = RunNodeState.Revealed;
                graph.currentNodeId = startNode.id;
            }

            return graph;
        }
    }

}