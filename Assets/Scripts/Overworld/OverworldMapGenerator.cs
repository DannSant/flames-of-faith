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
                acts = new List<RunMapGraph>()
            };

            for (int actIndex = 0; actIndex < mapDefinitions.Count; actIndex++)
            {
                MapDefinition mapDef = mapDefinitions[actIndex];

                RunMapGraph actGraph = GenerateActGraph(
                    mapDef,
                    seed + actIndex * 1000 // seed isolation per act
                );

                //Validate nodes, position must be unique
                HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();
                foreach (var node in actGraph.nodes.Values)
                {
                    if (occupiedPositions.Contains(node.worldPosition))
                    {
                        Debug.LogError(
                            $"[MapGenerator] Duplicate node position " +
                            $"at {node.worldPosition} in act {actIndex}"
                        );
                    }
                    else
                    {
                        occupiedPositions.Add(node.worldPosition);
                    }
                }

                runState.acts.Add(actGraph);
            }

            return runState;
        }
        private static RunMapGraph GenerateActGraph(MapDefinition mapDefinition, int actSeed)
        {
            System.Random rng = new System.Random(actSeed);
            RunMapGraph graph = new RunMapGraph
            {
                mapId = mapDefinition.mapId,
                seed = actSeed,
                actNumber = mapDefinition.actNumber,
                mapMusic = mapDefinition.mapMusic,
                nodes = new Dictionary<string, RunNode>()
            };

            // 1️ Create all nodes
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

            // 2️ Create connections 
            //Build a temporary list of connection candidates
            var connectionCandidates = new List<(NodeDefinition from, ConnectionDefinition conn)>();

            foreach (var nodeDef in mapDefinition.nodes)
            {
                foreach (var connection in nodeDef.connections)
                {
                    connectionCandidates.Add((nodeDef, connection));
                }
            }

            // Decide enabled state (with RNG)
            Dictionary<ConnectionDefinition, bool> connectionEnabledMap = new();
            Dictionary<string, List<ConnectionDefinition>> rngGroups = new();

            foreach (var (from, conn) in connectionCandidates)
            {
                bool enabled;

                if (conn.required)
                {
                    enabled = true;
                }
                else if (conn.canBeDisabledByRng)
                {
                    enabled = rng.NextDouble() <= conn.enableChance;
                }
                else
                {
                    enabled = true;
                }

                connectionEnabledMap[conn] = enabled;

                if (!string.IsNullOrEmpty(conn.rngGroupId))
                {
                    if (!rngGroups.ContainsKey(conn.rngGroupId))
                        rngGroups[conn.rngGroupId] = new List<ConnectionDefinition>();

                    rngGroups[conn.rngGroupId].Add(conn);
                }
            }

            //Enforce RNG group safety
            foreach (var group in rngGroups)
            {
                bool anyEnabled = false;

                foreach (var conn in group.Value)
                {
                    if (connectionEnabledMap[conn])
                    {
                        anyEnabled = true;
                        break;
                    }
                }

                if (!anyEnabled)
                {
                    // Force-enable one at random
                    int index = rng.Next(group.Value.Count);
                    connectionEnabledMap[group.Value[index]] = true;
                }
            }

            //Create RunEdges using final enabled state
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

                    bool enabled = connectionEnabledMap[connection];

                    RunEdge edge = new RunEdge
                    {
                        fromNodeId = fromNode.id,
                        toNodeId = toNode.id,
                        direction = connection.direction,
                        enabled = enabled,
                        revealOnClear = connection.revealOnClear
                    };

                    fromNode.outgoingEdges.Add(edge);
                }
            }

            // 3️ Resolve start node
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