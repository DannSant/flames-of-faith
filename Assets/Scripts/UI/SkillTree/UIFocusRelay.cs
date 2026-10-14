using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.SkillTree
{
    /// <summary>Raises <see cref="OnFocused"/> when the object is hovered or selected with the gamepad.</summary>
    public class UIFocusRelay : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        public event Action OnFocused;

        public void OnSelect(BaseEventData eventData) => OnFocused?.Invoke();

        public void OnPointerEnter(PointerEventData eventData) => OnFocused?.Invoke();
    }
}
