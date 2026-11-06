using Game.Combat;
using Game.Overworld.Render;
using Game.Scene;
using System.Collections;
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
        [Tooltip("The pilgrim marker: its position is where the pilgrim's feet stand. Its child sprite plays the class's animator.")]
        [SerializeField] private Transform playerMarker;
        [Tooltip("Where the marker stands from the node's position (0 = the feet on the node).")]
        [SerializeField] private Vector2 playerOffset;
        [Tooltip("The marker's sprite. Found under the marker when empty.")]
        [SerializeField] private SpriteRenderer pilgrimSprite;

        [Header("Walking")]
        [Tooltip("How fast the pilgrim walks along a path to the next node (world units per second).")]
        [Min(0.01f)]
        [SerializeField] private float walkSpeed = 3f;
        [Tooltip("The longest a walk may take (seconds): longer paths are walked faster, so they don't drag.")]
        [Min(0.05f)]
        [SerializeField] private float maxWalkDuration = 1.2f;

        [Header("Node Sprites")]
        [SerializeField] private List<MapIconSprite> iconSprites = new();

        
        // etc...

        private MapRunController mapController;

        private Dictionary<string, OverworldNodeView> nodeViews = new();
        private Dictionary<LevelType, Sprite> iconSpritesDictionary = new();
        private List<OverworldEdgeView> edgeViews = new();

        private static readonly int DirectionXParam = Animator.StringToHash("DirectionX");
        private static readonly int DirectionYParam = Animator.StringToHash("DirectionY");
        private static readonly int MoveParam = Animator.StringToHash("Move");
        private Animator pilgrimAnimator;
        private Coroutine walk;
        private string markerNodeId;
        private Vector2 facing = new Vector2(0f, -1f);

        public Transform PlayerMarkerTransform => playerMarker;

        /// <summary>True while the pilgrim walks to a node: clicks and the gamepad navigator are ignored.</summary>
        public bool IsWalking => walk != null;

        private void Start()
        {
            mapController = MapRunController.Instance;

            mapController.OnNodeRevealed += OnNodeRevealed;
            mapController.OnCurrentNodeChanged += OnCurrentNodeChanged;
            mapController.OnActChanged += OnActChanged;
            mapController.OnRunMapInitialized += HandleMapInitialized;
            mapController.OnMapStateChanged += HandleMapStateChanged;

            //Generate icon sprite dictionary
            foreach (var iconSprite in iconSprites)
            {
                iconSpritesDictionary[iconSprite.levelType] = iconSprite.sprite;
            }

            if (mapController.IsInitialized)
            {
                HandleMapInitialized();
            }

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnPlayerDisabledOnMap += AttachCorruptedVisualToMarker;
                PlayerManager.Instance.OnMapPilgrimReady += ShowPilgrim;
            }
        }

        private void OnDisable()
        {
            // Disabling stops the walk's coroutine
            walk = null;
            mapController.OnNodeRevealed -= OnNodeRevealed;
            mapController.OnCurrentNodeChanged -= OnCurrentNodeChanged;
            mapController.OnActChanged -= OnActChanged;
            mapController.OnRunMapInitialized -= HandleMapInitialized;
            mapController.OnMapStateChanged -= HandleMapStateChanged;
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnPlayerDisabledOnMap -= AttachCorruptedVisualToMarker;
                PlayerManager.Instance.OnMapPilgrimReady -= ShowPilgrim;
            }
        }

        // --- The pilgrim ---------------------------------------------------------------------------------------

        private SpriteRenderer PilgrimSprite
        {
            get
            {
                if (pilgrimSprite == null && playerMarker != null) pilgrimSprite = playerMarker.GetComponentInChildren<SpriteRenderer>(true);
                return pilgrimSprite;
            }
        }

        /// <summary>
        /// Shows the played class's pilgrim on the marker: its animator (idle and walk), its size in a level, and
        /// its feet on the marker's position. Gameplay pushes it (PlayerManager.OnMapPilgrimReady).
        /// </summary>
        public void ShowPilgrim(MapPilgrimLook look)
        {
            var sprite = PilgrimSprite;
            if (look == null || sprite == null) return;

            if (look.sprite != null) sprite.sprite = look.sprite;
            sprite.transform.localScale = look.scale;
            sprite.transform.localPosition = -(Vector3)look.feetOffset;

            if (look.controller != null)
            {
                pilgrimAnimator = sprite.GetComponent<Animator>();
                if (pilgrimAnimator == null) pilgrimAnimator = sprite.gameObject.AddComponent<Animator>();
                pilgrimAnimator.applyRootMotion = false;
                pilgrimAnimator.runtimeAnimatorController = look.controller;
            }
            SetPilgrimAnimation(facing, false);
        }

        private void SetPilgrimAnimation(Vector2 direction, bool moving)
        {
            if (pilgrimAnimator == null || pilgrimAnimator.runtimeAnimatorController == null || !pilgrimAnimator.isActiveAndEnabled) return;
            pilgrimAnimator.SetFloat(DirectionXParam, direction.x);
            pilgrimAnimator.SetFloat(DirectionYParam, direction.y);
            pilgrimAnimator.SetBool(MoveParam, moving);
        }

        // Places the pilgrim on a node at once (arriving on the map, an act change, the developer cheats)
        private void PlacePilgrim(RunNode node)
        {
            StopWalk();
            if (node == null || playerMarker == null) return;
            playerMarker.position = node.worldPosition + playerOffset;
            markerNodeId = node.id;
            SetPilgrimAnimation(facing, false);
        }

        private void StopWalk()
        {
            if (walk == null) return;
            StopCoroutine(walk);
            walk = null;
        }

        // Walks along the drawn path to the node the map just moved to; the move itself (state, autosave) is
        // already done, so this is only how it looks.
        private void WalkTo(RunNode node)
        {
            var graph = mapController.CurrentAct;
            bool canWalk = !mapController.FreeTravel && markerNodeId != null && markerNodeId != node.id
                && graph != null && graph.nodes.ContainsKey(markerNodeId) && isActiveAndEnabled;
            var edge = canWalk ? FindEdge(markerNodeId, node.id) : null;
            if (edge == null)
            {
                PlacePilgrim(node);
                return;
            }

            StopWalk();
            var path = MapWalk.PathBetween(edge.Points, graph.nodes[markerNodeId].worldPosition, node.worldPosition);
            markerNodeId = node.id;
            walk = StartCoroutine(WalkRoutine(path, node));
        }

        private OverworldEdgeView FindEdge(string a, string b)
        {
            foreach (var edge in edgeViews)
            {
                if ((edge.FromNodeId == a && edge.ToNodeId == b) || (edge.FromNodeId == b && edge.ToNodeId == a)) return edge;
            }
            return null;
        }

        private IEnumerator WalkRoutine(List<Vector2> path, RunNode node)
        {
            float length = MapWalk.Length(path);
            float duration = MapWalk.Duration(length, walkSpeed, maxWalkDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float distance = length * Mathf.Clamp01(elapsed / duration);
                facing = MapWalk.Facing(MapWalk.DirectionAt(path, distance), facing);
                playerMarker.position = MapWalk.PointAt(path, distance) + playerOffset;
                SetPilgrimAnimation(facing, true);
                yield return null;
            }

            playerMarker.position = node.worldPosition + playerOffset;
            SetPilgrimAnimation(facing, false);
            walk = null;
        }

        // On the map the player is hidden where it spawned and the marker is what moves, so the
        // Corrupted VFX has to follow the marker. The player is respawned on the next scene load.
        private void AttachCorruptedVisualToMarker()
        {
            var grace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>();
            if (grace == null) return;
            CorruptedVisual.GetOrAdd(grace.gameObject).SetAnchor(playerMarker);
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
                    this,
                    node.levelData != null ? node.levelData.taintLevel : 0
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

        private void PositionPlayerMarker() => PlacePilgrim(mapController.CurrentNode);

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

        public OverworldNodeView GetNodeView(string nodeId)
        {
            return nodeViews.TryGetValue(nodeId, out var view) ? view : null;
        }

        public void OnNodeClicked(string nodeId)
        {
            // Mid-walk nothing happens: no second move, and the level is entered only once the pilgrim is there
            if (IsWalking) return;

            var currentNode = mapController.CurrentNode;

            if(nodeId == currentNode.id && currentNode.state == RunNodeState.Revealed)
            {
                var levelData = mapController.CurrentNode.levelData;
                MainSceneController.Instance.LoadGameplay(levelData);
                return;
            }

            mapController.TryMoveTo(nodeId);
        }

        private void OnNodeRevealed(RunNode node)
        {
            nodeViews[node.id].SetState(node.state);
        }

        private void OnCurrentNodeChanged(RunNode node)
        {
            WalkTo(node);
            RefreshVisuals();
        }

        // A developer cheat changed the map (revealed it, cleared a node, switched act).
        private void HandleMapStateChanged()
        {
            RefreshVisuals();
            PlacePilgrim(mapController.CurrentNode);
        }

        private void OnActChanged(int actIndex)
        {
            ClearMap();
            BuildMap();
            RefreshVisuals();
            PlacePilgrim(mapController.CurrentNode);
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