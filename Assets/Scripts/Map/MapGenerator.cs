using Game.Scene;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Map
{
    public class MapGenerator
    {
        private System.Random rng = new();
        public List<MapLayer> GenerateActMap(ActConfig config)
        {
            var map = new List<MapLayer>();

            // Layer 0 — start
            var firstLayer = new MapLayer(0);
            var firstLayerNodes = new List<MapNode>();
            firstLayerNodes.Add(new MapNode(LevelType.Combat));
            firstLayer.Nodes = firstLayerNodes;
            map.Add(firstLayer);

            // Middle layers — type only
            for (int i = 0; i < config.middleLayers.Count; i++)
            {
                LayerConfig lc = config.middleLayers[i];
                MapLayer layer = new MapLayer(i + 1);
                var layerNodes = new List<MapNode>();

                for (int c = 0; c < lc.combatCount; c++)
                    layerNodes.Add(new MapNode(LevelType.Combat));

                for (int e = 0; e < lc.extraCount; e++)
                    layerNodes.Add(new MapNode(GetRandomExtraLevelType(config))); 

                layer.Nodes = layerNodes;
                map.Add(layer);
            }

            // Last layer — boss
            var bossLayerNodes = new List<MapNode>();
            bossLayerNodes.Add(new MapNode(LevelType.Boss));
            var bossLayer = new MapLayer(map.Count);
            bossLayer.Nodes = bossLayerNodes;
            map.Add(bossLayer);

            return map;
        }

        private LevelType GetRandomExtraLevelType(ActConfig config)
        {
            // Weighting logic could go here (e.g., more stores after certain layers)
            //var possible = new List<LevelType> { LevelType.Combat, LevelType.Rest, LevelType.Store, LevelType.Event };
            var possible = config.possibleExtraLevels;
            return possible[rng.Next(possible.Count)];
        }
    }
}
