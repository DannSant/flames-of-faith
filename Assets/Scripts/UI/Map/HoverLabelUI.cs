using Game.Map;
using TMPro;
using UnityEngine;

namespace Game.UI.Map
{
    public class HoverLabelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI label;

        private void Start()
        {
            MapRenderer.Instance.OnNodeHoverEnter += ShowLabel;
            MapRenderer.Instance.OnNodeHoverExit += HideLabel;

            HideLabel();
        }

        private void OnDisable()
        {
            if (MapRenderer.Instance != null)
            {
                MapRenderer.Instance.OnNodeHoverEnter -= ShowLabel;
                MapRenderer.Instance.OnNodeHoverExit -= HideLabel;
            }
        }
        private void ShowLabel(string text)
        {
            label.text = text;
            panel.SetActive(true);
        }

        private void HideLabel()
        {
            panel.SetActive(false);
        }
    }

}