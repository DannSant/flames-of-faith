using Game.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    public class MapRenderer : Singleton<MapRenderer>, ISceneCleanupHandler
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject layerContainerPrefab;
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private Transform mapRoot;
        [SerializeField] private Transform playerMarker;

        [Header("Layout Settings")]
        [SerializeField] private float layerSpacingY = 3f;
        [SerializeField] private float nodeSpacingX = 2.5f;

        // Reference to rendered objects (for cleanup)
        private List<GameObject> renderedLayers = new List<GameObject>();

        // Events
        public event Action<string> OnNodeHoverEnter;
        public event Action OnNodeHoverExit;

        protected override void Awake()
        {
            base.Awake();

        }
        /// <summary>
        /// Destroys all old map objects and re-renders the map.
        /// </summary>
        public void RenderMap(List<MapLayer> layers, int currentLayer)
        {
            CleanupMap();

            for (int i = 0; i < layers.Count; i++)
                RenderLayer(layers[i], i, currentLayer);

            PlacePlayerMarker(layers, currentLayer);
        }

        /// <summary>
        /// Render a single layer at height = layerIndex * spacing
        /// </summary>
        private void RenderLayer(MapLayer mapLayer, int layerIndex, int currentLayer)
        {
            GameObject layerGO = Instantiate(layerContainerPrefab, mapRoot);
            renderedLayers.Add(layerGO);

            float y = -layerIndex * layerSpacingY;

            var nodes = mapLayer.Nodes;
            float totalWidth = (nodes.Count - 1) * nodeSpacingX;

            for (int i = 0; i < nodes.Count; i++)
            {
                float x = -totalWidth / 2f + i * nodeSpacingX;
                Vector3 pos = new Vector3(x, y, 0);

                GameObject nodeGO = Instantiate(nodePrefab, layerGO.transform);
                nodeGO.transform.localPosition = pos;

                MapNodeSprite sprite = nodeGO.GetComponent<MapNodeSprite>();
                sprite.Setup(nodes[i], layerIndex, currentLayer);

                //subscribe node events
                sprite.RegisterCallbacks(HandleNodeHoverEnter, HandleNodeHoverExit);

                //register node for raycasting
                var hoverDetector = sprite.GetComponent<MapNodeHoverDetector>();
                MapNodeRaycaster.Instance.RegisterNode(hoverDetector);
            }
        }
        /// <summary>
        /// Place the marker next to the FIRST node of the current layer.
        /// </summary>
        private void PlacePlayerMarker(List<MapLayer> layers, int currentLayer)
        {
            if (currentLayer >= layers.Count) return;
            if (playerMarker == null) return;

            var firstNode = renderedLayers[currentLayer].transform.GetChild(0);
            Vector3 pos = firstNode.position + Vector3.left * 1f;

            playerMarker.position = pos;
        }
        private void CleanupMap()
        {
            foreach (var layer in renderedLayers)
                Destroy(layer);

            renderedLayers.Clear();
        }
        private void HandleNodeHoverEnter(MapNode node)
        {
            string title = node.Type.ToString();
            //Debug.Log($"Hover enter on node: {title}");
            OnNodeHoverEnter?.Invoke(title);
        }

        private void HandleNodeHoverExit()
        {
            OnNodeHoverExit?.Invoke();
        }

        public void Cleanup()
        {
            foreach (Transform child in mapRoot.transform)
            {
                var nodeSprite = child.GetComponentInChildren<MapNodeSprite>();
                if (nodeSprite != null)
                {
                    nodeSprite.CleanupSubscriptions(); // we'll add this method
                }

                Destroy(child.gameObject);
            }

            // Clear internal caches
            renderedLayers.Clear();
            Destroy(gameObject);
        }
    }

}