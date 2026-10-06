using TMPro;
using UnityEngine;

namespace Game.Overworld
{
    public class OverworldNodeView : MonoBehaviour
    {
        [Header("Taint")]
        [Tooltip("Optional. Shows the level's Taint Level next to the node.")]
        [SerializeField] private TMP_Text taintLabel;
        [Tooltip("{0} is replaced with the Taint Level.")]
        [SerializeField] private string taintFormat = "Taint {0}";
        [SerializeField] private bool hideTaintWhenZero = true;

        public string NodeId { get; private set; }

        private SpriteRenderer spriteRenderer;
        private OverworldMapRenderer mapRenderer;
        private bool isDisabled = false;

        private bool hasTaint;

        public void Initialize(
            string nodeId,
            Sprite sprite,
            OverworldMapRenderer renderer,
            int taintLevel = 0
        )
        {
            NodeId = nodeId;
            this.mapRenderer = renderer;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            isDisabled = false;

            hasTaint = taintLevel > 0 || !hideTaintWhenZero;
            if (taintLabel != null)
            {
                taintLabel.text = string.Format(taintFormat, taintLevel);
                taintLabel.gameObject.SetActive(hasTaint);
            }
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
                    spriteRenderer.color = new Color(1, 1, 1, 0.5f);
                    break;
                case RunNodeState.LockedHidden:
                    spriteRenderer.color = new Color(1, 1, 1, 0); // invisible
                    break;
                case RunNodeState.Blocked:
                    spriteRenderer.color = Color.black;
                    break;
            }

            // Hidden nodes keep their Taint secret too
            if (taintLabel != null)
            {
                taintLabel.gameObject.SetActive(hasTaint && state != RunNodeState.LockedHidden);
            }
        }

        private void OnMouseDown()
        {
            //if (isDisabled) return;
            //isDisabled = true; // Prevent multiple clicks until state is updated
            mapRenderer.OnNodeClicked(NodeId);
        }
    }

}