using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.Cutscenes
{
    /// <summary>Reports whether the pointer is holding this element down. Dragging off it counts as letting go.</summary>
    public class HoldButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsHeld { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) IsHeld = true;
        }

        public void OnPointerUp(PointerEventData eventData) => IsHeld = false;

        public void OnPointerExit(PointerEventData eventData) => IsHeld = false;

        private void OnDisable() => IsHeld = false;
    }
}
