using UnityEngine;

namespace Game.Overworld
{
    public class OverworldNodeView : MonoBehaviour
    {
        public string NodeId { get; private set; }

        private SpriteRenderer spriteRenderer;
        private OverworldMapRenderer mapRenderer;

        public void Initialize(
            string nodeId,
            Sprite sprite,
            OverworldMapRenderer renderer
        )
        {
            NodeId = nodeId;
            this.mapRenderer = renderer;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
        }

        public void SetState(RunNodeState state)
        {
            // Simple visual logic for now
            switch (state)
            {
                case RunNodeState.Revealed:
                    spriteRenderer.color = Color.white;
                    break;
                case RunNodeState.Cleared:
                    spriteRenderer.color = Color.gray;
                    break;
                case RunNodeState.LockedHidden:
                    spriteRenderer.color = new Color(1, 1, 1, 0); // invisible
                    break;
                case RunNodeState.Blocked:
                    spriteRenderer.color = Color.black;
                    break;
            }
        }

        private void OnMouseDown()
        {
           mapRenderer.OnNodeClicked(NodeId);
        }
    }

}