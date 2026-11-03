using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Chronicle
{
    /// <summary>One scene in the Pilgrim's Chronicle: its title on a button.</summary>
    [RequireComponent(typeof(Button))]
    public class ChronicleRowUI : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private TextMeshProUGUI label;

        private Action<ChronicleRowUI> onSelected;

        public void Setup(string title, Action onPressed, Action<ChronicleRowUI> onSelected)
        {
            label.text = title;
            this.onSelected = onSelected;
            GetComponent<Button>().onClick.AddListener(() => onPressed?.Invoke());
        }

        public void OnSelect(BaseEventData eventData) => onSelected?.Invoke(this);
    }
}
