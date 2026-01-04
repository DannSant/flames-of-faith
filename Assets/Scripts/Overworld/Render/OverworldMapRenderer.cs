using Game.Map;
using Game.Overworld.Render;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    [System.Serializable]
    public struct MapIconSprite
    {
        public Sprite sprite;
        public LevelType levelType;
    }

    public class OverworldMapRenderer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private OverworldNodeView nodePrefab;
        [SerializeField] private OverworldEdgeView edgePrefab;

        [Header("Containers")]
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private Transform edgeContainer;

        [Header("Player reference")]
        [SerializeField] private Transform playerMarker;
        [SerializeField] private Vector2 playerOffset;

        [Header("Node Sprites")]
        [SerializeField] private List<MapIconSprite> iconSprites = new();

        
        // etc...

        private MapRunController mapController;

        private Dictionary<string, OverworldNodeView> nodeViews = new();
        private Dictionary<LevelType, Sprite> iconSpritesDictionary = new();
        private List<OverworldEdgeView> edgeViews = new();

        private void Start()
        {
            mapController = MapRunController.Instance;

            mapController.OnNodeRevealed += OnNodeRevealed;
            mapController.OnCurrentNodeChanged += OnCurrentNodeChanged;
            mapController.OnActChanged += OnActChanged;
            mapController.OnRunMapInitialized += HandleMapInitialized;

            //Generate icon sprite dictionary
            foreach (var iconSprite in iconSprites)
            {
                iconSpritesDictionary[iconSprite.levelType] = iconSprite.sprite;
            }

            if (mapController.IsInitialized)
            {
                HandleMapInitialized();
            }
        }

        private void HandleMapInitialized()
        {
            BuildMap();
            RefreshVisuals();
            PositionPlayerMarker();
        }

        private void BuildMap()
        {
           
            var graph = mapController.CurrentAct;

           

            // Nodes
            foreach (var node in graph.nodes.Values)
            {
                var view = Instantiate(nodePrefab, node.worldPosition, Quaternion.identity, nodeContainer);

                view.Initialize(
                    node.id,
                    GetSpriteForType(node.nodeType),
                    this
                );

                nodeViews[node.id] = view;
            }

            // Edges
            foreach (var node in graph.nodes.Values)
            {
                foreach (var edge in node.outgoingEdges)
                {
                    if (!edge.enabled)
                        continue;

                    var from = graph.nodes[edge.fromNodeId].worldPosition;
                    var to = graph.nodes[edge.toNodeId].worldPosition;

                    var edgeView = Instantiate(edgePrefab, edgeContainer);
                    edgeView.Initialize(edge.fromNodeId,edge.toNodeId,from,to);

                    edgeViews.Add(edgeView);
                }
            }
        }

        private void PositionPlayerMarker()
        {
            var currentNode = mapController.CurrentNode;
            playerMarker.position = currentNode.worldPosition + playerOffset;
        }

        private void RefreshVisuals()
        {
            var graph = mapController.CurrentAct;

            foreach (var node in graph.nodes.Values)
            {
                nodeViews[node.id].SetState(node.state);
            }

            RefreshEdgeVisibility();
        }

        private void RefreshEdgeVisibility()
        {
            var graph = mapController.CurrentAct;

            foreach (var edgeView in edgeViews)
            {
                var fromNode = graph.nodes[edgeView.FromNodeId];
                var toNode = graph.nodes[edgeView.ToNodeId];

                bool visible =
                    fromNode.state != RunNodeState.LockedHidden &&
                    toNode.state != RunNodeState.LockedHidden;

                edgeView.SetVisible(visible);
            }
        }

        public void OnNodeClicked(string nodeId)
        {
            if (mapController.TryMoveTo(nodeId))
            {
                Debug.Log($"Moved to node {nodeId}");
            }
            else
            {
                Debug.Log($"Cannot move to node {nodeId}");
            }
        }

        private void OnNodeRevealed(RunNode node)
        {
            nodeViews[node.id].SetState(node.state);
        }

        private void OnCurrentNodeChanged(RunNode node)
        {
            MovePlayerMarker(node.worldPosition);
            RefreshVisuals();
        }

        private void OnActChanged(int actIndex)
        {
            ClearMap();
            BuildMap();
            RefreshVisuals();
        }

        private void ClearMap()
        {
            foreach (var v in nodeViews.Values)
                Destroy(v.gameObject);
            foreach (var e in edgeViews)
                Destroy(e.gameObject);

            nodeViews.Clear();
            edgeViews.Clear();
        }

        private void MovePlayerMarker(Vector2 targetPos)
        {
            playerMarker.position = targetPos + playerOffset;
        }

        private Sprite GetSpriteForType(LevelType type)
        {
            if (iconSpritesDictionary.TryGetValue(type, out var sprite))
            {
                return sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite found for LevelType {type}, using default.");
                return iconSpritesDictionary[LevelType.Combat]; // default sprite
            }
        }

    }

}