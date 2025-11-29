using Game.Scene;
using System;
using UnityEngine;


namespace Game.Map
{
    public class MapNodeSprite : MonoBehaviour
    {
        private enum SpriteRendererState
        {
            Hidden,
            Visible,
            Beaten
        }
        [SerializeField] private SpriteRenderer spriteRenderer;
        private MapNode mapNode;
        private int layerIndex;
        private MapNodeHoverDetector hoverDetector;
        private Animator animator;

        public event System.Action<MapNode> OnHoverEnter;
        public event System.Action OnHoverExit;

        private Action<MapNode> onHoverEnterHandler;
        private Action onHoverExitHandler;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            hoverDetector = GetComponent<MapNodeHoverDetector>();
            hoverDetector.OnHoverEnter += HandleHoverEnter;
            hoverDetector.OnHoverExit += HandleHoverExit;
        }

        private void OnDisable()
        {
            hoverDetector.OnHoverEnter -= HandleHoverEnter;
            hoverDetector.OnHoverExit -= HandleHoverExit;
        }

        public void Setup(MapNode node, int layerIndex, int currentLayer)
        {
            this.mapNode = node;
            this.layerIndex = layerIndex;

            spriteRenderer.sprite = MapUtils.Instance.GetSprite(node.Type);

            if (layerIndex == currentLayer)
            {
                EnableRenderSprite(SpriteRendererState.Visible);
                EnableClickableCollider(true);
            }else if( layerIndex < currentLayer)
            {
                EnableRenderSprite(SpriteRendererState.Beaten);
                EnableClickableCollider(false);
            }
            else // future layers
            {
                EnableRenderSprite(SpriteRendererState.Hidden);
                EnableClickableCollider(false);
            }

            /*if (node.levelData != null && layerIndex == currentLayer)
            {
                EnableClickable(true);
            }
            else
            {
                EnableClickable(false);
            }*/
        }
        private void EnableRenderSprite(SpriteRendererState mode)
        {
            if(mode == SpriteRendererState.Hidden)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0.0f);
            }
            else if (mode == SpriteRendererState.Visible)
            {
                spriteRenderer.color = Color.white;
            }
            else if (mode == SpriteRendererState.Beaten)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            }
            
        }

        private void EnableClickableCollider(bool enabled)
        {
            var collider = GetComponent<Collider2D>();
            collider.enabled = enabled;
        }

        private void OnMouseDown()
        {
            if (mapNode.levelData == null) return; // locked
            LevelSelectionController.Instance.SelectNode(mapNode);
        }

        private void HandleHoverExit(MapNodeSprite sprite)
        {

            Highlight(false);
            OnHoverExit?.Invoke();
        }

        private void HandleHoverEnter(MapNodeSprite sprite)
        {
            //Debug.Log("HandleHoverEnter called");
            if (mapNode.levelData == null) return; // locked
            Highlight(true);
            OnHoverEnter?.Invoke(mapNode);
        }

        public void Highlight(bool value)
        {
            if (value)
            {
                animator.SetTrigger("HighlightOn");
            }
            else 
            {
                animator.SetTrigger("HighlightOff");
            }
        }

        public void RegisterCallbacks(Action<MapNode> enter, Action exit)
        {
            onHoverEnterHandler = enter;
            onHoverExitHandler = exit;

            OnHoverEnter += onHoverEnterHandler;
            OnHoverExit += onHoverExitHandler;
        }

        public void CleanupSubscriptions()
        {
            if (onHoverEnterHandler != null)
                OnHoverEnter -= onHoverEnterHandler;

            if (onHoverEnterHandler != null)
                OnHoverExit -= onHoverExitHandler;
        }
    }

}