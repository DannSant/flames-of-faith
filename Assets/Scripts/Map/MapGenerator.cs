using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Map
{
    public class MapGenerator 
    {
        private System.Random rng = new();
        public List<List<MapNode>> GenerateMap(int layerCount, int minPerLayer = 2, int maxPerLayer = 4)
        {
            var map = new List<List<MapNode>>();

            // Start node
            var start = new MapNode(LevelType.Combat); // Could be a special "Start" type if you prefer
            map.Add(new List<MapNode> { start });

            // Middle layers
            for (int i = 1; i < layerCount - 1; i++)
            {
                int nodeCount = rng.Next(minPerLayer, maxPerLayer + 1);
                var layer = new List<MapNode>();

                // Ensure one combat level first
                layer.Add(new MapNode(LevelType.Combat));

                // Fill the rest randomly (excluding boss)
                for (int j = 1; j < nodeCount; j++)
                {
                    LevelType type = GetRandomNonBossType(i);
                    layer.Add(new MapNode(type));                    
                }

                // Shuffle to not always have combat at the top
                layer = layer.OrderBy(_ => rng.Next()).ToList();
                map.Add(layer);
            }

            // Boss layer (final)
            var boss = new MapNode(LevelType.Boss);
            map.Add(new List<MapNode> { boss });

            // Connect nodes
            for (int i = 0; i < map.Count - 1; i++)
            {
                var currentLayer = map[i];
                var nextLayer = map[i + 1];

                for (int j = 0; j < currentLayer.Count; j++)
                {
                    var node = currentLayer[j];

                    // Prefer to connect to nodes in similar horizontal positions
                    List<MapNode> preferredTargets = new();

                    // Try to connect to node at same index in next layer
                    if (j < nextLayer.Count)
                        preferredTargets.Add(nextLayer[j]);

                    // Try neighbors (left and right)
                    if (j - 1 >= 0 && j - 1 < nextLayer.Count)
                        preferredTargets.Add(nextLayer[j - 1]);

                    if (j + 1 < nextLayer.Count)
                        preferredTargets.Add(nextLayer[j + 1]);

                    // Always connect to at least one
                    var selected = preferredTargets.OrderBy(_ => rng.Next()).Take(rng.Next(1, Mathf.Min(3, preferredTargets.Count + 1)));
                    foreach (var target in selected)
                    {
                        node.Children.Add(target);
                        target.Parents.Add(node); // <<< This line ensures every node tracks its parents
                    }
                }
                foreach (var orphanCandidate in nextLayer)
                {
                    if (orphanCandidate.Parents.Count == 0)
                    {
                        // Connect it to the closest node in currentLayer
                        var closest = currentLayer.OrderBy(n => Mathf.Abs(currentLayer.IndexOf(n) - nextLayer.IndexOf(orphanCandidate))).First();
                        closest.Children.Add(orphanCandidate);
                        orphanCandidate.Parents.Add(closest);
                    }
                }

                // Ensure every node in the current layer has at least one child in the next layer
                foreach (var node in currentLayer)
                {
                    if (node.Children.Count == 0)
                    {
                        // Connect it to a random node in the next layer
                        var randomTarget = nextLayer[rng.Next(nextLayer.Count)];
                        node.Children.Add(randomTarget);
                        randomTarget.Parents.Add(node);
                    }
                }

            }

            return map;
        }

        private LevelType GetRandomNonBossType(int layerIndex)
        {

            // Weighting logic could go here (e.g., more stores after certain layers)
            //var possible = new List<LevelType> { LevelType.Combat, LevelType.Rest, LevelType.Store, LevelType.Event };
            var possible = new List<LevelType> { LevelType.Combat, LevelType.Store, LevelType.Rest };
            return possible[rng.Next(possible.Count)];
        }
    }
}
