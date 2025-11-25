using Game.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Map
{
    public class MapNodeRaycaster : Singleton<MapNodeRaycaster>, ISceneCleanupHandler
    {
        [SerializeField] private Camera mapCamera;

        private List<MapNodeHoverDetector> nodes = new();
        private MapNodeHoverDetector currentHoveredNode;

        protected override void Awake()
        {
            base.Awake();
            if (mapCamera == null)
                mapCamera = Camera.main;
        }

        public void RegisterNode(MapNodeHoverDetector detector)
        {
            nodes.Add(detector);
        }

        private void Update()
        {
            DetectHover();
        }

        private void DetectHover()
        {
            if(mapCamera == null)
            {
                return;
            }
            Vector2 mousePos = mapCamera.ScreenToWorldPoint(Input.mousePosition);

            // Raycast to 2D colliders
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            MapNodeHoverDetector hoveredNode = hit.collider
                ? hit.collider.GetComponent<MapNodeHoverDetector>()
                : null;

            if (hoveredNode != currentHoveredNode)
            {
                // Exit old node
                if (currentHoveredNode != null)
                {
                    currentHoveredNode.SetHoverState(false);
                }

                // Enter new node
                if (hoveredNode != null)
                {
                    hoveredNode.SetHoverState(true);
                }

                currentHoveredNode = hoveredNode;
            }
        }

        public void Cleanup()
        {
            nodes.Clear();  // clear hover detectors
            currentHoveredNode = null;
            Destroy(gameObject);
        }
    }
}
