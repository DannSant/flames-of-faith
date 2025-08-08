using Game.Map;
using Game.Scene;
using Game.UI.Map;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class LevelSelectorControllerUI : MonoBehaviour
    {
        [SerializeField] private GameObject nodesContainer;
        [SerializeField] private LevelLayerUI levelLayerPrefab;
        [SerializeField] private Transform connectionLineParent;
        [SerializeField] private GameObject bezierConnectorPrefab;
        [SerializeField] private RectTransform playerMapMarker;
        [SerializeField] private TextMeshProUGUI levelNametxt;

        private Dictionary<MapNode, LevelNodeUI> nodeUIMap = new();

        private void Start()
        {
            BuildLevelMap();
            StartCoroutine(DelayedInitializeMap());
        }

        private IEnumerator DelayedInitializeMap()
        {
            yield return null; // wait 1 frame
            InitializeMap();
        }

        private void InitializeMap()
        {
            
            var lastVisitedNode = LevelSelectionController.Instance.GetLastVisitedNode();

            if (lastVisitedNode == null)
            {
                EnableFirstNode();
            }
            else
            {
                EnableChildNodes(lastVisitedNode);
                MovePlayerMarkerToParent(lastVisitedNode);
            }

            levelNametxt.text = "";

        }

        private void EnableFirstNode()
        {
            var currentLayerNodes = LevelSelectionController.Instance.BuildLevelDataOfNodesInCurrentLayer();
            if (currentLayerNodes == null || currentLayerNodes.Count == 0)
            {
                Debug.LogWarning("No nodes found in the current layer.");
                return;
            }

            MapNode firstNode = currentLayerNodes[0];

            if (nodeUIMap.TryGetValue(firstNode, out LevelNodeUI firstNodeUI))
            {                
                //Setup first node
                firstNodeUI.Setup(firstNode.levelData);
            }
            else
            {
                Debug.LogWarning("First node UI not found in the map.");
            }
        }

        private void EnableChildNodes(MapNode parenNode)
        {
            LevelSelectionController.Instance.BuildLevelDataOfNodesInCurrentLayer();
            var childNodes = parenNode.Children;
            foreach (MapNode node in childNodes)
            {
                if (nodeUIMap.TryGetValue(node, out LevelNodeUI nodeUI))
                {                    
                    nodeUI.Setup(node.levelData);
                }
            }
        }

        private void MovePlayerMarkerToParent(MapNode parenNode)
        {
            if (nodeUIMap.TryGetValue(parenNode, out LevelNodeUI parentNodeUI))
            {
                RectTransform markerParent = playerMapMarker.parent as RectTransform;

                // Convert world position of the first node into local position of the marker's parent
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    markerParent,
                    RectTransformUtility.WorldToScreenPoint(null, parentNodeUI.transform.position),
                    null,
                    out localPoint
                );

                playerMapMarker.anchoredPosition = localPoint + new Vector2(50f, 0f);
            }
        }       

        private void BuildLevelMap()
        {
            nodeUIMap.Clear();
            for (int i = nodesContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(nodesContainer.transform.GetChild(i).gameObject);
            }

            var map = LevelSelectionController.Instance.GetCurrentActMap();

            //Generate map
            foreach (var row in map)
            {
                var levelLayerUI = Instantiate(levelLayerPrefab, nodesContainer.transform);
                List<LevelNodeUI> nodes = levelLayerUI.Initialize(row, SetLevelNameDisplayText);
                foreach (var node in nodes)
                {                    
                    nodeUIMap[node.MapNode] = node;
                }
            }

            // After all nodes are placed, create connections
            foreach (var pair in nodeUIMap)
            {
                MapNode parentNode = pair.Key;
                LevelNodeUI parentUI = pair.Value;

                foreach (MapNode child in parentNode.Children)
                {
                    if (nodeUIMap.TryGetValue(child, out LevelNodeUI childUI))
                    {
                        GameObject lineObj = Instantiate(bezierConnectorPrefab, connectionLineParent);
                        UIBezierConnector connector = lineObj.GetComponent<UIBezierConnector>();
                        connector.startPoint = parentUI.GetConnectorPoint();
                        connector.endPoint = childUI.GetConnectorPoint();
                       
                        connector.Refresh();
                    }
                }
            }            

        }

        private void SetLevelNameDisplayText(string levelName)
        {
            levelNametxt.text = levelName;
        }

      
    }

}