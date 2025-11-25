using Game.Map;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Map
{
    public class LevelLayerUI : MonoBehaviour
    {
        [SerializeField] private LevelNodeUI layerNodePrefab;

        public List<LevelNodeUI> Initialize(List<MapNode> nodes, Action<string> SetLevelNameDisplayText)
        {
            List<LevelNodeUI> nodesUI = new List<LevelNodeUI>();
            //Clear existing nodes
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            foreach(MapNode node in nodes)
            {
                /*if (node.Type!=LevelType.Boss && node.Children.Count <= 0) continue;
                var nodeUI = Instantiate(layerNodePrefab, transform);               
                var sprite = MapUtils.Instance.GetSprite(node.Type);
                
                nodeUI.Initialize(sprite, node.Type, node, SetLevelNameDisplayText);
                nodesUI.Add(nodeUI);*/
            }

            return nodesUI;

        }
    }
}
